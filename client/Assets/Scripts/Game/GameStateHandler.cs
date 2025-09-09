using UnityEngine;

public class GameStateHandler : MonoBehaviour
{
    [SerializeField] private GameController gameController;
    [SerializeField] private UIController uiController;

    private void Start()
    {
        uiController.OpenPage<UIConnectionPage>();
    }
}
