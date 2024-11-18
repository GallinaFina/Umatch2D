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

        var game = ServiceLocator.Instance.GameManager;
        if (game != null && game.currentState == Game.GameState.SidekickPlacement)
        {
            game.PlaceSidekick(this);
            return;
        }

        var movementUI = ServiceLocator.Instance.MovementUI;
        if (movementUI != null && movementUI.CurrentlySelectedUnit != null)
        {
            var selectedUnit = movementUI.CurrentlySelectedUnit;

            if (selectedUnit is Player player && player.movement > 0)
            {
                player.MoveToNode(this, false);
            }
            else if (selectedUnit is Sidekick sidekick && sidekick.movement > 0)
            {
                sidekick.MoveToNode(this, false);
            }
            else if (selectedUnit is Enemy enemy && enemy.movement > 0)
            {
                enemy.MoveToNode(this, false);
            }
        }
    }

    private void OnMouseEnter()
    {
        var game = ServiceLocator.Instance.GameManager;
        if (game != null && game.currentState == Game.GameState.SidekickPlacement)
        {
            var placementUI = ServiceLocator.Instance.GameManager.placementUI;
            if (placementUI != null)
            {
                bool isValid = zones.Intersect(game.player.GetStartingNode().zones).Any() && !IsOccupied();
                placementUI.HighlightHoveredNode(this, isValid);
            }
        }
    }

    private void OnMouseExit()
    {
        var game = ServiceLocator.Instance.GameManager;
        if (game != null && game.currentState == Game.GameState.SidekickPlacement)
        {
            var placementUI = ServiceLocator.Instance.GameManager.placementUI;
            if (placementUI != null)
            {
                placementUI.HighlightHoveredNode(this, false);
            }
        }
    }

    public void Highlight(bool highlight)
    {
        isHighlighted = highlight;
        if (nodeRenderer != null)
        {
            var game = ServiceLocator.Instance.GameManager;
            if (game != null && game.currentState == Game.GameState.SidekickPlacement)
            {
                return;
            }

            var movementUI = ServiceLocator.Instance.MovementUI;
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
        var game = ServiceLocator.Instance.GameManager;
        if (game == null) return false;

        return (game.player != null && game.player.currentNode == this) ||
               (game.enemy != null && game.enemy.currentNode == this) ||
               game.player.sidekicks.Any(s => s.currentNode == this);
    }

    public bool PathBlockedByUnit(Node targetNode, MonoBehaviour unit = null)
    {
        return PathfindingUtility.IsPathBlocked(this, targetNode, unit);
    }
}
