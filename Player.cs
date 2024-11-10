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
    private ActionManager actionManager;

    public enum CombatType { Melee, Ranged }
    public CombatType combatType;

    private bool canBoost = false;
    public bool CanBoost => canBoost;

    void Start()
    {
        actionManager = gameObject.AddComponent<ActionManager>();
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
        var turnManager = FindFirstObjectByType<TurnManager>();
        if (actionManager.CanStartAction(ActionState.Maneuvering) && turnManager.CanPerformAction())
        {
            actionManager.StartAction(ActionState.Maneuvering);
            DrawCard();
            movement = deck.baseMovement;
            canBoost = true;
            Debug.Log("Base movement for " + gameObject.tag + ": " + movement);

            var handDisplay = FindFirstObjectByType<HandDisplay>();
            if (handDisplay != null && CompareTag("Player"))
            {
                handDisplay.DisplayHand(hand, SelectCard);
            }

            HighlightNodesInRange();
        }
    }



    public void DiscardCard(Card card)
    {
        if (hand.Contains(card))
        {
            hand.Remove(card);
            Debug.Log("Discarded: " + card.name + ". Current hand: " + string.Join(", ", hand.Select(c => c.name)));

            var handDisplay = FindFirstObjectByType<HandDisplay>();
            if (handDisplay != null)
            {
                handDisplay.DisplayHand(hand, SelectCard);
            }
        }
    }

    public void BoostManeuver(Card cardToDiscard)
    {
        if ((actionManager.currentAction == ActionState.Maneuvering ||
             actionManager.currentAction == ActionState.BoostedManeuvering) &&
            canBoost && hand.Contains(cardToDiscard))
        {
            actionManager.StartAction(ActionState.BoostedManeuvering);
            DiscardCard(cardToDiscard);
            movement += cardToDiscard.boost;
            canBoost = false;
            Debug.Log("Boosted movement by: " + cardToDiscard.boost + " for " + gameObject.tag);
            HighlightNodesInRange();
        }
    }



    public void UseSchemeCard(Card card)
    {
        var turnManager = FindFirstObjectByType<TurnManager>();
        if (actionManager.CanStartAction(ActionState.Scheming) && turnManager != null && turnManager.CanPerformAction())
        {
            actionManager.StartAction(ActionState.Scheming);
            Debug.Log("Using scheme card: " + card.name);
            DiscardCard(card);
            turnManager.PerformAction(TurnManager.ActionType.Scheme);
            actionManager.EndAction();
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

        if (actionManager.currentAction == ActionState.Maneuvering && CanBoost)
        {
            BoostManeuver(card);
        }
        else if (actionManager.CanStartAction(ActionState.None))
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

        if (actionManager.CanStartAction(ActionState.Attacking))
        {
            var combatManager = FindFirstObjectByType<CombatManager>();
            if (combatManager != null)
            {
                actionManager.StartAction(ActionState.Attacking);
                combatManager.InitiateAttack(card);
            }
        }
        else
        {
            Debug.LogError("Cannot attack while another action is in progress. End current action first.");
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
        if (actionManager.currentAction != ActionState.Maneuvering &&
            actionManager.currentAction != ActionState.BoostedManeuvering)
        {
            Debug.LogError("Cannot move outside of maneuver action.");
            return;
        }

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

            if (movement == 0 && actionManager.currentAction == ActionState.BoostedManeuvering)
            {
                EndManeuver();
            }
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

    public void EndManeuver()
    {
        if (actionManager.currentAction == ActionState.Maneuvering ||
            actionManager.currentAction == ActionState.BoostedManeuvering)
        {
            var turnManager = FindFirstObjectByType<TurnManager>();
            if (turnManager != null)
            {
                turnManager.PerformAction(TurnManager.ActionType.Maneuver);
            }
            actionManager.EndAction();
            canBoost = false;
            movement = 0;
            ResetHighlights();
        }
    }
}
