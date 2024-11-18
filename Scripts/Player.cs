using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

public class Player : BaseUnit
{
    [SerializeField]
    public List<Card> hand;
    public Deck deck;
    public SpriteRenderer characterPortrait;
    public List<Sidekick> sidekicks = new List<Sidekick>();
    private bool canBoost = false;
    public bool CanBoost => canBoost;

    public void Initialize(Deck chosenDeck, CombatType type)
    {
        deck = chosenDeck;
        base.Initialize(chosenDeck.startingHealth, type);

        string portraitPath = "Images/Portraits/" + chosenDeck.name;
        Sprite portrait = Resources.Load<Sprite>(portraitPath);
        if (portrait != null)
        {
            characterPortrait.sprite = portrait;
        }
    }

    public void DrawCard()
    {
        if (deck.cards.Count == 0)
        {
            Debug.LogError("Deck is empty, cannot draw a card.");
            return;
        }

        if (hand.Count < 7)
        {
            int index = Random.Range(0, deck.cards.Count);
            Card drawnCard = deck.cards[index];
            deck.cards.RemoveAt(index);
            hand.Add(drawnCard);
            Debug.Log("Drew: " + drawnCard.name + " for " + gameObject.tag);
        }
        else
        {
            Debug.Log(gameObject.tag + " hand is full!");
        }
    }

    public void Maneuver()
    {
        if (actionManager.CanPerformAction(ActionState.Maneuvering))
        {
            actionManager.StartAction(ActionState.Maneuvering);
            DrawCard();
            movement = deck.baseMovement;
            canBoost = true;

            foreach (var sidekick in sidekicks)
            {
                sidekick.movement = deck.baseMovement;
            }

            var movementUI = ServiceLocator.Instance.MovementUI;
            if (movementUI != null)
            {
                List<MonoBehaviour> selectableUnits = new List<MonoBehaviour> { this };
                selectableUnits.AddRange(sidekicks);
                movementUI.StartUnitSelection(selectableUnits, OnUnitSelected);
            }

            ServiceLocator.Instance.HandDisplay?.DisplayHand(hand, SelectCard);
        }
    }

    private void OnUnitSelected(MonoBehaviour selectedUnit)
    {
        if (selectedUnit is Player player)
        {
            player.HighlightNodesInRange();
        }
        else if (selectedUnit is Sidekick sidekick)
        {
            sidekick.HighlightNodesInRange();
        }
    }

    public void DiscardCard(Card card)
    {
        if (hand.Contains(card))
        {
            hand.Remove(card);
            Debug.Log("Discarded: " + card.name + ". Current hand: " + string.Join(", ", hand.Select(c => c.name)));
            ServiceLocator.Instance.HandDisplay?.DisplayHand(hand, SelectCard);
        }
    }

    public void BoostManeuver(Card cardToDiscard)
    {
        if (actionManager.CanPerformAction(ActionState.BoostedManeuvering) && canBoost && hand.Contains(cardToDiscard))
        {
            actionManager.StartAction(ActionState.BoostedManeuvering);
            DiscardCard(cardToDiscard);
            int boostAmount = cardToDiscard.boost;

            movement += boostAmount;

            foreach (var sidekick in sidekicks)
            {
                sidekick.movement += boostAmount;
            }

            canBoost = false;
            Debug.Log($"Boosted movement by: {boostAmount} for all units");

            var movementUI = ServiceLocator.Instance.MovementUI;
            if (movementUI != null && movementUI.CurrentlySelectedUnit != null)
            {
                OnUnitSelected(movementUI.CurrentlySelectedUnit);
            }
        }
    }

    public void UseSchemeCard(Card card)
    {
        if (actionManager.CanPerformAction(ActionState.Scheming))
        {
            actionManager.StartAction(ActionState.Scheming);
            Debug.Log("Using scheme card: " + card.name);
            ServiceLocator.Instance.EffectManager.StartEffect();
            card.TriggerEffect(this, null);
            DiscardCard(card);
            StartCoroutine(WaitForEffectThenEndScheme());
        }
    }

    private IEnumerator WaitForEffectThenEndScheme()
    {
        while (!ServiceLocator.Instance.EffectManager.IsEffectComplete)
        {
            yield return null;
        }

        ServiceLocator.Instance.TurnManager?.PerformAction(TurnManager.ActionType.Scheme);
        actionManager.EndAction();
    }

    public void SelectCard(Card card)
    {
        if (!card.CanBeUsedBy(this))
        {
            Debug.Log($"Card {card.name} cannot be used by {gameObject.name}");
            return;
        }

        if ((actionManager.currentAction == ActionState.Maneuvering ||
             actionManager.currentAction == ActionState.BoostedManeuvering) &&
            CanBoost)
        {
            BoostManeuver(card);
            return;
        }

        switch (card.cardType)
        {
            case CardType.Attack:
            case CardType.Versatile:
                if (actionManager.CanPerformAction(ActionState.Attacking))
                    SelectCardForAttack(card);
                break;

            case CardType.Scheme:
                if (ServiceLocator.Instance.TurnManager.CanPerformAction())
                    UseSchemeCard(card);
                break;
        }
    }

    private void SelectCardForAttack(Card card)
    {
        var enemy = ServiceLocator.Instance.GameManager.enemy;
        var enemySidekick = ServiceLocator.Instance.GameManager.GetSidekickForOwner(enemy);
        List<MonoBehaviour> validAttackers = new List<MonoBehaviour>();

        // Check if player is in valid attack position (against either enemy or their sidekick)
        if ((combatType != CombatType.Melee) ||
            currentNode.IsConnectedTo(enemy.currentNode) ||
            (enemySidekick != null && currentNode.IsConnectedTo(enemySidekick.currentNode)))
        {
            validAttackers.Add(this);
        }

        // Check if sidekicks are in valid attack position
        foreach (var sidekick in sidekicks)
        {
            if (sidekick.combatType != CombatType.Melee ||
                sidekick.currentNode.IsConnectedTo(enemy.currentNode) ||
                (enemySidekick != null && sidekick.currentNode.IsConnectedTo(enemySidekick.currentNode)))
            {
                validAttackers.Add(sidekick);
            }
        }

        if (validAttackers.Count > 0)
        {
            ServiceLocator.Instance.MovementUI.StartUnitSelection(validAttackers, unit =>
            {
                actionManager.StartAction(ActionState.Attacking);
                DiscardCard(card);
                ServiceLocator.Instance.CombatManager.InitiateAttack(unit, card);
            });
        }
        else
        {
            Debug.LogError("No units in range to attack.");
        }
    }

    public Card SelectCardForDefense()
    {
        var validDefenseCards = hand.Where(card =>
            (card.cardType == CardType.Defense || card.cardType == CardType.Versatile) &&
            card.CanBeUsedBy(this)).ToList();

        if (validDefenseCards.Count > 0)
        {
            Card selectedCard = validDefenseCards[0];
            DefendAgainstAttack(selectedCard);
            return selectedCard;
        }
        return null;
    }

    private void DefendAgainstAttack(Card card)
    {
        if (card.cardType == CardType.Defense || card.cardType == CardType.Versatile)
        {
            var combatManager = ServiceLocator.Instance.CombatManager;
            if (combatManager != null)
            {
                DiscardCard(card);
                combatManager.DefendWith(this);
            }
        }
    }

    public void EndManeuver()
    {
        if (actionManager.currentAction == ActionState.Maneuvering ||
            actionManager.currentAction == ActionState.BoostedManeuvering)
        {
            canBoost = false;
            movement = 0;

            foreach (var sidekick in sidekicks)
            {
                sidekick.movement = 0;
                sidekick.ResetHighlights();
            }

            ResetHighlights();
            ServiceLocator.Instance.MovementUI?.ResetMovedUnits();
            actionManager.EndAction();
        }
    }
}
