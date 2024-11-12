using UnityEngine;

public class EffectManager : MonoBehaviour
{
    private static EffectManager instance;
    public static EffectManager Instance => instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void ResolveCardEffect(Card card, Player source, Enemy target)
    {
        Debug.Log($"Resolving effect for {card.name} with timing {card.effectTiming}");

        bool wonCombat = false;
        var combatManager = FindFirstObjectByType<CombatManager>();
        if (combatManager != null)
        {
            if (source == combatManager.player)
            {
                wonCombat = combatManager.DetermineWinner(combatManager.attackCard, combatManager.defendCard);
            }
            else
            {
                wonCombat = !combatManager.DetermineWinner(combatManager.attackCard, combatManager.defendCard);
            }
        }

        string methodName = card.name.Replace(" ", "");
        var method = typeof(BigfootCardEffects).GetMethod(methodName);
        if (method != null)
        {
            method.Invoke(null, new object[] { card, source, wonCombat });
        }
    }


    public void ResolveCardEffect(Card card, MonoBehaviour source, MonoBehaviour target)
    {
        Debug.Log($"Resolving effect for {card.name} with timing {card.effectTiming}");

        string methodName = card.name.Replace(" ", "");
        Debug.Log($"Looking for method: {methodName}");

        var method = typeof(BigfootCardEffects).GetMethod(methodName);
        if (method != null)
        {
            Debug.Log($"Found method {methodName}, executing...");
            bool wonCombat = DetermineWonCombat(source);
            method.Invoke(null, new object[] { card, source, wonCombat });
        }
        else
        {
            Debug.LogError($"Method {methodName} not found in BigfootCardEffects");
        }
    }

    private bool DetermineWonCombat(MonoBehaviour source)
    {
        var combatManager = FindFirstObjectByType<CombatManager>();
        if (combatManager != null)
        {
            if (source is Player)
            {
                return combatManager.DetermineWinner(combatManager.attackCard, combatManager.defendCard);
            }
            else
            {
                return !combatManager.DetermineWinner(combatManager.attackCard, combatManager.defendCard);
            }
        }
        return false;
    }



    public void DrawCards(MonoBehaviour target, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (target is Player p)
                p.DrawCard();
            else if (target is Enemy enemy)
                enemy.DrawCard();
        }

        if (target is Player player)
        {
            var handDisplay = FindFirstObjectByType<HandDisplay>();
            if (handDisplay != null)
            {
                handDisplay.DisplayHand(player.hand, player.SelectCard);
            }
        }
    }


    public void CancelEffects(Card targetCard)
    {
        Debug.Log($"Canceling effects on {targetCard.name}");
        targetCard.effectTiming = CardEffectTiming.None;
    }

    public void MovePlayer(Player player, int maxSpaces, bool throughUnits = false)
    {
        // Instead of directly setting currentAction, we should use ActionManager's methods
        var originalAction = player.actionManager.currentAction;
        player.actionManager.StartAction(ActionState.Maneuvering);
        player.movement = maxSpaces;
        player.MoveToNode(player.currentNode, throughUnits);
        player.actionManager.StartAction(originalAction);
    }


    public void ModifyCardPower(Card card, int newPower)
    {
        card.power = newPower;
    }

    public void DealBonusDamage(Player source, Enemy target, int amount)
    {
        target.TakeDamage(amount);
    }

    public void GainAction(TurnManager turnManager)
    {
        // Logic to add an action to the current turn
    }
}
