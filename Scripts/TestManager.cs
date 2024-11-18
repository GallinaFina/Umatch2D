using UnityEngine;
using System.Collections;

public class TestManager : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(WaitForServiceInitialization());
    }

    private IEnumerator WaitForServiceInitialization()
    {
        yield return null;
        // Services are now initialized
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
        if (Input.GetKeyDown(KeyCode.T))
        {
            CreateCrashThroughTreesCard();
        }
    }

    private void CreateCrashThroughTreesCard()
    {
        Card crashCard = new Card(
            "Crash through the trees",
            0,
            0,
            CardType.Scheme,
            "Move up to 5 spaces, ignoring enemy units.",
            1,
            "",
            CardEffectTiming.Immediately
        );

        var player = ServiceLocator.Instance.GameManager.player;
        player.hand.Add(crashCard);
        ServiceLocator.Instance.HandDisplay.DisplayHand(player.hand, player.SelectCard);
        Debug.Log("Added Crash through the trees card to player's hand");
    }

    private void CreateSkirmishCards()
    {
        Card playerSkirmish = new Card("Skirmish", 3, 1, CardType.Versatile, "After combat: If you won the combat, choose one of the fighters in the combat and move them up to 2 spaces", 1, "", CardEffectTiming.AfterCombat);
        Card enemySkirmish = new Card("Skirmish", 3, 1, CardType.Versatile, "After combat: If you won the combat, choose one of the fighters in the combat and move them up to 2 spaces", 1, "", CardEffectTiming.AfterCombat);

        var game = ServiceLocator.Instance.GameManager;
        game.player.hand.Add(playerSkirmish);
        game.enemy.hand.Add(enemySkirmish);

        ServiceLocator.Instance.HandDisplay.DisplayHand(game.player.hand, game.player.SelectCard);
        Debug.Log("Created Skirmish cards for both players");
    }

    private void ClearAllHands()
    {
        var game = ServiceLocator.Instance.GameManager;
        game.player.hand.Clear();
        game.enemy.hand.Clear();
        ServiceLocator.Instance.HandDisplay.DisplayHand(game.player.hand, game.player.SelectCard);
        Debug.Log("Cleared all hands");
    }

    private void ClearEnemyHand()
    {
        ServiceLocator.Instance.GameManager.enemy.hand.Clear();
        Debug.Log("Cleared enemy hands");
    }
}
