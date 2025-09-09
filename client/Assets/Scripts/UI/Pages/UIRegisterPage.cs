using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRegisterPage : UIPage
{
    [SerializeField] private ServerController serverController;
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private Button registerButton;
    [SerializeField] private TextMeshProUGUI errorText;

    public override void OnPageOpen()
    {
        registerButton.onClick.AddListener(OnRegisterButtonClicked);
        registerButton.interactable = true;
        errorText.text = "";
    }

    public override void OnPageClose()
    {
        registerButton.onClick.RemoveListener(OnRegisterButtonClicked);
    }

    private void OnRegisterButtonClicked()
    {
        errorText.text = "";

        if (nicknameInput.text.Length < 2)
        {
            errorText.text = "Nickname is too short, must be at least 2 characters long";
            return;
        }

        if (nicknameInput.text.Length > 16)
        {
            errorText.text = "Nickname is too long, max 16 characters long";
            return;
        }

        string nickname = nicknameInput.text;
        Authenticate(nickname);
    }

    private async void Authenticate(string nickname)
    {
        registerButton.interactable = false;
        bool success = await serverController.Authenticate(nickname);
        if (success)
        {
            uiController.OpenPage<UIMainMenuPage>();
        }
        else
        {
            registerButton.interactable = true;
        }
    }
}