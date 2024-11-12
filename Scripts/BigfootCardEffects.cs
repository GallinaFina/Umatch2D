using UnityEngine;

public static class BigfootCardEffects
{
    public static void Regroup(Card card, MonoBehaviour source, bool wonCombat)
    {
        Debug.Log($"Executing Regroup effect. Won combat: {wonCombat}");
        int cardsToDraw = wonCombat ? 2 : 1;
        EffectManager.Instance.DrawCards(source, cardsToDraw);
    }

    public static void ItsJustYourImagination(Card card, MonoBehaviour source, bool wonCombat)
    {
        Debug.Log($"Executing It's just your imagination effect");
        var combatManager = Object.FindFirstObjectByType<CombatManager>();
        if (combatManager != null)
        {
            EffectManager.Instance.CancelEffects(combatManager.attackCard);
        }
    }

    public static void Feint(Card card, MonoBehaviour source, bool wonCombat)
    {
        Debug.Log($"Executing Feint effect");
        var combatManager = Object.FindFirstObjectByType<CombatManager>();
        if (combatManager != null)
        {
            EffectManager.Instance.CancelEffects(combatManager.attackCard);
        }
    }

    public static void Skirmish(Card card, MonoBehaviour source, bool wonCombat)
    {
        Debug.Log($"Executing Skirmish effect");
        if (source is Player player && player.currentNode != player.GetStartingNode())
        {
            EffectManager.Instance.ModifyCardPower(card, card.power + 2);
        }
    }

    public static void CrashThroughTrees(Card card, MonoBehaviour source, bool wonCombat)
    {
        Debug.Log($"Executing Crash Through Trees effect");
        if (source is Player player)
        {
            EffectManager.Instance.MovePlayer(player, 2, true);
        }
    }

    public static void Momentousshift(Card card, MonoBehaviour source, bool wonCombat)
    {
        Debug.Log($"Executing Momentous shift effect check for {source.gameObject.tag}");

        if (source is Player player)
        {
            Debug.Log($"Player current position: {player.currentNode.nodeName}");
            Debug.Log($"Player starting position: {player.GetStartingNode().nodeName}");
            if (player.currentNode != player.GetStartingNode())
            {
                Debug.Log($"Player Momentous shift power before: {card.power}");
                EffectManager.Instance.ModifyCardPower(card, card.power + 2);
                Debug.Log($"Player Momentous shift power after: {card.power}");
            }
            else
            {
                Debug.Log("Player is at starting position, no power boost");
            }
        }
        else if (source is Enemy enemy)
        {
            Debug.Log($"Enemy current position: {enemy.currentNode?.nodeName}");
            Node startingNode = enemy.GetStartingNode();

            if (startingNode == null)
            {
                Debug.Log("Enemy starting node not set, skipping effect");
                return;
            }

            Debug.Log($"Enemy starting position: {startingNode.nodeName}");
            if (enemy.currentNode != startingNode)
            {
                Debug.Log($"Enemy Momentous shift power before: {card.power}");
                EffectManager.Instance.ModifyCardPower(card, card.power + 2);
                Debug.Log($"Enemy Momentous shift power after: {card.power}");
            }
            else
            {
                Debug.Log("Enemy is at starting position, no power boost");
            }
        }
    }



}
