using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Deck
{
    public string name;
    public List<Card> cards;
    private const int maxCards = 30;
    public int baseMovement;
    public int startingHealth;

    public Deck(string name, List<Card> cards, int baseMovement, int startingHealth)
    {
        this.name = name;
        this.cards = cards;
        this.baseMovement = baseMovement;
        this.startingHealth = startingHealth;
    }

    public void AddCard(Card card)
    {
        for (int i = 0; i < card.count; i++)
        {
            if (cards.Count < maxCards)
            {
                cards.Add(card);
                Debug.Log("Added card: " + card.name + " (Count: " + card.count + ")");
            }
            else
            {
                Debug.Log("Deck is full!");
                break;
            }
        }
    }
}
