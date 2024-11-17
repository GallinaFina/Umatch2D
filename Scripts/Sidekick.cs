using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using static Player;

public class Sidekick : MonoBehaviour
{
    public string sidekickName;
    public Player.CombatType combatType;
    public int maxHealth;
    public int currentHealth;
    public MonoBehaviour owner;
    public Node currentNode;
    public ActionManager actionManager;
    public int movement;

    public void Initialize(SidekickData data, MonoBehaviour owner)
    {
        sidekickName = data.name;
        combatType = data.combatType;
        maxHealth = data.health;
        currentHealth = maxHealth;
        this.owner = owner;
        actionManager = gameObject.AddComponent<ActionManager>();
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
        if (currentNode == null || targetNode == null || targetNode.IsOccupied())
        {
            return;
        }

        if (!throughUnits && currentNode.PathBlockedByUnit(targetNode, this))
        {
            Debug.LogError("Cannot move through enemy units without special movement.");
            return;
        }

        int steps = CalculateStepsToNode(targetNode);
        if (steps <= movement)
        {
            currentNode = targetNode;
            transform.position = targetNode.transform.position;
            movement -= steps;
            HighlightNodesInRange();

            if (movement <= 0)
            {
                var movementUI = FindFirstObjectByType<MovementUI>();
                if (movementUI != null)
                {
                    movementUI.MarkUnitMoved(this);
                }
            }
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

    public void ResetHighlights()
    {
        foreach (Node node in FindObjectsByType<Node>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            node.Highlight(false);
        }
    }

    public Card SelectCardForDefense()
    {
        if (owner is Player playerOwner)
        {
            return playerOwner.SelectCardForDefense();
        }
        else if (owner is Enemy enemyOwner)
        {
            return enemyOwner.SelectDefenseCard();
        }
        return null;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"Sidekick {sidekickName} took {amount} damage. HP: {currentHealth}/{maxHealth}");
    }
}
