using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICardItem : MonoBehaviour
{
    [SerializeField] private Button useButton;
    [SerializeField] private TextMeshProUGUI manaCostText;
    [SerializeField] private Image cooldownProgressImage;

    public void InitCard(int manaCost, UnityAction onButtonPress)
    {
        useButton.onClick.RemoveAllListeners();
        useButton.onClick.AddListener(onButtonPress);
        manaCostText.text = manaCost.ToString();
        cooldownProgressImage.fillAmount = 0;
    }

    public void StartCooldown(float cooldownTime)
    {
        useButton.interactable = false;
        cooldownProgressImage.fillAmount = 1;
        cooldownProgressImage.DOFillAmount(0, cooldownTime).SetEase(Ease.Linear).OnComplete(() =>
        {
            useButton.interactable = true;
        });
    }
}