using UnityEngine;
using System.Linq;

public class CombatManager : MonoBehaviour
{
    public Player player;
    public Enemy enemy;
    private TurnManager turnManager;

    void Start()
    {
        turnManager = FindFirstObjectByType<TurnManager>();
    }

    public void InitiateAttack(Card selectedCard)
    {
        if (turnManager == null || !turnManager.CanPerformAction())
        {
            Debug.LogError("Cannot perform action: TurnManager is null or no actions remaining.");
            return;
        }

        if (selectedCard.cardType != CardType.Attack && selectedCard.cardType != CardType.Versatile)
        {
            Debug.LogError("Invalid card type selected for attack.");
            return;
        }

        Debug.Log("InitiateAttack called with selected card: " + selectedCard.name);

        if (CanAttack(player, enemy))
        {
            Debug.Log("Can attack enemy");
            DefendAgainst(selectedCard, player, enemy);
            player.DiscardCard(selectedCard);

            // Consume action
            turnManager.PerformAction(TurnManager.ActionType.Attack);
        }
        else
        {
            Debug.Log("Cannot attack enemy.");
        }
    }

    public bool CanAttack(Player attacker, Enemy enemy)
    {
        if (attacker.combatType == Player.CombatType.Melee)
        {
            return attacker.currentNode.IsConnectedTo(enemy.currentNode);
        }
        else if (attacker.combatType == Player.CombatType.Ranged)
        {
            return attacker.currentNode.zones.Intersect(enemy.currentNode.zones).Any();
        }
        return false;
    }

    public void DefendAgainst(Card attackCard, Player attacker, Enemy defender)
    {
        Card defenseCard = SelectDefenseCard(defender);
        Debug.Log(defender.gameObject.tag + " defending with " + defenseCard.name);
        ResolveCombat(attackCard, defenseCard, attacker, defender);
    }

    public void ResolveCombat(Card attackCard, Card defenseCard, Player attacker, Enemy defender)
    {
        Debug.Log("Attacker: " + attacker.gameObject.tag + " using " + attackCard.name + " (Power: " + attackCard.power + ")");
        Debug.Log("Defender: " + defender.gameObject.tag + " using " + defenseCard.name + " (Power: " + defenseCard.power + ")");

        if (attackCard.power > defenseCard.power)
        {
            Debug.Log(attacker.gameObject.tag + " wins the combat");
            // Logic for attacker winning the combat
        }
        else if (attackCard.power < defenseCard.power)
        {
            Debug.Log(defender.gameObject.tag + " wins the combat");
            // Logic for defender winning the combat
        }
        else
        {
            Debug.Log("Combat is a tie, defender wins");
            // Logic for ties, where defender wins
        }
    }

    public Card SelectDefenseCard(Enemy enemy)
    {
        return enemy.hand.FirstOrDefault(card => card.cardType == CardType.Defend || card.cardType == CardType.Versatile);
    }
}
