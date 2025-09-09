using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMainMenuPage : UIPage
{
    [SerializeField] private ServerController serverController;
    [SerializeField] private Button createMatch;
    [SerializeField] private Button joinMatch;
    [SerializeField] private TextMeshProUGUI playerNameText;

    public override void OnPageOpen()
    {
        playerNameText.text = serverController.Account.User.Username;
    }

    public override void OnPageClose()
    {
    }
}
