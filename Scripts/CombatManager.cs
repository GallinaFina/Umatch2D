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
        combatUI = FindFirstObjectByType<CombatUI>();
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

        if (validDefenders.Count > 0)
        {
            var movementUI = FindFirstObjectByType<MovementUI>();
            movementUI.StartUnitSelection(validDefenders, DefendWith);
        }
        else
        {
            ResolveCombat(null);
        }
    }

    private List<MonoBehaviour> GetValidDefenders(MonoBehaviour attacker)
    {
        List<MonoBehaviour> defenders = new List<MonoBehaviour>();
        Node attackerNode = null;
        bool isMeleeAttacker = false;

        if (attacker is Player player)
        {
            attackerNode = player.currentNode;
            isMeleeAttacker = player.combatType == Player.CombatType.Melee;
        }
        else if (attacker is Sidekick sidekick)
        {
            attackerNode = sidekick.currentNode;
            isMeleeAttacker = sidekick.combatType == Player.CombatType.Melee;
        }
        else if (attacker is Enemy enemy)
        {
            attackerNode = enemy.currentNode;
            isMeleeAttacker = enemy.combatType == Player.CombatType.Melee;
        }

        var potentialDefenders = GetPotentialDefenders(attacker);

        foreach (var defender in potentialDefenders)
        {
            Node defenderNode = null;
            if (defender is Player p) defenderNode = p.currentNode;
            else if (defender is Enemy e) defenderNode = e.currentNode;
            else if (defender is Sidekick s) defenderNode = s.currentNode;

            if (!isMeleeAttacker || attackerNode.IsConnectedTo(defenderNode))
            {
                defenders.Add(defender);
            }
        }

        return defenders;
    }

    private List<MonoBehaviour> GetPotentialDefenders(MonoBehaviour attacker)
    {
        List<MonoBehaviour> defenders = new List<MonoBehaviour>();

        if (attacker is Player || attacker is Sidekick sidekickA && sidekickA.owner is Player)
        {
            defenders.Add(FindFirstObjectByType<Enemy>());
            defenders.AddRange(FindObjectsByType<Sidekick>(FindObjectsSortMode.None)
                .Where(s => s.owner is Enemy));
        }
        else
        {
            defenders.Add(FindFirstObjectByType<Player>());
            defenders.AddRange(FindObjectsByType<Sidekick>(FindObjectsSortMode.None)
                .Where(s => s.owner is Player));
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

        // Reveal cards
        attackCard.isFaceDown = false;
        if (defenseCard != null)
        {
            defenseCard.isFaceDown = false;
        }

        // Update UI to show revealed cards
        combatUI.DisplayAttackerCard(attackCard);
        combatUI.DisplayDefenderCard(defenseCard);
        yield return new WaitForSeconds(1.5f);

        // Immediate effects
        combatUI.UpdatePhase(CombatUI.CombatPhase.ImmediateEffects);
        ResolveEffects(CardEffectTiming.Immediately);
        yield return new WaitForSeconds(1.5f);

        // During combat effects
        combatUI.UpdatePhase(CombatUI.CombatPhase.DuringCombatEffects);
        ResolveEffects(CardEffectTiming.DuringCombat);
        yield return new WaitForSeconds(1.5f);

        // Compare values and determine winner AFTER effects
        bool attackerWins = DetermineWinner(attackCard, defenseCard);

        if (attackerWins)
        {
            int damageDifference = attackCard.power - (defenseCard?.power ?? 0);
            ApplyDamage(defender, damageDifference);
            Debug.Log($"Defender takes {damageDifference} damage from power difference");
        }

        // After combat effects
        combatUI.UpdatePhase(CombatUI.CombatPhase.AfterCombatEffects);
        ResolveEffects(CardEffectTiming.AfterCombat);

        // Wait for both selection and movement to complete
        var movementUI = FindFirstObjectByType<MovementUI>();
        while (movementUI != null && (!movementUI.IsMovementComplete || movementUI.IsSelectingUnit))
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        EndCombat(attackerWins);
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
        if (target is Player player)
            player.TakeDamage(damage);
        else if (target is Enemy enemy)
            enemy.TakeDamage(damage);
        else if (target is Sidekick sidekick)
            sidekick.TakeDamage(damage);
    }

    private void EndCombat(bool attackerWins)
    {
        Debug.Log($"Combat ended. Attacker {(attackerWins ? "wins!" : "loses!")}");

        attackCard = null;
        defendCard = null;
        isWaitingForDefender = false;
        attacker = null;
        defender = null;

        combatUI.ShowCombatUI(false);

        // End the attacking action state for the appropriate unit
        if (attacker is Player player)
            player.actionManager.EndAction();
        else if (attacker is Enemy enemy)
            enemy.actionManager.EndAction();
        else if (attacker is Sidekick sidekick)
            sidekick.actionManager.EndAction();

        var turnManager = FindFirstObjectByType<TurnManager>();
        if (turnManager != null)
        {
            turnManager.PerformAction(TurnManager.ActionType.Attack);
        }
    }
}
