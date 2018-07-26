using UnityEngine;
using System;
using System.Collections.Generic;

public enum MoveType {
    Stack,
    Fan,
    Grid,
    MouseFollow
}

//
// CardMover - Used to move a collection of cards (CardStore, Deck, Hand, Discard) around the screen.
//             Triggers movement completion through the CardsFinishedMoving event handler.
public class CardMover {

    MoveType moveType;
    Vector3  targetCenterPos;

    bool     dimensionsSet = false;
    Vector2  cardSize = Vector2.zero;
    Vector2  cardPadding = Vector2.zero;

    public int cardsPerGridRow = 3;

    public event EventHandler CardsFinishedMoving;

    public CardMover(MoveType moveType) {
        this.moveType = moveType;
    }

    public CardMover(MoveType moveType, Vector3 targetCenterPos) {
        this.moveType = moveType;
        this.targetCenterPos = targetCenterPos;
    }

    public bool MoveCard(Card card) {
        VerifyCardDimenionsSet(card);

        bool allFinished = MoveCardTowards(card, ComputePosition(0, 1));
        if (allFinished && CardsFinishedMoving != null) {
            CardsFinishedMoving(this, EventArgs.Empty);
        }
        return allFinished;
    }

    public bool MoveCards(List<Card> cards, List<Card> toIgnore = null) {
        bool allFinished = true;
        int cardCount = cards.Count;
		if (cardCount > 0) {
            VerifyCardDimenionsSet(cards[0]);
            
			for (int i = 0; i < cardCount; ++i) {
                if (toIgnore != null && toIgnore.Contains(cards[i])) {
                    continue;
                }
				allFinished &= MoveCardTowards(cards[i], ComputePosition(i, cardCount));
			}
		}

        if (allFinished && CardsFinishedMoving != null) {
            CardsFinishedMoving(this, EventArgs.Empty);
        }

        return allFinished;
    }

    bool MoveCardTowards(Card card, Vector3 target) {
        if (moveType == MoveType.MouseFollow) {
            float moveSpeedOverride = 1.0f;
            return card.MoveTowards(target, moveSpeedOverride);
        }
        return card.MoveTowards(target);
    }

    void VerifyCardDimenionsSet(Card card) {
        if (!dimensionsSet) {
            var cardRenderer = card.GetComponent<Renderer>();
            cardSize = new Vector2(cardRenderer.bounds.size.x, cardRenderer.bounds.size.y);
            cardPadding = cardSize / 5;
            dimensionsSet = true;
        }
    }

    Vector3 ComputePosition(int cardIndex, int cardCount) {
        Vector3 offset = Vector3.zero;
        switch (moveType) {
            case MoveType.Fan: {
                float baseXPos = -((cardCount * cardSize.x) + (cardCount * cardPadding.x)) / 2; 
                float xPos = baseXPos + cardIndex * (cardSize.x + cardPadding.x); 
                return targetCenterPos + new Vector3(xPos, 0.0f, -0.5f);
            }

            case MoveType.Stack: 
                break; // No offset

            case MoveType.Grid: {
                int row = cardIndex / cardsPerGridRow;
                int column = cardIndex % cardsPerGridRow;
                offset = new Vector3(
                    column * (cardSize.x + cardPadding.x),
                    row * (cardSize.y + cardPadding.y),
                    0.0f);
                return targetCenterPos + offset;
            }

            case MoveType.MouseFollow: {
                Plane plane = new Plane(Vector3.forward, 1.0f);
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                float enter = 0.0f;
                if (plane.Raycast(ray, out enter)) {
                    Vector3 hitPoint = ray.GetPoint(enter);
                    return hitPoint;
                }
                break;
            }
        }
        return targetCenterPos;
    }
}