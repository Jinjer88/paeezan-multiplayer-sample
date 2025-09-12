using Nakama;
using Nakama.TinyJson;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILobbyPage : UIPage
{
    [SerializeField] private ServerController serverController;
    [SerializeField] private GameController gameController;
    [SerializeField] private Button goBackButton;
    [SerializeField] private TextMeshProUGUI matchCodeText;
    [SerializeField] private TextMeshProUGUI[] playerNameTexts;
    [SerializeField] private Transform vsTransform;
    [SerializeField] private Transform waitingTransform;

    private void Start()
    {
        vsTransform.gameObject.SetActive(false);
        waitingTransform.gameObject.SetActive(false);
        for (int i = 0; i < playerNameTexts.Length; i++)
            playerNameTexts[i].text = string.Empty;
    }

    private void OnEnable()
    {
        serverController.OnPlayerJoined += OnPlayerJoined;
        serverController.OnMatchState += OnMatchState;
    }

    private void OnDisable()
    {
        serverController.OnPlayerJoined -= OnPlayerJoined;
        serverController.OnMatchState -= OnMatchState;
    }

    public override void OnPageOpen()
    {
        goBackButton.onClick.AddListener(GoBack);
        matchCodeText.text = $"Match Code: {gameController.MatchCode}";
    }

    public override void OnPageClose()
    {
        goBackButton.onClick.RemoveListener(GoBack);
        matchCodeText.text = string.Empty;
    }

    private async void GoBack()
    {
        uiController.OpenPage<UIMainMenuPage>();
        await serverController.LeaveMatch(gameController.MatchId);
        vsTransform.gameObject.SetActive(false);
        waitingTransform.gameObject.SetActive(false);
        for (int i = 0; i < playerNameTexts.Length; i++)
        {
            playerNameTexts[i].text = string.Empty;
        }
    }

    private void OnPlayerJoined(IMatchPresenceEvent presenceEvent)
    {
        var joinsArray = presenceEvent.Joins.ToArray();
        Debug.Log($"UILobbyPage - OnPlayerJoined, {joinsArray.Length}x player joined");
    }

    private void OnMatchState(IMatchState matchState)
    {
        string stateJson = System.Text.Encoding.UTF8.GetString(matchState.State);
        Debug.Log($"OnMatchState - code: {matchState.OpCode}, state: {stateJson}");
        OpCode code = (OpCode)matchState.OpCode;
        switch (code)
        {
            case OpCode.None:
                break;
            case OpCode.MatchReady:
                break;
            case OpCode.SignalReady:
                break;
            case OpCode.StartMatch:
                break;
            case OpCode.LobbyUpdate:
                var lobbyUpdate = JsonParser.FromJson<MatchLobbyUpdateMessageModel>(stateJson);
                gameController.PlayerNames.Clear();
                for (int i = 0; i < lobbyUpdate.players.Length; i++)
                {
                    gameController.PlayerNames.Add(lobbyUpdate.players[i]);
                    if (playerNameTexts.Length > i)
                        playerNameTexts[i].text = lobbyUpdate.players[i];
                }
                if (lobbyUpdate.players.Length == 1)
                {
                    vsTransform.gameObject.SetActive(false);
                    waitingTransform.gameObject.SetActive(true);
                }
                else if (lobbyUpdate.players.Length == 2)
                {
                    vsTransform.gameObject.SetActive(true);
                    waitingTransform.gameObject.SetActive(false);
                    StartGame();
                }
                break;
            default:
                break;
        }
    }

    private void StartGame()
    {
        matchCodeText.text = $"Battle starting soon...";
        goBackButton.interactable = false;
        gameController.StartGame();
    }
}