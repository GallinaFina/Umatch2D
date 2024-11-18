using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatUI : MonoBehaviour
{
    public GameObject combatPanel;
    public Image attackerCardSlot;
    public Image defenderCardSlot;
    public TextMeshProUGUI phaseText;
    public Sprite cardBackSprite;

    public enum CombatPhase
    {
        AttackerSelection,
        DefenderSelection,
        ImmediateEffects,
        DuringCombatEffects,
        Resolution,
        AfterCombatEffects
    }

    private CombatPhase currentPhase;

    void Start()
    {
        ServiceLocator.Instance.RegisterService(this);
    }

    public void ShowCombatUI(bool show)
    {
        combatPanel.SetActive(show);
    }

    public void UpdatePhase(CombatPhase phase)
    {
        currentPhase = phase;
        phaseText.text = $"Combat Phase: {phase}";
    }

    public void DisplayAttackerCard(Card card)
    {
        attackerCardSlot.sprite = card.isFaceDown ? cardBackSprite : LoadCardImage(card.imagePath);
    }

    public void DisplayDefenderCard(Card card)
    {
        if (card != null)
        {
            defenderCardSlot.sprite = card.isFaceDown ? cardBackSprite : LoadCardImage(card.imagePath);
        }
        else
        {
            defenderCardSlot.sprite = null;
        }
    }

    private Sprite LoadCardImage(string imagePath)
    {
        return Resources.Load<Sprite>(imagePath);
    }
}
