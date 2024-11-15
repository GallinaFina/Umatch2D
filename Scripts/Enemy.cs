using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Enemy : MonoBehaviour
{
    public List<Card> hand;
    public Deck deck;
    public Node currentNode;
    public Node startingNode;
    public ActionManager actionManager;
    public int maxHP;
    public int currentHP;
    public int movement;
    public Player.CombatType combatType = Player.CombatType.Melee;
    public SpriteRenderer characterPortrait;

    void Start()
    {
        actionManager = gameObject.AddComponent<ActionManager>();
    }

    public void Initialize(Deck chosenDeck)
    {
        deck = chosenDeck;
        maxHP = deck.startingHealth;
        currentHP = maxHP;
        SetStartingNode();
        Debug.Log($"Enemy Initialize - Current Node before SetStartingNode: {currentNode?.nodeName}");
        SetStartingNode();
        Debug.Log($"Enemy Initialize - Current Node after SetStartingNode: {currentNode?.nodeName}");

        string portraitPath = "Images/Portraits/" + chosenDeck.name;
        Sprite portrait = Resources.Load<Sprite>(portraitPath);
        if (portrait != null && characterPortrait != null)
        {
            characterPortrait.sprite = portrait;
        }
    }

    public void SetStartingNode()
    {
        Debug.Log($"Enemy SetStartingNode - Current Node before set: {currentNode?.nodeName}");
        startingNode = currentNode;
        Debug.Log($"Enemy SetStartingNode - Starting Node after set: {startingNode?.nodeName}");
        Debug.Log($"Enemy SetStartingNode - Node connections: {string.Join(", ", currentNode?.connections.Select(n => n.nodeName) ?? new string[] { "none" })}");
    }

    public Node GetStartingNode()
    {
        return startingNode;
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
        if (actionManager.CanPerformAction(ActionState.Maneuvering))
        {
            actionManager.StartAction(ActionState.Maneuvering);
            DrawCard();
            movement = deck.baseMovement;

            // Set same base movement for owned sidekick
            var sidekick = FindFirstObjectByType<Sidekick>();
            if (sidekick && sidekick.owner == this)
            {
                sidekick.movement = deck.baseMovement;
                sidekick.HighlightNodesInRange();
            }

            HighlightNodesInRange();
        }
    }

    public void EndManeuver()
    {
        if (actionManager.currentAction == ActionState.Maneuvering ||
            actionManager.currentAction == ActionState.BoostedManeuvering)
        {
            actionManager.EndAction();
            movement = 0;

            // Reset sidekick movement too
            var sidekick = FindFirstObjectByType<Sidekick>();
            if (sidekick && sidekick.owner == this)
            {
                sidekick.movement = 0;
                sidekick.ResetHighlights();
            }

            ResetHighlights();
        }
    }

    public void DiscardCard(Card card)
    {
        if (hand.Contains(card))
        {
            hand.Remove(card);
            Debug.Log("Discarded: " + card.name + ". Current hand: " + string.Join(", ", hand.Select(c => c.name)));
        }
    }

    public Card SelectDefenseCard()
    {
        Debug.Log($"Enemy hand contains {hand.Count} cards");
        Debug.Log($"Hand contents: {string.Join(", ", hand.Select(c => $"{c.name} ({c.cardType})"))}");

        var validDefenseCards = hand.Where(card =>
            card.cardType == CardType.Defense ||
            card.cardType == CardType.Versatile).ToList();

        Debug.Log($"Found {validDefenseCards.Count} valid defense cards: {string.Join(", ", validDefenseCards.Select(c => c.name))}");

        if (validDefenseCards.Count > 0)
        {
            int randomIndex = Random.Range(0, validDefenseCards.Count);
            Card selectedCard = validDefenseCards[randomIndex];
            Debug.Log($"Selected {selectedCard.name} ({selectedCard.cardType}) for defense");
            DiscardCard(selectedCard);
            return selectedCard;
        }

        Debug.Log("No valid defense cards found");
        return null;
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        Debug.Log($"{gameObject.tag} took {amount} damage. HP: {currentHP}/{maxHP}");
    }

    public void HighlightNodesInRange()
    {
        Debug.Log($"Enemy attempting to highlight nodes. Current node: {currentNode?.nodeName}, Movement: {movement}");
        Debug.Log($"Current node connections: {string.Join(", ", currentNode.connections.Select(n => n.nodeName))}");
        ResetHighlights();

        Queue<(Node, int)> queue = new Queue<(Node, int)>();
        HashSet<Node> visited = new HashSet<Node>();
        queue.Enqueue((currentNode, 0));
        visited.Add(currentNode);

        while (queue.Count > 0)
        {
            var (node, steps) = queue.Dequeue();
            Debug.Log($"Checking node {node.nodeName} at {steps} steps");
            foreach (Node connection in node.connections)
            {
                Debug.Log($"Found connection to {connection.nodeName}, steps + 1 = {steps + 1}, movement = {movement}");
                if (!visited.Contains(connection) && steps + 1 <= movement)
                {
                    Debug.Log($"Highlighting node {connection.nodeName}");
                    connection.Highlight(true);
                    queue.Enqueue((connection, steps + 1));
                    visited.Add(connection);
                }
            }
        }
    }

    private void ResetHighlights()
    {
        foreach (Node node in FindObjectsByType<Node>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            node.Highlight(false);
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

        if (!throughUnits && currentNode.PathBlockedByUnit(targetNode, this))
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
}
