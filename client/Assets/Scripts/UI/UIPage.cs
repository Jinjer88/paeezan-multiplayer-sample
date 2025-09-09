using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas), typeof(GraphicRaycaster))]
public abstract class UIPage : MonoBehaviour
{
    [SerializeField] protected UIController uiController;

    private Canvas canvas;
    private GraphicRaycaster raycaster;

    public abstract void OnPageOpen();
    public abstract void OnPageClose();

    protected virtual void Awake()
    {
        canvas = GetComponent<Canvas>();
        raycaster = GetComponent<GraphicRaycaster>();
        uiController.AddPage(this);
    }

    public void SetPageActive(bool active)
    {
        canvas.enabled = active;
        raycaster.enabled = active;

        if (active)
            OnPageOpen();
        else
            OnPageClose();
    }
}
