using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Game : MonoBehaviour
{
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

        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        InitializePlayer(player, "NodeN", Player.CombatType.Melee);

        DrawInitialHand(player, 5);

        enemy = GameObject.FindWithTag("Enemy").GetComponent<Enemy>();
        InitializeEnemy(enemy, "NodeE_1");

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
                turnManager.PerformAction(TurnManager.ActionType.Maneuver);
            }
        }
    }


    public void OnCardDiscarded(Card card)
    {
        player.BoostManeuver(card);
        handDisplay.DisplayHand(player.hand, OnCardDiscarded);
    }


    private void InitializePlayer(Player player, string startNodeName, Player.CombatType type)
    {
        Deck chosenDeck = deckManager.GetDeck("Bigfoot");
        player.Initialize(chosenDeck, type);
        player.currentNode = board.GetNodeByName(startNodeName);

        if (player.currentNode == null)
        {
            Debug.LogError("Starting node not found for: " + player.tag);
        }
    }

    private void InitializeEnemy(Enemy enemy, string startNodeName)
    {
        Deck chosenDeck = deckManager.GetDeck("Bigfoot");
        enemy.Initialize(chosenDeck);
        enemy.currentNode = board.GetNodeByName(startNodeName);

        if (enemy.currentNode == null)
        {
            Debug.LogError("Starting node not found for: " + enemy.tag);
        }
    }

    private void DrawInitialHand(Player player, int handSize)
    {
        player.hand = new List<Card>();
        Debug.Log("Drawing initial hand for player...");

        for (int i = 0; i < handSize; i++)
        {
            player.DrawCard();
        }

        Debug.Log("Player's hand after initial draw: " + string.Join(", ", player.hand.Select(card => card.name)));

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
