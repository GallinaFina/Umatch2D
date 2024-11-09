using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public Player player;
    public Enemy enemy;
    private int actionsRemaining;

    void Start()
    {
        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        actionsRemaining = 2;
        Debug.Log("Player's turn started. Actions remaining: " + actionsRemaining);
    }

    public bool CanPerformAction()
    {
        return actionsRemaining > 0;
    }

    public void PerformAction(ActionType actionType)
    {
        if (actionsRemaining > 0)
        {
            actionsRemaining--;
            Debug.Log("Action performed: " + actionType + ". Actions remaining: " + actionsRemaining);

            if (actionsRemaining == 0)
            {
                EndPlayerTurn();
            }
        }
        else
        {
            Debug.LogError("No actions remaining.");
        }
    }

    private void EndPlayerTurn()
    {
        Debug.Log("Player's turn ended.");
        // Placeholder for logic to start enemy's turn or other transitions
    }

    public enum ActionType
    {
        Maneuver,
        Attack,
        Scheme
    }
}
