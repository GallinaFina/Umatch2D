using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public List<Deck> allDecks = new List<Deck>();

    void Start()
    {
        ServiceLocator.Instance.RegisterService(this);
        LoadDecks();
    }

    void LoadDecks()
    {
        var deckConfigs = Resources.LoadAll<DeckConfigSO>("ScriptableObjects/Decks");
        foreach (var deckConfig in deckConfigs)
        {
            Deck deck = deckConfig.CreateDeck();
            allDecks.Add(deck);
            Debug.Log($"Loaded deck: {deck.name}");
        }
        Debug.Log($"Total decks loaded: {allDecks.Count}");
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
