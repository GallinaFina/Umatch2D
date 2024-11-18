using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Enemy : BaseUnit
{
    public List<Card> hand;
    public Deck deck;
    public SpriteRenderer characterPortrait;

    public void Initialize(Deck chosenDeck)
    {
        deck = chosenDeck;
        base.Initialize(deck.startingHealth, CombatType.Melee);

        string portraitPath = "Images/Portraits/" + chosenDeck.name;
        Sprite portrait = Resources.Load<Sprite>(portraitPath);
        if (portrait != null && characterPortrait != null)
        {
            characterPortrait.sprite = portrait;
        }
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

    public void Maneuver()
    {
        if (actionManager.CanPerformAction(ActionState.Maneuvering))
        {
            actionManager.StartAction(ActionState.Maneuvering);
            DrawCard();
            movement = deck.baseMovement;

            var sidekick = ServiceLocator.Instance.GameManager.GetSidekickForOwner(this);
            if (sidekick != null)
            {
                sidekick.movement = deck.baseMovement;
                sidekick.HighlightNodesInRange();
            }

            HighlightNodesInRange();
        }
    }

    public void EndManeuver()
    {
        if (actionManager.currentAction == ActionState.Maneuvering ||
            actionManager.currentAction == ActionState.BoostedManeuvering)
        {
            actionManager.EndAction();
            movement = 0;

            var sidekick = ServiceLocator.Instance.GameManager.GetSidekickForOwner(this);
            if (sidekick != null)
            {
                sidekick.movement = 0;
                sidekick.ResetHighlights();
            }

            ResetHighlights();
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
}
