using UnityEngine;

public class ActionManager : MonoBehaviour
{
    public ActionState currentAction { get; set; } = ActionState.None;
    private TurnManager turnManager;

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
                return currentAction == ActionState.None;

            case ActionState.BoostedManeuvering:
                return currentAction == ActionState.Maneuvering;

            default:
                return false;
        }
    }

    public void StartAction(ActionState action)
    {
        currentAction = action;
    }

    public void EndAction()
    {
        currentAction = ActionState.None;
    }
}
