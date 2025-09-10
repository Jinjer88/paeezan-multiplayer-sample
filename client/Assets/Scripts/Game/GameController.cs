using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Game Controller", order = 51)]
public class GameController : ScriptableObject
{
    public string MatchId { get; set; }
    public string MatchCode { get; set; }
    public List<string> PlayerNames { get; set; }

    private void OnEnable()
    {
        MatchCode = string.Empty;
        MatchId = string.Empty;
        PlayerNames = new List<string>();
    }
}
