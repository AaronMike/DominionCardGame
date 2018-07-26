using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//
// LevelScript - Responsible for setting up the game and keeping gamestate
public class LevelScript : MonoBehaviour {
    public Player player1;
    public Player player2;
    Player currentPlayer;

    public CardStore cardStore;

    public Button passButton;

    public const int kStartingActionsPerTurn = 1;
    public const int kPurchasesPerTurn = 1;

    enum GameState {
        SetupCards,
        StartGame,
        PlayerTurn,
        PlayerTurnEnded,
        ChangingPlayer,
        WaitingForCardMovement,
    }

    GameState gameState;

    public void PlayCard(Card card) {
        if (currentPlayer.CanPlayCard(card))
        {
            currentPlayer.PlayCard(card);
        }
        else
        {
            Debug.Log("No more actions permitted this turn");
        }
    }

    // Use this for initialization
    void Start() {
        SetGameState(GameState.SetupCards);
        passButton.onClick.AddListener(HandlePlayerPass);
    }

    void Update() {
        switch (gameState) {
            case GameState.SetupCards:
                SetupCards();
                break;
            case GameState.StartGame:
                StartGame();
                break;
            case GameState.ChangingPlayer:
                StartTurn(player1); // TODO: Make this swap players
                SetGameState(GameState.PlayerTurn);
                break;
        }
    }

    void SetGameState(GameState newState) {
        // TODO: Validate state changes
        gameState = newState;
    }

    bool CurrentlyInGameState(GameState state) {
        return gameState == state;
    }

    void SetupCards() {
        int counter = 2;
        var onReady = new System.EventHandler((object sender, System.EventArgs e) => {
            if (--counter == 0) {
                SetGameState(GameState.StartGame);
            }
        });

        cardStore.SetupCards();
        cardStore.PositionCards(onReady);

        cardStore.IssueStartingDeck(player1); 
        player1.PositionDeck(onReady);

        SetGameState(GameState.WaitingForCardMovement);
    }

    void StartGame() {
        player1.DrawCards(5);
        SetGameState(GameState.WaitingForCardMovement);
        player1.PositionHand(new System.EventHandler((object sender, System.EventArgs e) => {
            SetGameState(GameState.PlayerTurn);
            StartTurn(player1);
        }));
    }

    void StartTurn(Player player) {
        currentPlayer = player;
        currentPlayer.StartTurn();
    }

    public Player GetCurrentPlayer() {
        return currentPlayer;
    }

    void HandlePlayerPass() {
        if (!CurrentlyInGameState(GameState.PlayerTurn)) {
            Debug.Log("Not allowed to pass");
            return;
        }

        SetGameState(GameState.PlayerTurnEnded);
        currentPlayer.EndTurn();
        currentPlayer.DiscardAllCards();
        currentPlayer.PositionCards(new System.EventHandler((object sender, System.EventArgs e) => {
            currentPlayer.DrawCards(5);
            currentPlayer.PositionCards(new System.EventHandler((object sender2, System.EventArgs e2) => {
                SetGameState(GameState.ChangingPlayer);
            }));
        }));
    }
}