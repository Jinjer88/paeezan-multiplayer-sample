using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Game Controller", order = 51)]
public class GameController : ScriptableObject
{
    public string MatchId { get; set; }
    public string MatchCode { get; set; }
    public List<IdUsernamePair> IdUsernamePairs { get; set; }
    public GameStateHandler GameStateHandler { get; set; }
    public GameConfig GameConfig { get; set; }
    public bool IsHost { get; set; }

    public Action<int, bool> OnTowerAttack;
    public Action<Unit, bool> OnUnitSpawned;
    public Action<string> OnGameOver;

    private void OnEnable()
    {
        MatchCode = string.Empty;
        MatchId = string.Empty;
        IdUsernamePairs = new List<IdUsernamePair>();
        IsHost = false;
    }

    private void OnDisable()
    {
        MatchCode = string.Empty;
        MatchId = string.Empty;
        IdUsernamePairs = new List<IdUsernamePair>();
        IsHost = false;
    }
}

public struct IdUsernamePair
{
    public string id;
    public string username;
}