using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Card
{
    public string name;
    public int power;
    public int boost;
    [SerializeField]
    public string type;
    public CardType cardType { get; private set; }
    public string ability;
    public int count;
    public string imagePath;
    public bool isFaceDown;
    public CardEffectTiming effectTiming;
    public UnityEvent OnEffectTriggered;
    public CardUser cardUser { get; private set; }
    public string allowedUser;

    public Card(string name, int power, int boost, CardType type, string ability,
                int count, string imagePath, CardEffectTiming timing = CardEffectTiming.DuringCombat,
                CardUser user = CardUser.Any, string specificUser = "")
    {
        this.name = name;
        this.power = power;
        this.boost = boost;
        this.cardType = type;
        this.ability = ability;
        this.count = count;
        this.imagePath = imagePath;
        this.isFaceDown = false;
        this.effectTiming = timing;
        this.cardUser = user;
        this.allowedUser = specificUser;
        OnEffectTriggered = new UnityEvent();
    }

    public void TriggerEffect(MonoBehaviour source, MonoBehaviour target)
    {
        Debug.Log($"Triggering {effectTiming} effect for {name}");
        EffectManager.Instance.ResolveCardEffect(this, source, target);
    }

    public bool CanBeUsedBy(MonoBehaviour entity)
    {
        switch (cardUser)
        {
            case CardUser.Any:
                return true;
            case CardUser.MainCharacter:
                return entity is Player || entity is Enemy;
            case CardUser.Sidekick:
                return entity is Sidekick;
            case CardUser.SpecificSidekick:
                return entity is Sidekick sidekick && sidekick.sidekickName == allowedUser;
            default:
                return false;
        }
    }
}
