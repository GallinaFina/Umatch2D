using UnityEngine;
using System.Collections;

public class TestManager : MonoBehaviour
{
    private CombatManager combatManager;
    private Player player;
    private Enemy enemy;
    private Board board;
    private HandDisplay handDisplay;

    void Start()
    {
        StartCoroutine(InitializeReferences());
    }

    private IEnumerator InitializeReferences()
    {
        yield return null;
        combatManager = FindFirstObjectByType<CombatManager>();
        player = FindFirstObjectByType<Player>();
        enemy = FindFirstObjectByType<Enemy>();
        board = FindFirstObjectByType<Board>();
        handDisplay = FindFirstObjectByType<HandDisplay>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            ClearAllHands();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearEnemyHand();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            CreateSkirmishCards();
        }
    }

    private void CreateSkirmishCards()
    {
        Card playerSkirmish = new Card("Skirmish", 3, 1, CardType.Versatile, "After combat: If you won the combat, choose one of the fighters in the combat and move them up to 2 spaces", 1, "", CardEffectTiming.AfterCombat);
        Card enemySkirmish = new Card("Skirmish", 3, 1, CardType.Versatile, "After combat: If you won the combat, choose one of the fighters in the combat and move them up to 2 spaces", 1, "", CardEffectTiming.AfterCombat);

        player.hand.Add(playerSkirmish);
        enemy.hand.Add(enemySkirmish);

        handDisplay.DisplayHand(player.hand, player.SelectCard);
        Debug.Log("Created Skirmish cards for both players");
    }

    private void ClearAllHands()
    {
        player.hand.Clear();
        enemy.hand.Clear();
        handDisplay.DisplayHand(player.hand, player.SelectCard);
        Debug.Log("Cleared all hands");
    }

    private void ClearEnemyHand()
    {
        enemy.hand.Clear();
        Debug.Log("Cleared enemy hands");
    }
}
