using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HandDisplay : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform handPanel;
    private List<GameObject> cardInstances = new List<GameObject>();

    public void DisplayHand(List<Card> hand, System.Action<Card> onCardClicked = null)
    {
        // Clear existing card instances
        foreach (GameObject cardInstance in cardInstances)
        {
            Destroy(cardInstance);
        }
        cardInstances.Clear();

        // Add new cards
        for (int i = 0; hand != null && i < hand.Count; i++)
        {
            Card card = hand[i];
            GameObject newCard = Instantiate(cardPrefab, handPanel);

            // Position each card
            RectTransform cardTransform = newCard.GetComponent<RectTransform>();
            cardTransform.anchoredPosition = new Vector2(100 * i, 0);

            // Load and set card image
            Image cardImage = newCard.GetComponentInChildren<Image>();
            if (cardImage != null && !string.IsNullOrEmpty(card.imagePath))
            {
                Sprite sprite = Resources.Load<Sprite>(card.imagePath);
                cardImage.sprite = sprite;
                cardImage.SetNativeSize();
            }

            // Add Button and click event to discard
            Button cardButton = newCard.GetComponent<Button>();
            if (cardButton == null) cardButton = newCard.AddComponent<Button>();

            Card capturedCard = card; // Capture the current card in the loop
            cardButton.onClick.AddListener(() => onCardClicked?.Invoke(capturedCard));

            cardInstances.Add(newCard);
        }
    }
}
