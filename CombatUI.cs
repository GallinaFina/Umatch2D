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
        attackerCardSlot.sprite = card.isFaceDown ? cardBackSprite : card.sprite;
    }

    public void DisplayDefenderCard(Card card)
    {
        if (card != null)
        {
            defenderCardSlot.sprite = card.isFaceDown ? cardBackSprite : card.sprite;
        }
        else
        {
            defenderCardSlot.sprite = null;
        }
    }
}
