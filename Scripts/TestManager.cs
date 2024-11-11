using UnityEngine;

public class TestManager : MonoBehaviour
{
    private CombatManager combatManager;
    private Player player;
    private Enemy enemy;

    void Start()
    {
        combatManager = FindFirstObjectByType<CombatManager>();
        player = FindFirstObjectByType<Player>();
        enemy = FindFirstObjectByType<Enemy>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestRegroupScenario();
        }
    }

    private void TestRegroupScenario()
    {
        // Create test cards
        Card playerRegroup = new Card("Regroup", 3, 1, CardType.Versatile, "Draw cards based on combat result", 1, "", CardEffectTiming.AfterCombat);
        Card enemyRegroup = new Card("Regroup", 3, 1, CardType.Versatile, "Draw cards based on combat result", 1, "", CardEffectTiming.AfterCombat);

        // Add cards to hands
        player.hand.Add(playerRegroup);
        enemy.hand.Add(enemyRegroup);
    }
}
