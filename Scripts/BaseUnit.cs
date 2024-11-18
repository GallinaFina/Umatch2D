using UnityEngine;
using System.Collections.Generic;

public abstract class BaseUnit : MonoBehaviour
{
    public int maxHP;
    public int currentHP;
    public int movement;
    public Node currentNode;
    public Node startingNode;
    public ActionManager actionManager;
    public CombatType combatType;

    protected virtual void Start()
    {
        actionManager = gameObject.AddComponent<ActionManager>();
    }

    public virtual void Initialize(int startingHealth, CombatType type)
    {
        maxHP = startingHealth;
        currentHP = maxHP;
        combatType = type;
    }

    public virtual void SetStartingNode()
    {
        startingNode = currentNode;
    }

    public virtual Node GetStartingNode()
    {
        return startingNode;
    }

    public virtual void TakeDamage(int amount)
    {
        currentHP -= amount;
        Debug.Log($"{gameObject.tag} took {amount} damage. HP: {currentHP}/{maxHP}");
    }

    public virtual void MoveToNode(Node targetNode, bool throughUnits = false)
    {
        if (!ValidateMove(targetNode, throughUnits)) return;

        int steps = CalculateStepsToNode(targetNode);
        if (steps <= movement)
        {
            currentNode = targetNode;
            transform.position = targetNode.transform.position;
            movement -= steps;
            HighlightNodesInRange();

            if (movement <= 0)
            {
                ServiceLocator.Instance.MovementUI?.MarkUnitMoved(this);
            }
        }
    }

    protected bool ValidateMove(Node targetNode, bool throughUnits)
    {
        if (currentNode == null)
        {
            Debug.LogError("Current node is not set for: " + gameObject.tag);
            return false;
        }

        if (targetNode == null)
        {
            Debug.LogError("Target node is not set for: " + gameObject.tag);
            return false;
        }

        if (targetNode.IsOccupied())
        {
            Debug.LogError("Target node is occupied: " + targetNode.nodeName);
            return false;
        }

        bool isRegularMovement = actionManager.currentAction == ActionState.Maneuvering ||
                                actionManager.currentAction == ActionState.BoostedManeuvering;

        if (isRegularMovement && !throughUnits && currentNode.PathBlockedByUnit(targetNode, this))
        {
            Debug.LogError("Cannot move through enemy units without special movement.");
            return false;
        }

        return true;
    }

    public virtual void HighlightNodesInRange()
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

    public virtual void ResetHighlights()
    {
        var board = ServiceLocator.Instance.Board;
        if (board != null)
        {
            foreach (Node node in board.nodes)
            {
                node.Highlight(false);
            }
        }
    }

    protected virtual int CalculateStepsToNode(Node targetNode)
    {
        return PathfindingUtility.CalculateSteps(currentNode, targetNode);
    }
}
