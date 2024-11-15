using UnityEngine;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    public Player player;
    public Enemy enemy;
    private int actionsRemaining;
    private Dictionary<MonoBehaviour, ActionState> currentActions = new Dictionary<MonoBehaviour, ActionState>();

    public enum ActionType
    {
        Maneuver,
        Attack,
        Scheme
    }

    void Start()
    {

    }

    public void StartPlayerTurn()
    {
        actionsRemaining = 4;
        currentActions.Clear();
        player.SetStartingNode();
        player.actionManager.ResetTurn();
        var sidekick = FindFirstObjectByType<Sidekick>();
        if (sidekick && sidekick.owner == player)
        {
            sidekick.actionManager.ResetTurn();
        }
        Debug.Log("Player's turn started. Actions remaining: " + actionsRemaining);
    }

    public bool CanPerformAction()
    {
        Debug.Log($"Actions remaining: {actionsRemaining}");
        return actionsRemaining > 0;
    }

    public void PerformAction(ActionType actionType)
    {
        if (actionsRemaining > 0)
        {
            actionsRemaining--;
            Debug.Log($"Action performed: {actionType}. Actions remaining: {actionsRemaining}");

            if (actionsRemaining == 0)
            {
                EndPlayerTurn();
            }
        }
        else
        {
            Debug.Log("No actions remaining to perform.");
        }
    }

    public void TrackActionState(MonoBehaviour unit, ActionState state)
    {
        currentActions[unit] = state;
        Debug.Log($"Unit {unit.gameObject.name} action state: {state}");
    }

    public ActionState GetCurrentActionState(MonoBehaviour unit)
    {
        return currentActions.ContainsKey(unit) ? currentActions[unit] : ActionState.None;
    }

    private void EndPlayerTurn()
    {
        Debug.Log("Player's turn ended.");
        currentActions.Clear();
        // Logic for transitioning to enemy's turn will go here
    }
}
