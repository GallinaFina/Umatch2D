using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class PathfindingUtility
{
    private static Dictionary<(Node, Node), List<Node>> pathCache = new Dictionary<(Node, Node), List<Node>>();

    public static int CalculateSteps(Node startNode, Node targetNode)
    {
        var path = FindPath(startNode, targetNode);
        return path != null ? path.Count - 1 : int.MaxValue;
    }

    public static List<Node> FindPath(Node startNode, Node targetNode, bool canMoveThrough = false)
    {
        if (pathCache.TryGetValue((startNode, targetNode), out List<Node> cachedPath))
        {
            return cachedPath;
        }

        var path = CalculatePath(startNode, targetNode);
        pathCache[(startNode, targetNode)] = path;
        return path;
    }

    public static bool IsPathBlocked(Node startNode, Node targetNode, MonoBehaviour unit)
    {
        var path = FindPath(startNode, targetNode);
        if (path == null) return true;

        foreach (var node in path)
        {
            if (node != startNode && node != targetNode && node.IsOccupied())
            {
                if (unit is Player || (unit is Sidekick s && s.owner is Player))
                {
                    var enemy = ServiceLocator.Instance.GameManager.enemy;
                    var enemySidekick = ServiceLocator.Instance.GameManager.GetSidekickForOwner(enemy);
                    return enemy.currentNode == node || (enemySidekick != null && enemySidekick.currentNode == node);
                }
                else if (unit is Enemy || (unit is Sidekick s2 && s2.owner is Enemy))
                {
                    var player = ServiceLocator.Instance.GameManager.player;
                    return player.currentNode == node || player.sidekicks.Any(s => s.currentNode == node);
                }
                return true;
            }
        }
        return false;
    }

    private static List<Node> CalculatePath(Node startNode, Node targetNode)
    {
        var queue = new Queue<(Node node, List<Node> path)>();
        var visited = new HashSet<Node>();

        queue.Enqueue((startNode, new List<Node> { startNode }));
        visited.Add(startNode);

        while (queue.Count > 0)
        {
            var (currentNode, currentPath) = queue.Dequeue();
            if (currentNode == targetNode)
            {
                return currentPath;
            }

            foreach (var connection in currentNode.connections)
            {
                if (!visited.Contains(connection))
                {
                    var newPath = new List<Node>(currentPath) { connection };
                    queue.Enqueue((connection, newPath));
                    visited.Add(connection);
                }
            }
        }

        return null;
    }

    public static void ClearCache()
    {
        pathCache.Clear();
    }

#if UNITY_EDITOR
    public static void DebugDrawPath(List<Node> path, Color color, float duration = 1f)
    {
        if (path == null || path.Count < 2) return;
        
        for (int i = 0; i < path.Count - 1; i++)
        {
            Debug.DrawLine(
                path[i].transform.position, 
                path[i + 1].transform.position, 
                color, 
                duration
            );
        }
    }
#endif
}
