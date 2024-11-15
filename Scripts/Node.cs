using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Node : MonoBehaviour
{
    public string nodeName;
    public List<Node> connections = new List<Node>();
    public List<string> zones = new List<string>();
    public bool isHighlighted = false;
    private Renderer nodeRenderer;

    void Start()
    {
        nodeRenderer = GetComponent<Renderer>();
    }

    public bool IsConnectedTo(Node otherNode)
    {
        return connections.Contains(otherNode);
    }

    private void OnMouseDown()
    {
        if (!isHighlighted) return;

        var game = FindFirstObjectByType<Game>();
        if (game != null && game.currentState == Game.GameState.SidekickPlacement)
        {
            game.PlaceSidekick(this);
            return;
        }

        var movementUI = FindFirstObjectByType<MovementUI>();
        if (movementUI != null && movementUI.CurrentlySelectedUnit != null)
        {
            var selectedUnit = movementUI.CurrentlySelectedUnit;

            if (selectedUnit is Player player && player.movement > 0)
            {
                player.MoveToNode(this, true);
            }
            else if (selectedUnit is Sidekick sidekick && sidekick.movement > 0)
            {
                sidekick.MoveToNode(this, true);
            }
            else if (selectedUnit is Enemy enemy && enemy.movement > 0)
            {
                enemy.MoveToNode(this, true);
            }
        }
    }

    public void Highlight(bool highlight)
    {
        isHighlighted = highlight;
        if (nodeRenderer != null)
        {
            var game = FindFirstObjectByType<Game>();
            if (game != null && game.currentState == Game.GameState.SidekickPlacement)
            {
                nodeRenderer.material.color = highlight ? Color.green : Color.white;
                return;
            }

            var movementUI = FindFirstObjectByType<MovementUI>();
            if (movementUI != null && movementUI.CurrentlySelectedUnit != null)
            {
                var selectedUnit = movementUI.CurrentlySelectedUnit;
                bool canMove = false;

                if (selectedUnit is Player player)
                    canMove = !IsOccupied() && player.movement > 0;
                else if (selectedUnit is Sidekick sidekick)
                    canMove = !IsOccupied() && sidekick.movement > 0;
                else if (selectedUnit is Enemy enemy)
                    canMove = !IsOccupied() && enemy.movement > 0;

                nodeRenderer.material.color = highlight && canMove ? Color.yellow : Color.white;
            }
            else
            {
                nodeRenderer.material.color = highlight ? Color.yellow : Color.white;
            }
        }
    }

    public bool IsOccupied()
    {
        return FindObjectsByType<Player>(FindObjectsSortMode.None).Any(p => p.currentNode == this) ||
               FindObjectsByType<Enemy>(FindObjectsSortMode.None).Any(e => e.currentNode == this) ||
               FindObjectsByType<Sidekick>(FindObjectsSortMode.None).Any(s => s.currentNode == this);
    }

    public bool PathBlockedByUnit(Node targetNode, MonoBehaviour unit = null)
    {
        var path = GetPathToNode(targetNode);
        foreach (var node in path)
        {
            if (node != this && node != targetNode && node.IsOccupied())
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerable<Node> GetPathToNode(Node targetNode)
    {
        Queue<(Node, List<Node>)> queue = new Queue<(Node, List<Node>)>();
        HashSet<Node> visited = new HashSet<Node>();
        queue.Enqueue((this, new List<Node> { this }));
        visited.Add(this);

        while (queue.Count > 0)
        {
            var (node, path) = queue.Dequeue();
            if (node == targetNode)
            {
                return path;
            }

            foreach (Node connection in node.connections)
            {
                if (!visited.Contains(connection))
                {
                    var newPath = new List<Node>(path) { connection };
                    queue.Enqueue((connection, newPath));
                    visited.Add(connection);
                }
            }
        }

        return Enumerable.Empty<Node>();
    }
}
