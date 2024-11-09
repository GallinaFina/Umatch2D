using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Player : MonoBehaviour
{
    [SerializeField]
    public List<Card> hand;
    private Deck deck;
    public int movement;
    public Node currentNode;

    public enum CombatType { Melee, Ranged }
    public CombatType combatType;

    private bool canBoost = false;

    public bool CanBoost => canBoost;

    void Start()
    {
        // Initialize other components if necessary
    }

    public void Initialize(Deck chosenDeck, CombatType type)
    {
        deck = chosenDeck;
        combatType = type;
    }

    public void DrawCard()
    {
        if (deck.cards.Count == 0)
        {
            Debug.LogError("Deck is empty, cannot draw a card.");
            return;
        }

        if (hand.Count < 7)
        {
            int index = Random.Range(0, deck.cards.Count);
            Card drawnCard = deck.cards[index];
            deck.cards.RemoveAt(index);
            hand.Add(drawnCard);
            Debug.Log("Drew: " + drawnCard.name + " for " + gameObject.tag);
        }
        else
        {
            Debug.Log(gameObject.tag + " hand is full!");
        }
    }

    public void Maneuver()
    {
        DrawCard();
        movement = deck.baseMovement;
        canBoost = true;
        Debug.Log("Base movement for " + gameObject.tag + ": " + movement);

        Debug.Log("Hand before display update: " + string.Join(", ", hand.Select(card => card.name)));

        var handDisplay = FindFirstObjectByType<HandDisplay>();
        if (handDisplay != null && CompareTag("Player"))
        {
            var game = FindFirstObjectByType<Game>();
            Debug.Log("HandDisplay and Game scripts found.");
            handDisplay.DisplayHand(hand, SelectCard);
        }
        else
        {
            Debug.LogError("HandDisplay script not found or this is not the Player.");
        }

        Debug.Log("Hand after maneuver: " + string.Join(", ", hand.Select(card => card.name)));

        HighlightNodesInRange();
    }

    public void DiscardCard(Card card)
    {
        if (hand.Contains(card))
        {
            hand.Remove(card);
            Debug.Log("Discarded: " + card.name + ". Current hand: " + string.Join(", ", hand.Select(c => c.name)));

            // Update hand display
            var handDisplay = FindFirstObjectByType<HandDisplay>();
            if (handDisplay != null)
            {
                handDisplay.DisplayHand(hand, SelectCard);
            }
        }
    }

    public void BoostManeuver(Card cardToDiscard)
    {
        Debug.Log("Attempting to boost maneuver.");
        Debug.Log("canBoost: " + canBoost);
        Debug.Log("hand contains card: " + hand.Contains(cardToDiscard));

        if (canBoost && hand.Contains(cardToDiscard))
        {
            DiscardCard(cardToDiscard);
            movement += cardToDiscard.boost;
            canBoost = false;
            Debug.Log("Boosted movement by: " + cardToDiscard.boost + " for " + gameObject.tag);
            HighlightNodesInRange();
        }
        else
        {
            Debug.Log("Cannot boost. Either already boosted or card not in hand.");
        }
    }

    public void UseSchemeCard(Card card)
    {
        var turnManager = FindFirstObjectByType<TurnManager>();
        if (turnManager != null && turnManager.CanPerformAction())
        {
            Debug.Log("Using scheme card: " + card.name);
            DiscardCard(card);
            turnManager.PerformAction(TurnManager.ActionType.Scheme);
        }
        else
        {
            Debug.LogError("No actions remaining to use a scheme card.");
        }
    }

    public void SelectCard(Card card)
    {
        var turnManager = FindFirstObjectByType<TurnManager>();
        if (turnManager != null && !turnManager.CanPerformAction() && card.cardType == CardType.Scheme)
        {
            Debug.LogError("No actions remaining to use a scheme card.");
            return;
        }

        if (CanBoost)
        {
            BoostManeuver(card);
        }
        else
        {
            switch (card.cardType)
            {
                case CardType.Attack:
                case CardType.Versatile:
                    SelectCardForAttack(card);
                    break;
                case CardType.Scheme:
                    UseSchemeCard(card);
                    break;
                default:
                    Debug.LogError("Unknown card type: " + card.cardType);
                    break;
            }
        }
    }

    private void SelectCardForAttack(Card card)
    {
        if (combatType == CombatType.Melee && !currentNode.IsConnectedTo(FindFirstObjectByType<Enemy>().currentNode))
        {
            Debug.LogError("Not in melee range to attack.");
            return;
        }

        var combatManager = FindFirstObjectByType<CombatManager>();
        if (combatManager != null)
        {
            combatManager.InitiateAttack(card);
        }
        else
        {
            Debug.LogError("CombatManager not found");
        }
    }

    private void HighlightNodesInRange()
    {
        ResetHighlights();

        Queue<(Node, int)> queue = new Queue<(Node, int)>();
        HashSet<Node> visited = new HashSet<Node>();
        queue.Enqueue((currentNode, 0));
        visited.Add(currentNode);

        while (queue.Count > 0)
        {
            var (node, steps) = queue.Dequeue();
            foreach (Node connection in node.connections)
            {
                if (!visited.Contains(connection) && steps + 1 <= movement)
                {
                    connection.Highlight(true);
                    queue.Enqueue((connection, steps + 1));
                    visited.Add(connection);
                }
            }
        }
    }

    public void MoveToNode(Node targetNode, bool throughUnits = false)
    {
        if (currentNode == null)
        {
            Debug.LogError("Current node is not set for: " + gameObject.tag);
            return;
        }

        if (targetNode == null)
        {
            Debug.LogError("Target node is not set for: " + gameObject.tag);
            return;
        }

        if (targetNode.IsOccupied())
        {
            Debug.LogError("Target node is occupied: " + targetNode.nodeName);
            return;
        }

        if (!throughUnits && currentNode.PathBlockedByUnit(targetNode))
        {
            Debug.LogError("Cannot move through enemy units without special movement.");
            return;
        }

        Debug.Log(gameObject.tag + " attempting to move to node: " + targetNode.nodeName);
        int steps = CalculateStepsToNode(targetNode);
        if (steps <= movement)
        {
            currentNode = targetNode;
            transform.position = targetNode.transform.position;
            movement -= steps;
            Debug.Log(gameObject.tag + " moved to node: " + targetNode.nodeName + ". Remaining movement: " + movement);
            HighlightNodesInRange();
        }
        else
        {
            Debug.Log(gameObject.tag + " does not have enough movement left.");
        }
    }

    private int CalculateStepsToNode(Node targetNode)
    {
        Queue<(Node, int)> queue = new Queue<(Node, int)>();
        HashSet<Node> visited = new HashSet<Node>();
        queue.Enqueue((currentNode, 0));
        visited.Add(currentNode);

        while (queue.Count > 0)
        {
            var (node, steps) = queue.Dequeue();
            if (node == targetNode)
            {
                return steps;
            }

            foreach (Node connection in node.connections)
            {
                if (!visited.Contains(connection))
                {
                    queue.Enqueue((connection, steps + 1));
                    visited.Add(connection);
                }
            }
        }

        return int.MaxValue;
    }

    private void ResetHighlights()
    {
        foreach (Node node in FindObjectsByType<Node>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            node.Highlight(false);
        }
    }
}
