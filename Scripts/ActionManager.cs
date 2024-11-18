using UnityEngine;

public class ActionManager : MonoBehaviour
{
    public ActionState currentAction { get; set; } = ActionState.None;
    private MonoBehaviour currentUnit;
    private bool hasMovedThisTurn = false;

    private void Start()
    {
        ServiceLocator.Instance.TurnManager.RegisterActionManager(this);
    }

    public bool CanPerformAction(ActionState newAction)
    {
        if (!ServiceLocator.Instance.TurnManager.CanPerformAction())
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
        ServiceLocator.Instance.TurnManager.TrackActionState(currentUnit, action);

        if (action == ActionState.Maneuvering)
        {
            hasMovedThisTurn = true;
            ServiceLocator.Instance.MovementUI?.ResetMovedUnits();
        }
    }

    public void EndAction()
    {
        if (currentAction != ActionState.None)
        {
            ServiceLocator.Instance.MovementUI?.ResetMovedUnits();
            ServiceLocator.Instance.TurnManager.PerformAction(ConvertToTurnAction(currentAction));
            ServiceLocator.Instance.TurnManager.TrackActionState(currentUnit, ActionState.None);

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
