using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIConnectionPage : UIPage
{
    [SerializeField] private ServerController serverController;
    [SerializeField] private Image loadingSpinner;
    [SerializeField] private Transform loadingContainer;
    [SerializeField] private Transform connectionErrorContainer;

    public override void OnPageOpen()
    {
        SetLoadingSpinnerActive(true);

        if (PlayerPrefs.HasKey("username"))
        {
            Authenticate();
        }
        else
        {
            uiController.OpenPage<UIRegisterPage>();
        }
    }

    public override void OnPageClose()
    {
        SetLoadingSpinnerActive(false);
    }

    private async void Authenticate()
    {
        string username = PlayerPrefs.GetString("username");
        bool success = await serverController.Authenticate(username);
        if (success)
        {
            uiController.OpenPage<UIMainMenuPage>();
        }
        else
        {
            uiController.OpenPage<UIRegisterPage>();
        }
    }

    private void SetLoadingSpinnerActive(bool active)
    {
        if (loadingSpinner != null)
        {
            loadingSpinner.transform.DOKill();

            if (active)
                loadingSpinner.transform.DORotate(new Vector3(0, 0, 360), 1f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
        }
    }
}