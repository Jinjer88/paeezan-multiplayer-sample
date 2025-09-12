using Nakama;
using System;
using System.Collections;
using UnityEngine;

public class GameStateHandler : MonoBehaviour
{
    [SerializeField] private ServerController serverController;
    [SerializeField] private GameController gameController;
    [SerializeField] private UIController uiController;

    private GameState currentGameState;

    private void Start()
    {
        currentGameState = GameState.NotStarted;
        gameController.GameStateHandler = this;
        uiController.OpenPage<UIConnectionPage>();
    }

    private void Update()
    {
        switch (currentGameState)
        {
            case GameState.NotStarted:
                break;
            case GameState.Starting:
                break;
            case GameState.Battling:
                break;
            case GameState.Finished:
                break;
            default:
                break;
        }
    }

    public void StartGame()
    {
        serverController.OnMatchState += OnMatchState;
        StartCoroutine(StartGameCoroutine());
    }

    public void EndGame()
    {
        serverController.OnMatchState -= OnMatchState;
    }

    private void OnMatchState(IMatchState state)
    {

    }

    public void SwitchState(GameState newState)
    {
        OnStateExit();
        currentGameState = newState;
        OnStateEnter();
    }

    private IEnumerator StartGameCoroutine()
    {
        yield return new WaitForSeconds(5f);
        uiController.OpenPage<UIMatchPage>();
        SwitchState(GameState.Starting);
    }

    private void OnStateEnter()
    {
        switch (currentGameState)
        {
            case GameState.NotStarted:
                break;
            case GameState.Starting:
                break;
            case GameState.Battling:
                break;
            case GameState.Finished:
                break;
            default:
                break;
        }
    }

    private void OnStateExit()
    {
        switch (currentGameState)
        {
            case GameState.NotStarted:
                break;
            case GameState.Starting:
                break;
            case GameState.Battling:
                break;
            case GameState.Finished:
                break;
            default:
                break;
        }
    }
}

public enum GameState
{
    NotStarted, Starting, Battling, Finished
}