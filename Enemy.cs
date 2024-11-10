using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Enemy : MonoBehaviour
{
    public List<Card> hand;
    private Deck deck;
    public Node currentNode;
    private ActionManager actionManager;

    void Start()
    {
        actionManager = gameObject.AddComponent<ActionManager>();
    }

    public void Initialize(Deck chosenDeck)
    {
        deck = chosenDeck;
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
        var validDefenseCards = hand.Where(card =>
            card.cardType == CardType.Defense ||
            card.cardType == CardType.Versatile).ToList();

        if (validDefenseCards.Count > 0)
        {
            int randomIndex = Random.Range(0, validDefenseCards.Count);
            Card selectedCard = validDefenseCards[randomIndex];
            DiscardCard(selectedCard);
            return selectedCard;
        }

        return null;
    }
}