using System.Collections.Generic;
using UnityEngine;

//
// CardPile -  Houses a list of cards and movers to move them around.
class CardPile {
	public List<Card>      cards;
	public List<CardMover> movers;

	public enum Location {
		Deck,
		Hand,
		Discard
	}

	public CardPile() {
		this.cards = new List<Card>();
		this.movers = new List<CardMover>();
	}

	public void UpdateMovement() {
		UpdateMovement(new List<Card>());
	}

	public void UpdateMovement(List<Card> toIgnore) {
		if (movers.Count > 0) {
			bool finished = movers[0].MoveCards(cards, toIgnore);
			if (finished) {
				movers.RemoveAt(0);
			}
		}
	}
	
	public void Shuffle() {
		for (int i = 0; i < cards.Count; ++i) {
			Card tempCard = cards[i];
			int newIndex = Random.Range(0, cards.Count - 1);
			cards[i] = cards[newIndex];
			cards[newIndex] = tempCard;
		}	
	}

	public void AddMovement(CardMover cardMover) {
		movers.Add(cardMover);
	}
}