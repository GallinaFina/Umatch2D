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

    public Card(string name, int power, int boost, CardType type, string ability, int count, string imagePath, CardEffectTiming timing = CardEffectTiming.DuringCombat)
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
        OnEffectTriggered = new UnityEvent();
    }

    public void TriggerEffect(MonoBehaviour source, MonoBehaviour target)
    {
        switch (effectTiming)
        {
            case CardEffectTiming.Immediately:
                HandleImmediateEffect(source, target);
                break;
            case CardEffectTiming.DuringCombat:
                HandleDuringCombatEffect(source, target);
                break;
            case CardEffectTiming.AfterCombat:
                HandleAfterCombatEffect(source, target);
                break;
        }

        OnEffectTriggered?.Invoke();
    }

    private void HandleImmediateEffect(MonoBehaviour source, MonoBehaviour target)
    {
        Debug.Log($"Triggering immediate effect for {name}");
    }

    private void HandleDuringCombatEffect(MonoBehaviour source, MonoBehaviour target)
    {
        Debug.Log($"Triggering during combat effect for {name}");
        if (cardType == CardType.Attack || cardType == CardType.Versatile)
        {
            if (target is Player playerTarget)
                playerTarget.TakeDamage(power);
            else if (target is Enemy enemyTarget)
                enemyTarget.TakeDamage(power);
        }
    }

    private void HandleAfterCombatEffect(MonoBehaviour source, MonoBehaviour target)
    {
        Debug.Log($"Triggering after combat effect for {name}");
    }
}
