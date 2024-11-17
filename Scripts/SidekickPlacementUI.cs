using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SidekickPlacementUI : MonoBehaviour
{
    public TextMeshProUGUI remainingSidekicksText;
    public GameObject placementPanel;
    public GameObject confirmationPanel;
    public Button confirmButton;
    public Color validPlacementColor = new Color(0.5f, 1f, 0.5f, 1f);
    public Color invalidPlacementColor = new Color(1f, 0.5f, 0.5f, 1f);

    private void Start()
    {
        confirmButton.onClick.AddListener(ConfirmPlacement);
        confirmationPanel.SetActive(false);
    }

    public void UpdateRemainingSidekicks(int count)
    {
        remainingSidekicksText.text = $"Sidekicks to Place: {count}";
        if (count == 0)
        {
            ShowConfirmation(true);
        }
    }

    public void ShowPlacementUI(bool show)
    {
        placementPanel.SetActive(show);
        if (!show)
        {
            confirmationPanel.SetActive(false);
            ResetAllNodeColors();
        }
    }

    public void ShowConfirmation(bool show)
    {
        confirmationPanel.SetActive(show);
        placementPanel.SetActive(!show);
    }

    public void HighlightHoveredNode(Node node, bool isValid)
    {
        var renderer = node.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = isValid ? validPlacementColor : Color.white;
        }
    }

    private void ResetAllNodeColors()
    {
        var nodes = FindObjectsByType<Node>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var node in nodes)
        {
            var renderer = node.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = Color.white;
            }
        }
    }

    private void ConfirmPlacement()
    {
        ResetAllNodeColors();
        var game = FindFirstObjectByType<Game>();
        game.FinishSidekickPlacement();
    }
}
