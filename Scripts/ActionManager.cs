using UnityEngine;

public class ActionManager : MonoBehaviour
{
    public ActionState currentAction { get; set; } = ActionState.None;
    private TurnManager turnManager;
    private MonoBehaviour currentUnit;
    private bool hasMovedThisTurn = false;

    private void Start()
    {
        turnManager = FindFirstObjectByType<TurnManager>();
    }

    public bool CanPerformAction(ActionState newAction)
    {
        if (!turnManager.CanPerformAction())
            return false;

        switch (newAction)
        {
            case ActionState.None:
                return true;

            case ActionState.Attacking:
            case ActionState.Scheming:
                return currentAction == ActionState.None;

            case ActionState.Maneuvering:
                return currentAction == ActionState.None && !hasMovedThisTurn;

            case ActionState.BoostedManeuvering:
                return currentAction == ActionState.Maneuvering;

            default:
                return false;
        }
    }

    public void StartAction(ActionState action, MonoBehaviour unit = null)
    {
        currentUnit = unit ?? GetComponent<MonoBehaviour>();
        currentAction = action;
        turnManager.TrackActionState(currentUnit, action);

        if (action == ActionState.Maneuvering)
        {
            hasMovedThisTurn = true;
            var movementUI = FindFirstObjectByType<MovementUI>();
            if (movementUI != null)
            {
                movementUI.ResetMovedUnits();
            }
        }
    }

    public void EndAction()
    {
        if (currentAction != ActionState.None)
        {
            var movementUI = FindFirstObjectByType<MovementUI>();
            if (movementUI != null)
            {
                movementUI.ResetMovedUnits();
            }

            turnManager.PerformAction(ConvertToTurnAction(currentAction));
            turnManager.TrackActionState(currentUnit, ActionState.None);

            if (currentAction == ActionState.Maneuvering ||
                currentAction == ActionState.BoostedManeuvering)
            {
                ResetTurn();
            }

            currentAction = ActionState.None;
            currentUnit = null;
        }
    }

    public void ResetTurn()
    {
        hasMovedThisTurn = false;
        currentAction = ActionState.None;
        currentUnit = null;
    }

    private TurnManager.ActionType ConvertToTurnAction(ActionState state)
    {
        switch (state)
        {
            case ActionState.Maneuvering:
            case ActionState.BoostedManeuvering:
                return TurnManager.ActionType.Maneuver;
            case ActionState.Attacking:
                return TurnManager.ActionType.Attack;
            case ActionState.Scheming:
                return TurnManager.ActionType.Scheme;
            default:
                return TurnManager.ActionType.Maneuver;
        }
    }
}
