using Nakama;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIMainMenuPage : UIPage
{
    [SerializeField] private GameController gameController;
    [SerializeField] private ServerController serverController;
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private Button createMatch;
    [SerializeField] private Button joinMatch;
    [SerializeField] private TMP_InputField matchCodeInputField;
    [SerializeField] private TextMeshProUGUI errorMessageText;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerWinsText;
    [SerializeField] private Button refreshMatchList;
    [SerializeField] private UIMatchItem matchItemPrefab;
    [SerializeField] private Transform matchListParent;
    [SerializeField] private Button newUserButton;

    public override void OnPageOpen()
    {
        foreach (Transform child in matchListParent)
            Destroy(child.gameObject);

        gameController.IsHost = false;
        errorMessageText.text = string.Empty;
        matchCodeInputField.text = string.Empty;
        playerNameText.text = serverController.Account.User.Username;
        refreshMatchList.onClick.AddListener(UpdateMatchList);
        createMatch.onClick.AddListener(CreateMatch);
        joinMatch.onClick.AddListener(CheckMatchCode);
        leaderboardButton.onClick.AddListener(ShowLeaderboard);
        newUserButton.onClick.AddListener(LogOut);
        UpdateMatchList();
        UpdateLeaderboard();
    }

    public override void OnPageClose()
    {
        refreshMatchList.onClick.RemoveListener(UpdateMatchList);
        createMatch.onClick.RemoveListener(CreateMatch);
        joinMatch.onClick.RemoveListener(CheckMatchCode);
        leaderboardButton.onClick.RemoveListener(ShowLeaderboard);
        newUserButton.onClick.RemoveListener(LogOut);
    }

    private async void LogOut()
    {
        PlayerPrefs.DeleteKey("username");
        uiController.OpenPage<UIConnectionPage>();
        await serverController.CloseSocket();
        await serverController.LogOut();
    }

    private void ShowLeaderboard()
    {
        uiController.OpenPage<UILeaderboardPage>();
    }

    private async void UpdateLeaderboard()
    {
        var myRank = await serverController.GetMyLeaderboardRecord();
        if (myRank != null)
        {
            playerWinsText.text = $"Total Wins: {myRank.Score}";
        }
        else
        {
            playerWinsText.text = $"Total Wins: 0";
        }

        _ = serverController.GetLeaderboardRecords();
    }

    private async void UpdateMatchList()
    {
        errorMessageText.text = string.Empty;
        await Task.Delay(1000);
        var matches = await serverController.GetMatchesList();

        foreach (Transform child in matchListParent)
            Destroy(child.gameObject);

        foreach (var match in matches)
        {
            var matchItem = Instantiate(matchItemPrefab, matchListParent);
            matchItem.SetData($"{match.MatchId} - ({match.Size}/2) Players in match", () =>
            {
                JoinMatch(match);
            });
        }
    }

    private async void CreateMatch()
    {
        errorMessageText.text = string.Empty;
        joinMatch.interactable = false;
        createMatch.interactable = false;

        var match = await serverController.CreateMatch();

        if (match == null)
        {
            errorMessageText.text = $"Failed to create match";
            joinMatch.interactable = true;
            createMatch.interactable = true;
            return;
        }

        gameController.MatchCode = match.code;
        gameController.MatchId = match.matchId;
        await serverController.JoinMatchWithId(match.matchId);

        joinMatch.interactable = true;
        createMatch.interactable = true;
        gameController.IsHost = true;
        uiController.OpenPage<UILobbyPage>();
    }

    private async void JoinMatch(IApiMatch match)
    {
        if (match.Size > 1)
        {
            Debug.Log($"Failed to join match {match.MatchId}, match is full, size: {match.Size}");
            errorMessageText.text = $"This match is full";
            joinMatch.interactable = true;
            createMatch.interactable = true;
            return;
        }
        var result = await serverController.JoinMatchWithId(match.MatchId);
        if (result == null)
        {
            Debug.Log($"Failed to join match {match.MatchId}, wrong code");
            errorMessageText.text = $"Failed to join match";
            joinMatch.interactable = true;
            createMatch.interactable = true;
            return;
        }

        gameController.MatchId = match.MatchId;
        uiController.OpenPage<UILobbyPage>();
    }

    private async void CheckMatchCode()
    {
        errorMessageText.text = string.Empty;
        joinMatch.interactable = false;
        createMatch.interactable = false;
        string matchCode = matchCodeInputField.text.ToUpper();

        if (matchCode.Length != 4)
        {
            errorMessageText.text = $"Code must be 4 characters";
            joinMatch.interactable = true;
            createMatch.interactable = true;
            return;
        }

        var matchId = await serverController.GetMatchIDWithCode(matchCode);
        gameController.MatchId = matchId;
        gameController.MatchCode = matchCode;
        var match = await serverController.JoinMatchWithId(matchId);

        joinMatch.interactable = true;
        createMatch.interactable = true;
        if (match == null)
        {
            errorMessageText.text = $"Failed to join match, make sure the match code is correct";
            return;
        }

        uiController.OpenPage<UILobbyPage>();
    }
}
