using TMPro;
using UnityEngine;

public class UILeaderboardRecordItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerUsername;
    [SerializeField] private TextMeshProUGUI playerScore;
    [SerializeField] private TextMeshProUGUI playerRank;

    public void SetData(string rank, string username, string score)
    {
        playerUsername.text = username;
        playerScore.text = score;
        playerRank.text = rank;
    }
}