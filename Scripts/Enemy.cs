using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Enemy : MonoBehaviour
{
    public List<Card> hand;
    private Deck deck;
    public Node currentNode;
    public Node startingNode;
    private ActionManager actionManager;
    public int maxHP;
    public int currentHP;

    void Start()
    {
        actionManager = gameObject.AddComponent<ActionManager>();
    }

    public void Initialize(Deck chosenDeck)
    {
        deck = chosenDeck;
        maxHP = deck.startingHealth;
        currentHP = maxHP;
        SetStartingNode();
    }

    public void SetStartingNode()
    {
        startingNode = currentNode;
    }

    public Node GetStartingNode()
    {
        return startingNode;
    }

    public void DrawCard()
    {
        if (deck.cards.Count == 0)
        {
            Debug.LogError("Deck is empty, cannot draw a card.");
            return;
        }

        if (hand.Count < 7)
        {
            int index = Random.Range(0, deck.cards.Count);
            Card drawnCard = deck.cards[index];
            deck.cards.RemoveAt(index);
            hand.Add(drawnCard);
            Debug.Log("Drew: " + drawnCard.name + " for " + gameObject.tag);
        }
        else
        {
            Debug.Log(gameObject.tag + " hand is full!");
        }
    }

    public void DiscardCard(Card card)
    {
        if (hand.Contains(card))
        {
            hand.Remove(card);
            Debug.Log("Discarded: " + card.name + ". Current hand: " + string.Join(", ", hand.Select(c => c.name)));
        }
    }

    public Card SelectDefenseCard()
    {
        Debug.Log($"Enemy hand contains {hand.Count} cards");
        Debug.Log($"Hand contents: {string.Join(", ", hand.Select(c => $"{c.name} ({c.cardType})"))}");

        var validDefenseCards = hand.Where(card =>
            card.cardType == CardType.Defense ||
            card.cardType == CardType.Versatile).ToList();

        Debug.Log($"Found {validDefenseCards.Count} valid defense cards: {string.Join(", ", validDefenseCards.Select(c => c.name))}");

        if (validDefenseCards.Count > 0)
        {
            int randomIndex = Random.Range(0, validDefenseCards.Count);
            Card selectedCard = validDefenseCards[randomIndex];
            Debug.Log($"Selected {selectedCard.name} ({selectedCard.cardType}) for defense");
            DiscardCard(selectedCard);
            return selectedCard;
        }

        Debug.Log("No valid defense cards found");
        return null;
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        Debug.Log($"{gameObject.tag} took {amount} damage. HP: {currentHP}/{maxHP}");
    }
}
