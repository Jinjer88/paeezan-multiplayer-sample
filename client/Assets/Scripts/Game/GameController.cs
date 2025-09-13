using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Game Controller", order = 51)]
public class GameController : ScriptableObject
{
    public string MatchId { get; set; }
    public string MatchCode { get; set; }
    public List<string> PlayerNames { get; set; }
    public GameStateHandler GameStateHandler { get; set; }
    public GameConfig GameConfig { get; set; }
    public bool IsHost { get; set; }

    public Action<Unit, bool> OnUnitSpawned;

    public void StartGame()
    {
        GameStateHandler.StartGame();
    }

    public void SpawnUnit(string unitType)
    {
        GameStateHandler.RequestUnitSpawn(unitType);
    }

    private void OnEnable()
    {
        MatchCode = string.Empty;
        MatchId = string.Empty;
        PlayerNames = new List<string>();
        IsHost = false;
    }
}
