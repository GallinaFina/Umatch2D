using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class DeckManager : MonoBehaviour
{
    public List<Deck> allDecks = new List<Deck>();

    void Start()
    {
        LoadDecks();
    }

    void LoadDecks()
    {
        string path = "Assets/JSON/Decks/";
        Debug.Log("Loading decks from path: " + path);
        foreach (string file in Directory.GetFiles(path, "*.json"))
        {
            try
            {
                string json = File.ReadAllText(file);
                Deck deck = JsonUtility.FromJson<Deck>(json);

                Debug.Log("Loaded deck: " + deck.name + " with baseMovement: " + deck.baseMovement);

                List<Card> originalCards = new List<Card>(deck.cards);

                List<Card> expandedCards = new List<Card>();
                foreach (Card card in originalCards)
                {
                    for (int i = 0; i < card.count; i++)
                    {
                        expandedCards.Add(new Card(card.name, card.power, card.boost, card.type, card.ability, 1, card.imagePath));
                    }
                }
                deck.cards = expandedCards;
                allDecks.Add(deck);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load deck from file: " + file + "\n" + e.Message);
            }
        }
        Debug.Log("Total decks loaded: " + allDecks.Count);
    }



    public Deck GetDeck(string deckName)
    {
        Deck foundDeck = allDecks.Find(deck => deck.name == deckName);
        if (foundDeck == null)
        {
            Debug.LogError("Deck not found: " + deckName);
        }
        return foundDeck;
    }
}
