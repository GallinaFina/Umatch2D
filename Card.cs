using System;

[System.Serializable]
public class Card
{
    public string name;
    public int power;
    public int boost;
    public string type;  // Keep original type for JSON
    public CardType cardType;  // Internal use for type safety
    public string ability;
    public int count;
    public string imagePath;

    public Card(string name, int power, int boost, string type, string ability, int count = 1, string imagePath = "")
    {
        this.name = name;
        this.power = power;
        this.boost = boost;
        this.type = type;  // Keep type as string for JSON parsing
        this.cardType = (CardType)Enum.Parse(typeof(CardType), type, true);  // Convert to enum for internal use
        this.ability = ability;
        this.count = count;
        this.imagePath = imagePath;
    }
}

public enum CardType
{
    Attack,
    Defend,
    Versatile,
    Scheme
}
