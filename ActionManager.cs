using UnityEngine;

public class ActionManager : MonoBehaviour
{
    public ActionState currentAction { get; private set; } = ActionState.None;

    public bool CanStartAction(ActionState newAction)
    {
        Debug.Log($"Attempting to start {newAction}. Current action: {currentAction}");
        return currentAction == ActionState.None;
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
