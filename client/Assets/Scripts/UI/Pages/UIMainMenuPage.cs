using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMainMenuPage : UIPage
{
    [SerializeField] private GameController gameController;
    [SerializeField] private ServerController serverController;
    [SerializeField] private Button createMatch;
    [SerializeField] private Button joinMatch;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Button refreshMatchList;
    [SerializeField] private UIMatchItem matchItemPrefab;
    [SerializeField] private Transform matchListParent;

    public override void OnPageOpen()
    {
        playerNameText.text = serverController.Account.User.Username;
        refreshMatchList.onClick.AddListener(UpdateMatchList);
        createMatch.onClick.AddListener(CreateMatch);
        UpdateMatchList();
    }

    public override void OnPageClose()
    {
        refreshMatchList.onClick.RemoveListener(UpdateMatchList);
        createMatch.onClick.RemoveListener(CreateMatch);
    }

    private async void UpdateMatchList()
    {
        await Task.Delay(1000);
        var matches = await serverController.GetMatchesList();

        foreach (Transform child in matchListParent)
            Destroy(child.gameObject);

        foreach (var match in matches)
        {
            var matchItem = Instantiate(matchItemPrefab, matchListParent);
            matchItem.SetData($"{match.MatchId} - ({match.Size}/2)", async () =>
            {
                if (match.Size > 1)
                {
                    Debug.Log($"Failed to join match {match.MatchId}, match is full, size: {match.Size}");
                    return;
                }
                var result = await serverController.JoinMatchWithCode(match.MatchId);
                if (result == null)
                {
                    Debug.Log($"Failed to join match {match.MatchId}, wrong code");
                    return;
                }
                UpdateMatchList();
            });
        }
    }

    private async void CreateMatch()
    {
        var match = await serverController.CreateMatch();
        if (match != null)
        {
            gameController.MatchCode = match.code;
            gameController.MatchId = match.matchId;
            await serverController.JoinMatchWithId(match.matchId);
            uiController.OpenPage<UILobbyPage>();
        }
    }
}
