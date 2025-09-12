using Nakama;
using Nakama.TinyJson;
using System;
using System.Collections;
using UnityEngine;

public class GameStateHandler : MonoBehaviour
{
    [SerializeField] private ServerController serverController;
    [SerializeField] private GameController gameController;
    [SerializeField] private UIController uiController;
    [SerializeField] private Pirate meleeUnit;
    [SerializeField] private Pirate rangedUnit;

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
        Debug.Log($"GameStateHandler - StartGame, sending ready signal to server...");
        serverController.Socket.SendMatchStateAsync(gameController.MatchId, (long)OpCode.ReadySignal, string.Empty);
    }

    public void EndGame()
    {
        serverController.OnMatchState -= OnMatchState;
    }

    public void RequestUnitSpawn(string unitType)
    {
        var request = new MatchSpawnUnitRequestModel() { unitType = unitType };
        Debug.Log($"GameStateHandler - RequestUnitSpawn, sending unit spawn request for {unitType}");
        serverController.Socket.SendMatchStateAsync(gameController.MatchId, (long)OpCode.SpawnUnit, JsonWriter.ToJson(request));
    }

    private void OnMatchState(IMatchState matchState)
    {
        Debug.Log($"OnMatchState - opCode: {matchState.OpCode}");
        string stateJson = System.Text.Encoding.UTF8.GetString(matchState.State);
        OpCode code = (OpCode)matchState.OpCode;
        if (code == OpCode.SpawnUnit)
        {
            var unitData = JsonParser.FromJson<MatchSpawnUnitResponseModel>(stateJson).unit;
            Pirate unit = unitData.type == "melee" ? meleeUnit : rangedUnit;
            float rotation = unitData.position < 0 ? 0 : 180f;
            var pirateUnit = Instantiate(unit, new Vector3(0, 0, unitData.position), Quaternion.Euler(0, rotation, 0), transform);
        }
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