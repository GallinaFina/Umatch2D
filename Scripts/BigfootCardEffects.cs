using UnityEngine;

public static class BigfootCardEffects
{
    public static void Regroup(Card card, MonoBehaviour source, bool wonCombat)
    {
        Debug.Log($"Executing Regroup effect. Won combat: {wonCombat}");
        
        if (source is Player player)
        {
            if (wonCombat)
            {
                EffectManager.Instance.DrawCards(player, 2);
            }
            else
            {
                EffectManager.Instance.DrawCards(player, 1);
            }
        }
        else if (source is Enemy enemy)
        {
            if (wonCombat)
            {
                enemy.DrawCard();
                enemy.DrawCard();
            }
            else
            {
                enemy.DrawCard();
            }
        }
    }

    public static void ItsJustYourImagination(Card card, Player source, bool wonCombat)
    {
        Debug.Log($"Executing It's just your imagination effect");
        var combatManager = Object.FindFirstObjectByType<CombatManager>();
        if (combatManager != null)
        {
            EffectManager.Instance.CancelEffects(combatManager.attackCard);
        }
    }

    public static void Feint(Card card, Player source, bool wonCombat)
    {
        Debug.Log($"Executing Feint effect");
        var combatManager = Object.FindFirstObjectByType<CombatManager>();
        if (combatManager != null)
        {
            EffectManager.Instance.CancelEffects(combatManager.attackCard);
        }
    }

    public static void Skirmish(Card card, Player source, bool wonCombat)
    {
        Debug.Log($"Executing Skirmish effect");
        if (source.currentNode != source.GetStartingNode())
        {
            EffectManager.Instance.ModifyCardPower(card, card.power + 2);
        }
    }

    public static void CrashThroughTrees(Card card, Player source, bool wonCombat)
    {
        Debug.Log($"Executing Crash Through Trees effect");
        EffectManager.Instance.MovePlayer(source, 2, true);
    }

    public static void QuickStrike(Card card, Player source, bool wonCombat)
    {
        Debug.Log($"Executing Quick Strike effect");
        var turnManager = Object.FindFirstObjectByType<TurnManager>();
        if (turnManager != null)
        {
            EffectManager.Instance.GainAction(turnManager);
        }
    }
}
