using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 
// Player - Owns and manipulates cards by moving them into the different CardPiles.
public class Player : MonoBehaviour {
	public int id;

	// UI
	public Text debugText;
	public Button playCardButton; 
	public Button discardButton;

	Card      cardBeingDragged;
	CardMover cardBeingDraggedMover;
	Card      cardBeingViewed;
	CardMover cardBeingViewedMover;

	// CardPiles
	CardPile deck;
	CardPile hand;
	CardPile discard;

	// Turn temporary variables
	bool turnActive = false;
    int actionsRemaining = 0;
	int purchasesRemaining = 0;
	int money = 0;

	TextMesh deckText;
	TextMesh discardText;

	Vector3 deckPos;
	Vector3 discardPos;

	void Start() {
		deck = new CardPile();
		hand = new CardPile();
		discard = new CardPile();

		Transform deckTransform = transform.Find("Deck");
		deckPos = deckTransform.position;
		deckText = deckTransform.Find("StackSize").GetComponent<TextMesh>();

		Transform discardTransform = transform.Find("Discard");
		discardPos = discardTransform.position;
		discardText = discardTransform.Find("StackSize").GetComponent<TextMesh>();

		playCardButton.onClick.AddListener(PlayCardBeingViewed);
		discardButton.onClick.AddListener(DiscardCardBeingViewed);
	}
	
	void Update () {
		string text = string.Format("Gold: {0}", money);
		text += string.Format("\nActions: {0}", actionsRemaining);
		text += string.Format("\nBuys: {0}", purchasesRemaining);
		text += string.Format("\nDeck: {0}, Discard: {1}, Hand: {2}", deck.cards.Count, discard.cards.Count, hand.cards.Count);
		debugText.text = text;

		deckText.text = string.Format("x{0}", deck.cards.Count);
		discardText.text = string.Format("x{0}", discard.cards.Count);

		if (cardBeingViewedMover != null) {
			List<Card> ignoreList = new List<Card>();
			ignoreList.Add(cardBeingViewed);
			cardBeingViewedMover.MoveCards(ignoreList);
		}

		deck.UpdateMovement();
		hand.UpdateMovement(CardsIgnoringMovement());
		discard.UpdateMovement();

		PositionDraggedCard();
	}

	List<Card> CardsIgnoringMovement() 
	{
		// Hack for now to make sure that the hand movers don't move the card being viewed or dragged card too
		List<Card> toIgnore = new List<Card>();
		
		if (cardBeingDragged != null) {
			toIgnore.Add(cardBeingDragged);
		}

		if (cardBeingViewed != null) {
			toIgnore.Add(cardBeingViewed);
		}

		return toIgnore;
	}

	public void StartTurn() {
		actionsRemaining = LevelScript.kStartingActionsPerTurn;
		purchasesRemaining = LevelScript.kPurchasesPerTurn;
		turnActive = true;
	}

	public void EndTurn() {
		turnActive = false;
		money = 0;
		actionsRemaining = 0;
		purchasesRemaining = 0;
	}

	public void ShuffleDeck() {
		deck.Shuffle();
	}

	public void CycleDiscard() {
		for (int i = 0; i < discard.cards.Count; ) {
			Card cycledCard = discard.cards[i];
			deck.cards.Insert(Random.Range(0, deck.cards.Count - 1), cycledCard);
			discard.cards.Remove(cycledCard);
		}
	}

	public void DrawCards(int total) {
		for (int i = 0; i < total; ++i) {
			if (deck.cards.Count == 0) {
				// No more cards to draw, can we cycle the discard?
				if (discard.cards.Count > 0) {
					CycleDiscard();
				} else {
					// No more cards there either
					return;
				}
			}
			DrawCard(deck.cards[0]);
		}
		return;
	}

	void DrawCard(Card card) {
		Card drawnCard = deck.cards[0];
		hand.cards.Add(drawnCard);
		deck.cards.Remove(drawnCard);
		PlayerHandClickHandler clickHandler = drawnCard
			.gameObject
			.AddComponent(typeof(PlayerHandClickHandler)) as PlayerHandClickHandler;
		clickHandler.onClick = ViewCard;
		clickHandler.onDrag = StartDraggingCard;		
		clickHandler.onUp = StopDraggingCard;
	}

	void DiscardCard(Card card) {
		ClearCardBeingViewedIfEqualTo(card);
		hand.cards.Remove(card);
		Destroy(card.GetComponent<PlayerHandClickHandler>());
		discard.cards.Add(card);
	}

	public void DiscardAllCards() {
		while (hand.cards.Count > 0) {
			DiscardCard(hand.cards[0]);
		}
	}

	public bool CanPlayCard(Card card) {
		return turnActive && 
			(!card.requiresAction || actionsRemaining > 0);
	}

    public void PlayCard(Card card) {
		if (CanPlayCard(card)) {
			card.PlayCard(this);
			if (card.requiresAction) {
				--actionsRemaining;
			}
			DiscardCard(card);
			PositionCards();
		}
	}

	void StartDraggingCard(Card card) {
		cardBeingDragged = card;
		cardBeingDraggedMover = new CardMover(MoveType.MouseFollow);
	}

	void StopDraggingCard(Card card) {
		cardBeingDragged = null;
		cardBeingDraggedMover = null;
		PositionCards();
	}

	void PositionDraggedCard() {
		if (cardBeingDragged != null && cardBeingDraggedMover != null) {
			cardBeingDraggedMover.MoveCard(cardBeingDragged);
		}
	}

	void ViewCard(Card card) {
		if (!ClearCardBeingViewedIfEqualTo(card)) {
			Debug.Log("Viewing card");
			cardBeingViewed = card;
			cardBeingViewedMover = new CardMover(MoveType.Stack, GetCardViewPosition());
			cardBeingViewedMover.CardsFinishedMoving += new System.EventHandler((object sender, System.EventArgs e) => {
				playCardButton.gameObject.SetActive(true);
				discardButton.gameObject.SetActive(true);
			});
		} else {
			Debug.Log("Nope");
		}
		PositionCards();
	}

	void PlayCardBeingViewed() {
		if (cardBeingViewed != null) {
			PlayCard(cardBeingViewed);
		}
	}

	void DiscardCardBeingViewed() {
		if (cardBeingViewed != null) {
			DiscardCard(cardBeingViewed);
			PositionCards();
		}
	}

	bool ClearCardBeingViewedIfEqualTo(Card card) {
		if (card == cardBeingViewed) {
			cardBeingViewed = null;
			cardBeingViewedMover = null;
			playCardButton.gameObject.SetActive(false);
			discardButton.gameObject.SetActive(false);
			return true;
		}
		return false;
	}

	public bool CanBuyCard(Card card) {
		return purchasesRemaining > 0 && money >= card.cardCost;
	}

	public bool BuyCard(Card card) {
		if (CanBuyCard(card)) {
			money -= card.cardCost;
			--purchasesRemaining;
			AddUsedCard(card);
			PositionCards();
			return true;
		}
		return false;
	}

	public void AddUsedCard(Card card) {
		discard.cards.Add(card);
	}

	public void AddNewCard(Card card) {
		deck.cards.Add(card);
	}

	public Vector3 GetDeckPosition() {
		return deckPos;
	}

	public Vector3 GetHandPosition() {
		return transform.position;
	}

	public Vector3 GetDiscardPosition() {
		return discardPos;
	}

	public void AddMoney(int amount) {
		money += amount;
	}

	public void AddActions(int amount) {
		actionsRemaining += amount;
	}

	bool IsCardBeingMovedElsewhere(Card card) {
		return card == cardBeingViewed ||
				card == cardBeingDragged;
	}

	public void PositionCards(System.EventHandler listener = null) {
        int counter = 3;
        var onReady = new System.EventHandler((object sender, System.EventArgs e) => {
            if (--counter == 0) {
				if (listener != null) {
					listener.Invoke(sender, e);
				}
            }
        });
		PositionDeck(onReady);
		PositionHand(onReady);
		PositionDiscard(onReady);
	}

	public void PositionDeck() {
		PositionDeck();
	}

	public void PositionDeck(System.EventHandler listener = null) {
        CardMover cardMover = new CardMover(MoveType.Stack, GetDeckPosition());
		if (listener != null) {
        	cardMover.CardsFinishedMoving += listener;
		}
		deck.AddMovement(cardMover);
	}

	public void PositionHand(System.EventHandler listener = null) {
        CardMover cardMover = new CardMover(MoveType.Fan, GetHandPosition());
		if (listener != null) {
        	cardMover.CardsFinishedMoving += listener;
		}
		hand.AddMovement(cardMover);
	}

	public void PositionDiscard(System.EventHandler listener = null) {
        CardMover cardMover = new CardMover(MoveType.Stack, GetDiscardPosition());
		if (listener != null) {
        	cardMover.CardsFinishedMoving += listener;
		}
		discard.AddMovement(cardMover);
	}

    Vector3 GetCardViewPosition() {
        return new Vector3(-0.8f, -0.12f, -8.2f);
    }
}

class PlayerHandClickHandler : CardClickHandler { }