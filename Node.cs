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
        if (isHighlighted)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                player.GetComponent<Player>().MoveToNode(this);
            }
            else
            {
                Debug.LogError("Player GameObject not found.");
            }
        }
    }

    public void Highlight(bool highlight)
    {
        isHighlighted = highlight;
        if (nodeRenderer != null)
        {
            nodeRenderer.material.color = highlight ? Color.yellow : Color.white;
        }
    }

    public bool IsOccupied()
    {
        return FindObjectsByType<Player>(FindObjectsSortMode.None).Any(p => p.currentNode == this) ||
               FindObjectsByType<Enemy>(FindObjectsSortMode.None).Any(e => e.currentNode == this);
    }

    public bool PathBlockedByUnit(Node targetNode)
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
