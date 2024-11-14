using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Player : MonoBehaviour
{
    [SerializeField]
    public List<Card> hand;
    public Deck deck;
    public int movement;
    public Node currentNode;
    public Node startingNode;
    public SpriteRenderer characterPortrait;
    public ActionManager actionManager;

    public int maxHP;
    public int currentHP;

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
        maxHP = deck.startingHealth;
        currentHP = maxHP;

        string portraitPath = "Images/Portraits/" + chosenDeck.name;
        Sprite portrait = Resources.Load<Sprite>(portraitPath);
        if (portrait != null)
        {
            characterPortrait.sprite = portrait;
        }
    }

    public void SetStartingNode()
    {
        startingNode = currentNode;
    }

    public Node GetStartingNode()
    {
        return startingNode;
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        Debug.Log($"{gameObject.tag} took {amount} damage. HP: {currentHP}/{maxHP}");
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
        if (actionManager.CanPerformAction(ActionState.BoostedManeuvering) && canBoost && hand.Contains(cardToDiscard))
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
        if (actionManager.CanPerformAction(ActionState.Scheming))
        {
            actionManager.StartAction(ActionState.Scheming);
            Debug.Log("Using scheme card: " + card.name);
            card.TriggerEffect(this, null);  // Add this line to trigger the effect
            DiscardCard(card);
            var turnManager = FindFirstObjectByType<TurnManager>();
            if (turnManager != null)
            {
                turnManager.PerformAction(TurnManager.ActionType.Scheme);
            }
            actionManager.EndAction();
        }
    }


    public void SelectCard(Card card)
    {
        var turnManager = FindFirstObjectByType<TurnManager>();

        // If we're maneuvering and can boost, use the card as a boost regardless of type
        if ((actionManager.currentAction == ActionState.Maneuvering ||
             actionManager.currentAction == ActionState.BoostedManeuvering) &&
            CanBoost)
        {
            BoostManeuver(card);
            return;
        }

        // Otherwise, handle the card based on its type
        switch (card.cardType)
        {
            case CardType.Attack:
            case CardType.Versatile:
                if (actionManager.CanPerformAction(ActionState.Attacking))
                    SelectCardForAttack(card);
                break;

            case CardType.Scheme:
                if (turnManager.CanPerformAction())
                    UseSchemeCard(card);
                break;
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
            if (card.cardType == CardType.Attack || card.cardType == CardType.Versatile)
            {
                actionManager.StartAction(ActionState.Attacking);
                DiscardCard(card);
                combatManager.InitiateAttack(card);
            }
            else
            {
                Debug.LogError("Cannot attack with this card type");
            }
        }
    }

    public void DefendAgainstAttack(Card card)
    {
        if (card.cardType == CardType.Defense || card.cardType == CardType.Versatile)
        {
            var combatManager = FindFirstObjectByType<CombatManager>();
            if (combatManager != null)
            {
                DiscardCard(card);
                combatManager.DefendWith(card);
            }
        }
        else
        {
            Debug.LogError("Cannot defend with this card type");
        }
    }

    public void HighlightNodesInRange()
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
