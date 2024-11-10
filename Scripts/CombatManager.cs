using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public Player player;
    public Enemy enemy;
    private Card attackCard;
    private Card defendCard;
    private bool isWaitingForDefender = false;

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
        }

        ResolveCombat(defendCard);
    }

    private void ResolveCombat(Card defenseCard)
    {
        Debug.Log("Beginning combat resolution");

        // Reveal cards
        attackCard.isFaceDown = false;
        if (defenseCard != null)
        {
            defenseCard.isFaceDown = false;
        }

        // Resolve IMMEDIATELY effects
        ResolveEffects(CardEffectTiming.Immediately);

        // Resolve DURING COMBAT effects
        ResolveEffects(CardEffectTiming.DuringCombat);

        // Compare values and determine winner
        bool attackerWins = DetermineWinner(attackCard, defenseCard);

        // Resolve AFTER COMBAT effects
        ResolveEffects(CardEffectTiming.AfterCombat);

        EndCombat(attackerWins);
    }

    private void ResolveEffects(CardEffectTiming timing)
    {
        if (defendCard?.effectTiming == timing)
        {
            Debug.Log($"Triggering defender's {timing} effect");
            defendCard.OnEffectTriggered?.Invoke();
        }

        if (attackCard.effectTiming == timing)
        {
            Debug.Log($"Triggering attacker's {timing} effect");
            attackCard.OnEffectTriggered?.Invoke();
        }
    }

    private bool DetermineWinner(Card attackCard, Card defenseCard)
    {
        if (defenseCard == null) return true; // Undefended attacks always succeed

        // Tie goes to defender
        return attackCard.power > defenseCard.power;
    }

    private void EndCombat(bool attackerWins)
    {
        Debug.Log($"Combat ended. Attacker {(attackerWins ? "wins!" : "loses!")}");

        attackCard = null;
        defendCard = null;
        isWaitingForDefender = false;

        var turnManager = FindFirstObjectByType<TurnManager>();
        if (turnManager != null)
        {
            turnManager.PerformAction(TurnManager.ActionType.Attack);
        }
    }
}
