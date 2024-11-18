using UnityEngine;
using System.Linq;
using System.Collections;

public static class BigfootCardEffects
{
    public static void Regroup(Card card, MonoBehaviour source, bool wonCombat)
    {
        Debug.Log($"Executing Regroup effect. Won combat: {wonCombat}");
        int cardsToDraw = wonCombat ? 2 : 1;
        ServiceLocator.Instance.EffectManager.DrawCards(source, cardsToDraw);
    }

    public static void Itsjustyourimagination(Card card, MonoBehaviour source, bool wonCombat)
    {
        Debug.Log($"Executing It's just your imagination effect");
        var combatManager = ServiceLocator.Instance.CombatManager;
        if (combatManager != null)
        {
            ServiceLocator.Instance.EffectManager.CancelEffects(combatManager.attackCard);
        }
    }

    public static void Feint(Card card, MonoBehaviour source, bool wonCombat)
    {
        Debug.Log($"Executing Feint effect");
        var combatManager = ServiceLocator.Instance.CombatManager;
        if (combatManager != null)
        {
            var targetCard = source is Player ? combatManager.defendCard : combatManager.attackCard;
            ServiceLocator.Instance.EffectManager.CancelEffects(targetCard);
        }
    }

    public static void Skirmish(Card card, MonoBehaviour source, bool wonCombat)
    {
        Debug.Log($"Executing Skirmish effect. Combat won: {wonCombat}");
        if (wonCombat)
        {
            var combatManager = ServiceLocator.Instance.CombatManager;
            var movementUI = ServiceLocator.Instance.MovementUI;

            if (combatManager != null && movementUI != null)
            {
                MonoBehaviour[] selectableUnits = { combatManager.player, combatManager.enemy };
                movementUI.StartUnitSelection(selectableUnits.ToList(), (selected) =>
                    ServiceLocator.Instance.EffectManager.MovePlayer(selected, 2, true));
            }
        }
    }

    public static void Crashthroughthetrees(Card card, MonoBehaviour source, bool wonCombat)
    {
        Debug.Log($"Executing Crash Through Trees effect");
        if (source is Player player)
        {
            var effectManager = ServiceLocator.Instance.EffectManager;
            effectManager.StartEffect();
            var movementUI = ServiceLocator.Instance.MovementUI;

            effectManager.MovePlayer(player, 5, true);

            ServiceLocator.Instance.GameManager.StartCoroutine(WaitForMovement(movementUI));
        }
    }

    private static IEnumerator WaitForMovement(MovementUI movementUI)
    {
        while (!movementUI.IsMovementComplete)
        {
            yield return null;
        }
        ServiceLocator.Instance.EffectManager.CompleteEffect();
    }

    public static void MomentousShift(Card card, MonoBehaviour source, bool wonCombat)
    {
        Debug.Log($"Executing Momentous shift effect check for {source.gameObject.tag}");

        if (source is Player player)
        {
            Debug.Log($"Player current position: {player.currentNode.nodeName}");
            Debug.Log($"Player starting position: {player.GetStartingNode().nodeName}");
            if (player.currentNode != player.GetStartingNode())
            {
                Debug.Log($"Player Momentous shift power before: {card.power}");
                ServiceLocator.Instance.EffectManager.ModifyCardPower(card, card.power + 2);
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
                ServiceLocator.Instance.EffectManager.ModifyCardPower(card, card.power + 2);
                Debug.Log($"Enemy Momentous shift power after: {card.power}");
            }
            else
            {
                Debug.Log("Enemy is at starting position, no power boost");
            }
        }
    }
}
