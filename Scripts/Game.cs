using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Game : MonoBehaviour
{
    [SerializeField] private GameObject playerTokenPrefab;
    [SerializeField] private GameObject enemyTokenPrefab;

    private Player player;
    private Enemy enemy;
    private DeckManager deckManager;
    private Board board;
    public HandDisplay handDisplay;
    public CombatManager combatManager;
    public TurnManager turnManager;

    void Start()
    {
        board = FindFirstObjectByType<Board>();
        deckManager = FindFirstObjectByType<DeckManager>();
        turnManager = FindFirstObjectByType<TurnManager>();

        combatManager = FindFirstObjectByType<CombatManager>();
        if (combatManager == null)
        {
            Debug.LogError("CombatManager not found in the scene. Please ensure it is added");
        }

        InitializePlayer("NodeN", Player.CombatType.Melee);
        DrawInitialHand(player, 5);

        InitializeEnemy("NodeE_1");
        DrawInitialHand(enemy, 5);

        if (combatManager != null)
        {
            combatManager.player = player;
            combatManager.enemy = enemy;
            if (combatManager.player != null)
            {
                Debug.Log("CombatManager.player successfully assigned.");
            }
            if (combatManager.enemy != null)
            {
                Debug.Log("CombatManager.enemy successfully assigned.");
            }
        }

        if (turnManager != null)
        {
            turnManager.player = player;
            turnManager.enemy = enemy;
            turnManager.StartPlayerTurn();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (turnManager.CanPerformAction())
            {
                player.Maneuver();
                handDisplay.DisplayHand(player.hand, player.SelectCard);
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            var enemy = FindFirstObjectByType<Enemy>();
            if (player != null && player.movement > 0)
            {
                player.EndManeuver();
            }
            else if (enemy != null && enemy.movement > 0)
            {
                enemy.EndManeuver();
            }
        }
    }


    public void OnCardDiscarded(Card card)
    {
        player.BoostManeuver(card);
        handDisplay.DisplayHand(player.hand, OnCardDiscarded);
    }

    private void InitializePlayer(string startNodeName, Player.CombatType type)
    {
        Deck chosenDeck = deckManager.GetDeck("Bigfoot");

        GameObject playerToken = Instantiate(playerTokenPrefab, Vector3.zero, Quaternion.identity);
        player = playerToken.GetComponent<Player>();

        player.Initialize(chosenDeck, type);
        player.currentNode = board.GetNodeByName(startNodeName);
        player.SetStartingNode();

        playerToken.transform.position = player.currentNode.transform.position;
    }

    private void InitializeEnemy(string startNodeName)
    {
        Deck chosenDeck = deckManager.GetDeck("Bigfoot");

        GameObject enemyToken = Instantiate(enemyTokenPrefab, Vector3.zero, Quaternion.identity);
        enemy = enemyToken.GetComponent<Enemy>();

        enemy.Initialize(chosenDeck);
        enemy.currentNode = board.GetNodeByName(startNodeName);
        enemy.SetStartingNode();

        enemyToken.transform.position = enemy.currentNode.transform.position;
    }

    private void DrawInitialHand(Player player, int handSize)
    {
        Debug.Log($"HandDisplay reference: {handDisplay != null}");
        Debug.Log($"Player reference: {player != null}");
        Debug.Log($"Player hand: {player.hand != null}");

        player.hand = new List<Card>();
        Debug.Log("Drawing initial hand for player...");

        for (int i = 0; i < handSize; i++)
        {
            player.DrawCard();
        }

        Debug.Log("Player's hand after initial draw: " + string.Join(", ", player.hand.Select(card => card.name)));
        Debug.Log($"Card count in hand: {player.hand.Count}");

        handDisplay.DisplayHand(player.hand, OnCardDiscarded);
        Debug.Log("Displayed player's initial hand.");
    }

    private void DrawInitialHand(Enemy enemy, int handSize)
    {
        enemy.hand = new List<Card>();
        for (int i = 0; i < handSize; i++)
        {
            enemy.DrawCard();
        }
        Debug.Log("Enemy initial hand drawn: " + string.Join(", ", enemy.hand.Select(card => card.name)));
    }
}
