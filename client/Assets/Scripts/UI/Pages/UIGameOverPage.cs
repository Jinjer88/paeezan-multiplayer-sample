using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGameOverPage : UIPage
{
    [SerializeField] private ServerController serverController;
    [SerializeField] private GameController gameController;
    [SerializeField] private TextMeshProUGUI winnerNameText;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Button leaveButton;

    private void OnEnable()
    {
        gameController.OnGameOver += OnGameOver;
    }

    private void OnDisable()
    {
        gameController.OnGameOver -= OnGameOver;
    }

    public override void OnPageOpen()
    {
        leaveButton.onClick.AddListener(LeaveMatch);
    }

    public override void OnPageClose()
    {
        leaveButton.onClick.RemoveListener(LeaveMatch);
    }

    private void LeaveMatch()
    {
        uiController.OpenPage<UIMainMenuPage>();
        _ = serverController.LeaveMatch(gameController.MatchId);
        gameController.GameStateHandler.ClearBattleScene();
    }

    private void OnGameOver(string winnerID)
    {
        var winner = gameController.IdUsernamePairs.FirstOrDefault(p => p.id == winnerID);
        winnerNameText.text = $"{winner.username} wins";

        bool isWinner = serverController.Session.UserId == winnerID;
        resultText.color = isWinner ? Color.green : Color.red;
        resultText.text = isWinner ? "VICTORY!" : "DEFEAT";
    }
}
