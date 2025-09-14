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
        errorText.text = string.Empty;
        nicknameInput.text = string.Empty;
    }

    public override void OnPageClose()
    {
        registerButton.onClick.RemoveListener(OnRegisterButtonClicked);
    }

    private void OnRegisterButtonClicked()
    {
        errorText.text = "";

        if (nicknameInput.text.Length < 3)
        {
            errorText.text = "Nickname is too short, must be at least 3 characters long";
            return;
        }

        if (nicknameInput.text.Length > 16)
        {
            errorText.text = "Nickname is too long, max 16 characters long";
            return;
        }

        string nickname = nicknameInput.text;
        registerButton.interactable = false;
        PlayerPrefs.SetString("username", nickname);
        uiController.OpenPage<UIConnectionPage>();
    }
}