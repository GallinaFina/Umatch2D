using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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
        ServiceLocator.Instance.RegisterService(this);
    }

    public void RegisterActionManager(ActionManager actionManager)
    {
        currentActions[actionManager.gameObject.GetComponent<MonoBehaviour>()] = ActionState.None;
    }


    public void StartPlayerTurn()
    {
        PathfindingUtility.ClearCache();
        actionsRemaining = 4;
        currentActions.Clear();
        player.SetStartingNode();
        player.actionManager.ResetTurn();

        var sidekick = ServiceLocator.Instance.GameManager.GetSidekickForOwner(player);
        if (sidekick != null)
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
            if (!ServiceLocator.Instance.EffectManager.IsEffectComplete)
            {
                return;
            }

            actionsRemaining--;
            Debug.Log($"Action performed: {actionType}. Actions remaining: {actionsRemaining}");

            if (actionsRemaining == 0)
            {
                StartCoroutine(WaitForEffectsThenEndTurn());
            }
        }
        else
        {
            Debug.Log("No actions remaining to perform.");
        }
    }

    private IEnumerator WaitForEffectsThenEndTurn()
    {
        while (!ServiceLocator.Instance.EffectManager.IsEffectComplete)
        {
            yield return null;
        }
        EndPlayerTurn();
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
    }
}
