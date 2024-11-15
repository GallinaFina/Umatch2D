using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

[System.Serializable]
public class SidekickDataJson
{
    public string name;
    public int startingHealth;
    public string combatType;
}

[System.Serializable]
public class DeckJson
{
    public string name;
    public int startingHealth;
    public int baseMovement;
    public List<Card> cards;
    public List<SidekickDataJson> sidekicks;
}

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
                DeckJson deckJson = JsonUtility.FromJson<DeckJson>(json);
                Deck deck = new Deck(deckJson.name, new List<Card>(), deckJson.baseMovement, deckJson.startingHealth);

                List<Card> originalCards = new List<Card>(deckJson.cards);
                List<Card> expandedCards = new List<Card>();

                foreach (Card card in originalCards)
                {
                    for (int i = 0; i < card.count; i++)
                    {
                        CardType parsedType = (CardType)System.Enum.Parse(typeof(CardType), card.type);
                        CardEffectTiming timing = DetermineEffectTiming(card.ability);
                        CardUser cardUser = DetermineCardUser(card.allowedUser);

                        expandedCards.Add(new Card(
                            card.name,
                            card.power,
                            card.boost,
                            parsedType,
                            card.ability,
                            1,
                            card.imagePath,
                            timing,
                            cardUser,
                            card.allowedUser
                        ));
                    }
                }
                deck.cards = expandedCards;

                // Process sidekicks
                foreach (var sidekickJson in deckJson.sidekicks)
                {
                    Player.CombatType combatType = (Player.CombatType)System.Enum.Parse(typeof(Player.CombatType), sidekickJson.combatType);
                    SidekickData sidekickData = new SidekickData
                    {
                        name = sidekickJson.name,
                        health = sidekickJson.startingHealth,
                        combatType = combatType
                    };
                    deck.sidekicks.Add(sidekickData);
                    Debug.Log($"Loaded sidekick: {sidekickData.name} with health: {sidekickData.health}");
                }

                allDecks.Add(deck);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load deck from file: " + file + "\n" + e.Message);
            }
        }
        Debug.Log("Total decks loaded: " + allDecks.Count);
    }

    private CardEffectTiming DetermineEffectTiming(string ability)
    {
        if (string.IsNullOrEmpty(ability)) return CardEffectTiming.None;

        if (ability.StartsWith("Immediately:"))
            return CardEffectTiming.Immediately;
        if (ability.StartsWith("During combat:"))
            return CardEffectTiming.DuringCombat;
        if (ability.StartsWith("After combat:"))
            return CardEffectTiming.AfterCombat;

        return CardEffectTiming.None;
    }

    private CardUser DetermineCardUser(string allowedUser)
    {
        if (string.IsNullOrEmpty(allowedUser)) return CardUser.Any;
        if (allowedUser == "MainCharacter") return CardUser.MainCharacter;
        if (allowedUser == "Sidekick") return CardUser.Sidekick;
        return CardUser.SpecificSidekick;
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
