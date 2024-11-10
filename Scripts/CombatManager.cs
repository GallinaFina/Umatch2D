using UnityEngine;
using System.Collections;

public class CombatManager : MonoBehaviour
{
    public Player player;
    public Enemy enemy;
    private Card attackCard;
    private Card defendCard;
    private bool isWaitingForDefender = false;
    private CombatUI combatUI;

    void Start()
    {
        combatUI = FindFirstObjectByType<CombatUI>();
    }

    public void InitiateAttack(Card selectedCard)
    {
        if (selectedCard.cardType != CardType.Attack && selectedCard.cardType != CardType.Versatile)
        {
            Debug.LogError("Cannot attack with non-attack card type");
            return;
        }

        attackCard = selectedCard;
        attackCard.isFaceDown = true;
        isWaitingForDefender = true;
        Debug.Log($"Attack initiated with {attackCard.name}");

        combatUI.ShowCombatUI(true);
        combatUI.UpdatePhase(CombatUI.CombatPhase.AttackerSelection);
        combatUI.DisplayAttackerCard(attackCard);

        RequestEnemyDefense();
    }

    private void RequestEnemyDefense()
    {
        var defendCard = enemy.SelectDefenseCard();
        if (defendCard != null)
        {
            DefendWith(defendCard);
        }
        else
        {
            ResolveCombat(null);
        }
    }

    public void DefendWith(Card selectedCard)
    {
        if (!isWaitingForDefender) return;

        if (selectedCard != null &&
            selectedCard.cardType != CardType.Defense &&
            selectedCard.cardType != CardType.Versatile)
        {
            Debug.LogError("Cannot defend with non-defense card type");
            return;
        }

        defendCard = selectedCard;
        if (defendCard != null)
        {
            defendCard.isFaceDown = true;
            Debug.Log($"Defense declared with {defendCard.name}");

            combatUI.UpdatePhase(CombatUI.CombatPhase.DefenderSelection);
            combatUI.DisplayDefenderCard(defendCard);
        }

        ResolveCombat(defendCard);
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

        // Compare values and determine winner
        bool attackerWins = DetermineWinner(attackCard, defenseCard);

        // After combat effects
        combatUI.UpdatePhase(CombatUI.CombatPhase.AfterCombatEffects);
        ResolveEffects(CardEffectTiming.AfterCombat);
        yield return new WaitForSeconds(2f);

        EndCombat(attackerWins);
    }

    private void ResolveEffects(CardEffectTiming timing)
    {
        if (defendCard?.effectTiming == timing)
        {
            Debug.Log($"Triggering defender's {timing} effect");
            defendCard.TriggerEffect(enemy, player);
        }

        if (attackCard.effectTiming == timing)
        {
            Debug.Log($"Triggering attacker's {timing} effect");
            attackCard.TriggerEffect(player, enemy);
        }
    }

    private bool DetermineWinner(Card attackCard, Card defenseCard)
    {
        if (defenseCard == null) return true;
        return attackCard.power > defenseCard.power;
    }

    private void EndCombat(bool attackerWins)
    {
        Debug.Log($"Combat ended. Attacker {(attackerWins ? "wins!" : "loses!")}");

        attackCard = null;
        defendCard = null;
        isWaitingForDefender = false;

        combatUI.ShowCombatUI(false);

        // End the attacking action state
        player.actionManager.EndAction();

        var turnManager = FindFirstObjectByType<TurnManager>();
        if (turnManager != null)
        {
            turnManager.PerformAction(TurnManager.ActionType.Attack);
        }
    }
}
