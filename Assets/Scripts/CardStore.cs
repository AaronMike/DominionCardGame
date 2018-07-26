using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
// CardStore - Maintains the list of avaiable cards for players to purchase on their turn.
//  		   Provides the starting decks for players.
public class CardStore : MonoBehaviour {
	public LevelScript levelScript;

	public List<Card> availableCards;
	List<CardMover>   cardMovers;

	void Start() {
		cardMovers = new List<CardMover>();
	}

	void Update() {
		if (cardMovers.Count > 0) {
			if (cardMovers[0].MoveCards(availableCards)) {
				cardMovers.RemoveAt(0);
			}
		}
	}

	public void SetupCards() {
		for (int i = 0; i < availableCards.Count; ++i) {
			StoreCard clickHandler = availableCards[i]
				.gameObject
				.AddComponent(typeof(StoreCard)) as StoreCard;
			clickHandler.onClick = HandleCardClick;
		}
	}

	public void IssueStartingDeck(Player player) {
		foreach (Card card in availableCards) {
			for (int i = 0; i < card.playerStartCount; ++i) {
				player.AddNewCard(MakeCopy(card));
			}
		}
	}

	public Vector3 GetCenterPosition() {
		return transform.position;
	}

	public void PositionCards(System.EventHandler listener) {
        CardMover cardMover = new CardMover(MoveType.Grid, GetCenterPosition());
        cardMover.CardsFinishedMoving += listener;
		cardMovers.Add(cardMover);
	}

	Card MakeCopy(Card cardToCopy) {
		Card newCard = Instantiate(cardToCopy);	
		Destroy(newCard.GetComponent<StoreCard>());
		Destroy(newCard.transform.Find("StackSize").gameObject);
		newCard.transform.position = transform.position;
		return newCard;
	}

	void HandleCardClick(Card cardTemplate) {
		if (cardTemplate.storeStackSize <= 0) {
			Debug.Log("This card cannot be purchased anymore.");
			return;
		}

		Player player = levelScript.GetCurrentPlayer();
		if (!player.CanBuyCard(cardTemplate)) {
			Debug.Log("You are unable to buy this card.");
			return;
		}

		Card newCard = MakeCopy(cardTemplate);
		if (player.BuyCard(newCard)) {
			--cardTemplate.storeStackSize;
		} else {
			Destroy(newCard);
		}
	}
	
	class StoreCard : CardClickHandler {
		void Update() {
			GameObject stackSize = transform.Find("StackSize").gameObject;
			if (stackSize) {
				Card card = gameObject.GetComponent<Card>();
				stackSize.GetComponent<TextMesh>().text = string.Format("x{0}", card.storeStackSize);
			}
		}	
	}
}
