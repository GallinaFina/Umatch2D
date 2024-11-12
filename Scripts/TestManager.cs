using UnityEngine;

public class TestManager : MonoBehaviour
{
    private CombatManager combatManager;
    private Player player;
    private Enemy enemy;
    private Board board;
    private HandDisplay handDisplay;

    void Start()
    {
        combatManager = FindFirstObjectByType<CombatManager>();
        player = FindFirstObjectByType<Player>();
        enemy = FindFirstObjectByType<Enemy>();
        board = FindFirstObjectByType<Board>();
        handDisplay = FindFirstObjectByType<HandDisplay>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            CreateRegroupCards();
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            CreateMomentousShiftCards();
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            ClearAllHands();
        }
    }

    private void CreateRegroupCards()
    {
        Card playerRegroup = new Card("Regroup", 3, 1, CardType.Versatile, "Draw cards based on combat result", 1, "", CardEffectTiming.AfterCombat);
        Card enemyRegroup = new Card("Regroup", 3, 1, CardType.Versatile, "Draw cards based on combat result", 1, "", CardEffectTiming.AfterCombat);

        player.hand.Add(playerRegroup);
        enemy.hand.Add(enemyRegroup);

        handDisplay.DisplayHand(player.hand, player.SelectCard);
        Debug.Log("Created Regroup cards for both players");
    }

    private void CreateMomentousShiftCards()
    {
        Card playerMomentousShift = new Card("Momentous shift", 3, 1, CardType.Versatile, "If you've moved from your starting position, gain +2 power", 1, "", CardEffectTiming.DuringCombat);
        Card enemyMomentousShift = new Card("Momentous shift", 3, 1, CardType.Versatile, "If you've moved from your starting position, gain +2 power", 1, "", CardEffectTiming.DuringCombat);

        player.hand.Add(playerMomentousShift);
        enemy.hand.Add(enemyMomentousShift);

        handDisplay.DisplayHand(player.hand, player.SelectCard);
        Debug.Log("Created Momentous Shift cards for both players");
    }

    private void ClearAllHands()
    {
        player.hand.Clear();
        enemy.hand.Clear();
        handDisplay.DisplayHand(player.hand, player.SelectCard);
        Debug.Log("Cleared all hands");
    }
}
