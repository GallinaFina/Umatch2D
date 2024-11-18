using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CombatManager : MonoBehaviour
{
    public Player player;
    public Enemy enemy;
    private MonoBehaviour attacker;
    private MonoBehaviour defender;
    public Card attackCard;
    public Card defendCard;
    private bool isWaitingForDefender = false;
    private CombatUI combatUI;

    void Start()
    {
        ServiceLocator.Instance.RegisterService(this);
        combatUI = ServiceLocator.Instance.CombatUI;
        if (combatUI == null)
        {
            Debug.LogError("CombatUI not found in ServiceLocator");
        }
    }

    public void InitiateAttack(MonoBehaviour attackingUnit, Card selectedCard)
    {
        if (selectedCard.cardType != CardType.Attack && selectedCard.cardType != CardType.Versatile)
        {
            Debug.LogError("Cannot attack with non-attack card type");
            return;
        }

        attacker = attackingUnit;
        attackCard = selectedCard;
        attackCard.isFaceDown = true;
        isWaitingForDefender = true;
        Debug.Log($"Attack initiated by {attacker.name} with {attackCard.name}");

        combatUI.ShowCombatUI(true);
        combatUI.UpdatePhase(CombatUI.CombatPhase.AttackerSelection);
        combatUI.DisplayAttackerCard(attackCard);

        RequestDefense();
    }

    private void RequestDefense()
    {
        List<MonoBehaviour> validDefenders = GetValidDefenders(attacker);
        Debug.Log($"Found {validDefenders.Count} valid defenders");

        if (validDefenders.Count > 0)
        {
            ServiceLocator.Instance.MovementUI.StartUnitSelection(validDefenders, DefendWith);
        }
        else
        {
            Debug.Log("No valid defenders found, resolving combat without defense");
            ResolveCombat(null);
        }
    }

    private List<MonoBehaviour> GetValidDefenders(MonoBehaviour attacker)
    {
        List<MonoBehaviour> defenders = new List<MonoBehaviour>();
        Node attackerNode = null;
        bool isMeleeAttacker = false;

        Debug.Log($"Checking valid defenders for attacker: {attacker.name}");

        if (attacker is Player player)
        {
            attackerNode = player.currentNode;
            isMeleeAttacker = player.combatType == CombatType.Melee;
        }
        else if (attacker is Sidekick sidekick)
        {
            attackerNode = sidekick.currentNode;
            isMeleeAttacker = sidekick.combatType == CombatType.Melee;
        }
        else if (attacker is Enemy enemy)
        {
            attackerNode = enemy.currentNode;
            isMeleeAttacker = enemy.combatType == CombatType.Melee;
        }

        Debug.Log($"Attacker is melee: {isMeleeAttacker}, at node: {attackerNode?.nodeName}");

        var potentialDefenders = GetPotentialDefenders(attacker);

        foreach (var defender in potentialDefenders)
        {
            Node defenderNode = null;
            if (defender is Player p) defenderNode = p.currentNode;
            else if (defender is Enemy e) defenderNode = e.currentNode;
            else if (defender is Sidekick s) defenderNode = s.currentNode;

            Debug.Log($"Checking defender {defender.name} at node: {defenderNode?.nodeName}");
            Debug.Log($"Is connected: {!isMeleeAttacker || attackerNode.IsConnectedTo(defenderNode)}");

            if (!isMeleeAttacker || attackerNode.IsConnectedTo(defenderNode))
            {
                defenders.Add(defender);
                Debug.Log($"Added valid defender: {defender.name}");
            }
        }

        return defenders;
    }

    private List<MonoBehaviour> GetPotentialDefenders(MonoBehaviour attacker)
    {
        List<MonoBehaviour> defenders = new List<MonoBehaviour>();
        var game = ServiceLocator.Instance.GameManager;

        Debug.Log($"Getting potential defenders for attacker: {attacker.name}");

        if (attacker is Player || (attacker is Sidekick sidekickA && sidekickA.owner is Player))
        {
            defenders.Add(game.enemy);
            var enemySidekick = game.GetSidekickForOwner(game.enemy);
            Debug.Log($"Enemy sidekick found: {enemySidekick != null}");
            if (enemySidekick != null)
            {
                defenders.Add(enemySidekick);
            }
        }
        else
        {
            defenders.Add(game.player);
            defenders.AddRange(game.player.sidekicks);
        }

        Debug.Log($"Total potential defenders found: {defenders.Count}");
        foreach (var defender in defenders)
        {
            Debug.Log($"Potential defender: {defender.name}");
        }

        return defenders;
    }

    public void DefendWith(MonoBehaviour defendingUnit)
    {
        if (!isWaitingForDefender) return;

        defender = defendingUnit;
        Card selectedDefenseCard = null;

        if (defender is Player player)
            selectedDefenseCard = player.SelectCardForDefense();
        else if (defender is Enemy enemy)
            selectedDefenseCard = enemy.SelectDefenseCard();
        else if (defender is Sidekick sidekick)
            selectedDefenseCard = sidekick.SelectCardForDefense();

        if (selectedDefenseCard != null)
        {
            defendCard = selectedDefenseCard;
            defendCard.isFaceDown = true;
            Debug.Log($"Defense declared by {defender.name} with {defendCard.name}");

            combatUI.UpdatePhase(CombatUI.CombatPhase.DefenderSelection);
            combatUI.DisplayDefenderCard(defendCard);
        }

        ResolveCombat(selectedDefenseCard);
    }

    private void ResolveCombat(Card defenseCard)
    {
        StartCoroutine(ResolveCombatSequence(defenseCard));
    }

    private IEnumerator ResolveCombatSequence(Card defenseCard)
    {
        Debug.Log("Beginning combat resolution");

        attackCard.isFaceDown = false;
        if (defenseCard != null)
        {
            defenseCard.isFaceDown = false;
        }

        combatUI.DisplayAttackerCard(attackCard);
        combatUI.DisplayDefenderCard(defenseCard);
        yield return new WaitForSeconds(1.5f);

        combatUI.UpdatePhase(CombatUI.CombatPhase.ImmediateEffects);
        ResolveEffects(CardEffectTiming.Immediately);
        yield return new WaitForSeconds(1.5f);

        combatUI.UpdatePhase(CombatUI.CombatPhase.DuringCombatEffects);
        ResolveEffects(CardEffectTiming.DuringCombat);
        yield return new WaitForSeconds(1.5f);

        bool attackerWins = DetermineWinner(attackCard, defenseCard);

        if (attackerWins)
        {
            int damageDifference = attackCard.power - (defenseCard?.power ?? 0);
            ApplyDamage(defender, damageDifference);
            Debug.Log($"Defender takes {damageDifference} damage from power difference");
        }

        combatUI.UpdatePhase(CombatUI.CombatPhase.AfterCombatEffects);
        ResolveEffects(CardEffectTiming.AfterCombat);
        yield return new WaitForSeconds(1.5f);

        Debug.Log("Combat sequence complete, ending combat...");
        EndCombat(attackerWins);
    }

    private void EndCombat(bool attackerWins)
    {
        Debug.Log($"Combat ended. Attacker {(attackerWins ? "wins!" : "loses!")}");

        if (attacker is BaseUnit baseUnit)
        {
            baseUnit.actionManager.EndAction();
        }

        attackCard = null;
        defendCard = null;
        isWaitingForDefender = false;
        attacker = null;
        defender = null;

        combatUI.ShowCombatUI(false);
    }

    private void ResolveEffects(CardEffectTiming timing)
    {
        if (defendCard?.effectTiming == timing)
        {
            defendCard.TriggerEffect(defender, attacker);
        }

        if (attackCard.effectTiming == timing)
        {
            attackCard.TriggerEffect(attacker, defender);
        }
    }

    public bool DetermineWinner(Card attackCard, Card defenseCard)
    {
        if (defenseCard == null) return true;
        return attackCard.power > defenseCard.power;
    }

    private void ApplyDamage(MonoBehaviour target, int damage)
    {
        if (target is BaseUnit baseUnit)
        {
            baseUnit.TakeDamage(damage);
        }
    }

}
