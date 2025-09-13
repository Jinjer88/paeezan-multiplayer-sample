using DG.Tweening;
using Nakama;
using Nakama.TinyJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateHandler : MonoBehaviour
{
    [SerializeField] private ServerController serverController;
    [SerializeField] private GameController gameController;
    [SerializeField] private UIController uiController;
    [SerializeField] private Pirate meleeUnit;
    [SerializeField] private Pirate rangedUnit;
    [SerializeField] private Material blueMat;
    [SerializeField] private Material redMat;

    private GameState currentGameState;
    private Dictionary<int, Pirate> units = new Dictionary<int, Pirate>();

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

    private void OnEnable()
    {
        serverController.OnMatchState += OnMatchState;
    }

    private void OnDisable()
    {
        serverController.OnMatchState -= OnMatchState;
    }

    public void StartGame()
    {
        units = new Dictionary<int, Pirate>();
        StartCoroutine(StartGameCoroutine());
        Debug.Log($"GameStateHandler - StartGame, sending ready signal to server...");
        serverController.Socket.SendMatchStateAsync(gameController.MatchId, (long)OpCode.ReadySignal, string.Empty);
    }

    public void EndGame()
    {
    }

    public void RequestUnitSpawn(string unitType)
    {
        var request = new MatchSpawnUnitRequestModel() { unitType = unitType };
        Debug.Log($"GameStateHandler - RequestUnitSpawn, sending unit spawn request for {unitType}");
        serverController.Socket.SendMatchStateAsync(gameController.MatchId, (long)OpCode.SpawnUnitRequest, JsonWriter.ToJson(request));
    }

    private void OnMatchState(IMatchState matchState)
    {
        Debug.Log($"OnMatchState - opCode: {matchState.OpCode}");
        string stateJson = System.Text.Encoding.UTF8.GetString(matchState.State);
        Debug.Log($"OnMatchState - state json: {stateJson}");
        OpCode code = (OpCode)matchState.OpCode;
        if (code == OpCode.SpawnUnit)
        {
            var unitData = JsonParser.FromJson<MatchSpawnUnitResponseModel>(stateJson).unit;
            bool isMine = unitData.owner == serverController.Session.UserId;
            Pirate unitPrefab = unitData.type == "melee" ? meleeUnit : rangedUnit;
            float rotation = isMine ? 0 : 180f;
            var mat = isMine ? blueMat : redMat;
            float position = isMine ? -5f : 5f;
            var pirateUnit = Instantiate(unitPrefab, new Vector3(0, 0, position), Quaternion.Euler(0, rotation, 0), transform);
            pirateUnit.InitUnit(isMine, unitData.health, mat);
            units.Add(unitData.id, pirateUnit);
            gameController.OnUnitSpawned?.Invoke(unitData, isMine);
        }
        else if (code == OpCode.UnitPositionUpdates)
        {
            var positionUpdate = JsonParser.FromJson<MatchUnitUpdateResponseModel>(stateJson).updates;
            for (int i = 0; i < positionUpdate.Length; i++)
            {
                if (units.TryGetValue(positionUpdate[i].id, out Pirate pirate))
                {
                    int modifier = gameController.IsHost ? 1 : -1;
                    float targetPos = positionUpdate[i].position * modifier;
                    //pirate.transform.position = new Vector3(0, 0, targetPos);
                    pirate.transform.DOKill();
                    pirate.transform.DOMoveZ(targetPos, 0.2f).SetEase(Ease.Linear);
                    pirate.SwitchState(pirate.movingState);
                }
            }
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