using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Enemy : MonoBehaviour
{
    public List<Card> hand;
    private Deck deck;
    public int movement;
    public Node currentNode;

    void Start()
    {
        hand = new List<Card>();  // Initialize in Start or Awake
    }

    public void Initialize(Deck chosenDeck)
    {
        deck = chosenDeck;
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
        Debug.Log("Base movement for " + gameObject.tag + ": " + movement);

        // No hand display for the enemy
        HighlightNodesInRange();
    }

    public void BoostManeuver(Card cardToDiscard)
    {
        if (hand.Contains(cardToDiscard))
        {
            DiscardCard(cardToDiscard);
            movement += cardToDiscard.boost;
            Debug.Log("Boosted movement by: " + cardToDiscard.boost + " for " + gameObject.tag);
            HighlightNodesInRange();
        }
    }

    public void DiscardCard(Card card)
    {
        if (hand.Contains(card))
        {
            hand.Remove(card);
            Debug.Log("Discarded: " + card.name);
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
            Debug.LogError("Cannot move through player units without special movement.");
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

    private void HighlightNodesInRange()
    {
        // Your logic for highlighting nodes
    }
}
