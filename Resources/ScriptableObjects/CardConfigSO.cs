using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Game/Card Configuration")]
public class CardConfigSO : ScriptableObject
{
    public string cardName;
    public int power;
    public int boost;
    public CardType cardType;
    public string ability;
    public int count;
    public string imagePath;
    public CardEffectTiming effectTiming;
    public CardUser cardUser = CardUser.Any;
    public string allowedUser;

    public Card CreateCard()
    {
        return new Card(
            cardName,
            power,
            boost,
            cardType,
            ability,
            count,
            imagePath,
            effectTiming,
            cardUser,
            allowedUser
        );
    }
}

