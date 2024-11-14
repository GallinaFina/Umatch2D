using UnityEngine;
using System.Collections;

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

        // Get the character's deck name from the source
        string deckName = "";
        if (source is Player player)
            deckName = player.deck.name;
        else if (source is Enemy enemy)
            deckName = enemy.deck.name;

        // Construct the effect library class name
        string effectLibrary = $"{deckName}CardEffects";
        Debug.Log($"Looking for method: {card.name.Replace(" ", "").Replace("'", "")} in {effectLibrary}");

        // Get the effect library type dynamically
        var libraryType = System.Type.GetType(effectLibrary);
        if (libraryType != null)
        {
            var method = libraryType.GetMethod(card.name.Replace(" ", "").Replace("'", ""));
            if (method != null)
            {
                Debug.Log($"Found method {card.name.Replace(" ", "")}, executing...");
                bool wonCombat = DetermineWonCombat(source);
                method.Invoke(null, new object[] { card, source, wonCombat });
            }
            else
            {
                Debug.LogError($"Method {card.name.Replace(" ", "")} not found in {effectLibrary}");
            }
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

    public void MovePlayer(MonoBehaviour unit, int maxSpaces, bool throughUnits = false)
    {
        Debug.Log($"Setting up movement for {unit.gameObject.name} with {maxSpaces} spaces");

        // Store original action state
        var originalAction = unit is Player player ?
            player.actionManager.currentAction :
            ((Enemy)unit).actionManager.currentAction;
        Debug.Log($"Original action state: {originalAction}");

        // Set up movement state
        if (unit is Player selectedPlayer)
        {
            selectedPlayer.actionManager.StartAction(ActionState.Maneuvering);
            selectedPlayer.movement = maxSpaces;
            selectedPlayer.HighlightNodesInRange();
            Debug.Log($"Player movement set to: {selectedPlayer.movement}, Action state: {selectedPlayer.actionManager.currentAction}");
        }
        else if (unit is Enemy selectedEnemy)
        {
            selectedEnemy.actionManager.StartAction(ActionState.Maneuvering);
            selectedEnemy.movement = maxSpaces;
            selectedEnemy.HighlightNodesInRange();
            Debug.Log($"Enemy movement set to: {selectedEnemy.movement}, Action state: {selectedEnemy.actionManager.currentAction}");
        }

        Debug.Log($"Movement setup completed for {unit.gameObject.name}");
    }


    private IEnumerator HandleForcedMovement(MonoBehaviour unit, ActionState originalAction)
    {
        if (unit is Player player)
        {
            player.HighlightNodesInRange();
            while (player.movement > 0)
            {
                yield return null;
            }
            player.actionManager.StartAction(originalAction);
        }
        else if (unit is Enemy enemy)
        {
            enemy.HighlightNodesInRange();
            while (enemy.movement > 0)
            {
                yield return null;
            }
            enemy.actionManager.StartAction(originalAction);
        }
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
