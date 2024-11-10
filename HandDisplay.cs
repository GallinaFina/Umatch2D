using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HandDisplay : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform handArea;

    public void DisplayHand(List<Card> cards, System.Action<Card> onCardSelected)
    {
        Debug.Log($"CardPrefab reference: {cardPrefab != null}");
        Debug.Log($"HandArea reference: {handArea != null}");
        Debug.Log($"Cards count: {cards.Count}");

        ClearHand();
        foreach (Card card in cards)
        {
            GameObject cardObj = Instantiate(cardPrefab, handArea);

            // Position and scale the card
            RectTransform cardTransform = cardObj.GetComponent<RectTransform>();
            cardTransform.localScale = new Vector3(0.5f, 0.5f, 1f);
            cardTransform.anchoredPosition = new Vector2(100 * cards.IndexOf(card), 0);

            Image cardImage = cardObj.GetComponentInChildren<Image>();
            if (cardImage != null && !string.IsNullOrEmpty(card.imagePath))
            {
                Sprite sprite = Resources.Load<Sprite>(card.imagePath);
                cardImage.sprite = sprite;
                cardImage.SetNativeSize();
            }

            Button button = cardObj.GetComponent<Button>();
            if (button == null) button = cardObj.AddComponent<Button>();
            button.onClick.AddListener(() => onCardSelected(card));
        }
    }

    private void ClearHand()
    {
        foreach (Transform child in handArea)
        {
            Destroy(child.gameObject);
        }
    }
}
