using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILobbyPage : UIPage
{
    [SerializeField] private ServerController serverController;
    [SerializeField] private GameController gameController;
    [SerializeField] private Button goBackButton;
    [SerializeField] private TextMeshProUGUI matchCodeText;
    [SerializeField] private TextMeshProUGUI p1NameText;
    [SerializeField] private TextMeshProUGUI p2NameText;

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
    }
}
