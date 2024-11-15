using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Game : MonoBehaviour
{
    [SerializeField] private GameObject playerTokenPrefab;
    [SerializeField] private GameObject enemyTokenPrefab;
    [SerializeField] private GameObject sidekickTokenPrefab;
    [SerializeField] private SidekickPlacementUI placementUI;

    private Player player;
    private Enemy enemy;
    private DeckManager deckManager;
    private Board board;
    public HandDisplay handDisplay;
    public CombatManager combatManager;
    public TurnManager turnManager;

    public GameState currentState = GameState.Setup;
    private List<Sidekick> sidekicksToPlace = new List<Sidekick>();

    public enum GameState
    {
        Setup,
        SidekickPlacement,
        Playing
    }

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
        InitializeSidekicks();
        StartSidekickPlacement();

        InitializeEnemy("NodeE_1");
        InitializeEnemySidekicks();
        DrawInitialHand(enemy, 5);

        if (combatManager != null)
        {
            combatManager.player = player;
            combatManager.enemy = enemy;
        }

        DrawInitialHand(player, 5);
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
            var movementUI = FindFirstObjectByType<MovementUI>();
            if (movementUI != null)
            {
                movementUI.ResetMovedUnits();
            }

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

    private void InitializeSidekicks()
    {
        foreach (var sidekickData in player.deck.sidekicks)
        {
            GameObject sidekickToken = Instantiate(sidekickTokenPrefab);
            Sidekick sidekick = sidekickToken.GetComponent<Sidekick>();
            sidekick.Initialize(sidekickData, player);
            sidekicksToPlace.Add(sidekick);
        }
    }

    private void InitializeEnemySidekicks()
    {
        foreach (var sidekickData in enemy.deck.sidekicks)
        {
            GameObject sidekickToken = Instantiate(sidekickTokenPrefab);
            Sidekick sidekick = sidekickToken.GetComponent<Sidekick>();
            sidekick.Initialize(sidekickData, enemy);
            PlaceEnemySidekick(sidekick);
        }
    }

    private void PlaceEnemySidekick(Sidekick sidekick)
    {
        var enemyStartZone = enemy.GetStartingNode().zones;
        var validNodes = board.nodes
            .Where(node => node.zones.Intersect(enemyStartZone).Any() && !node.IsOccupied())
            .ToList();

        if (validNodes.Count > 0)
        {
            Node targetNode = validNodes[Random.Range(0, validNodes.Count)];
            sidekick.currentNode = targetNode;
            sidekick.transform.position = targetNode.transform.position;
        }
        else
        {
            Debug.LogError("No valid placement zones available for enemy sidekick");
        }
    }

    private void StartSidekickPlacement()
    {
        currentState = GameState.SidekickPlacement;
        HighlightValidPlacementZones();
        placementUI.ShowPlacementUI(true);
        placementUI.UpdateRemainingSidekicks(sidekicksToPlace.Count);
    }

    public void PlaceSidekick(Node targetNode)
    {
        if (sidekicksToPlace.Count > 0 && IsValidSidekickPlacement(targetNode, player.GetStartingNode().zones))
        {
            var sidekick = sidekicksToPlace[0];
            sidekick.currentNode = targetNode;
            sidekick.transform.position = targetNode.transform.position;
            player.sidekicks.Add(sidekick);
            sidekicksToPlace.RemoveAt(0);

            placementUI.UpdateRemainingSidekicks(sidekicksToPlace.Count);

            if (sidekicksToPlace.Count == 0)
            {
                FinishSidekickPlacement();
            }
            else
            {
                HighlightValidPlacementZones();
            }
        }
    }

    public void FinishSidekickPlacement()
    {
        foreach (Node node in board.nodes)
        {
            node.Highlight(false);
            var renderer = node.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = Color.white;
            }
        }

        placementUI.ShowPlacementUI(false);
        currentState = GameState.Playing;
        turnManager.StartPlayerTurn();
    }

    private void HighlightValidPlacementZones()
    {
        var playerStartZone = player.GetStartingNode().zones;
        foreach (Node node in board.nodes)
        {
            if (IsValidSidekickPlacement(node, playerStartZone))
            {
                node.Highlight(true);
            }
        }
    }

    private bool IsValidSidekickPlacement(Node node, List<string> ownerZones)
    {
        return node.zones.Intersect(ownerZones).Any() && !node.IsOccupied();
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

    public void OnCardDiscarded(Card card)
    {
        player.BoostManeuver(card);
        handDisplay.DisplayHand(player.hand, OnCardDiscarded);
    }
}
