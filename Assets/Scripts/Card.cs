using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
// Cards - Have their effect amounts and are what's passed around during the game.
public class Card : MonoBehaviour
{
    public LevelScript levelScript;
    
    public int  cardCost = 0;
    public int  storeStackSize = 5;
    public int  playerStartCount = 0;

    public int  actionsGainedOnPlay = 0;
    public int  moneyGainedOnPlay = 0;
    public int  cardsGainedOnPlay = 0;

    public bool requiresAction = false;

    public float moveSpeed = 0.01f;

    void Start() {
        if (cardCost > 0) {
            transform.Find("CardCost").GetComponent<TextMesh>().text = cardCost.ToString();
        }
    }

    public bool MoveTowards(Vector3 targetPos) {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed);
        return Vector3.Distance(transform.position, targetPos) <= 0.001f;
    }

    public bool MoveTowards(Vector3 targetPos, float moveSpeedOverride) {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeedOverride);
        return Vector3.Distance(transform.position, targetPos) <= 0.001f;
    }

    public void PlayCard(Player player) {
        player.AddMoney(moneyGainedOnPlay);
        player.AddActions(actionsGainedOnPlay);
        player.DrawCards(cardsGainedOnPlay);
    }
}