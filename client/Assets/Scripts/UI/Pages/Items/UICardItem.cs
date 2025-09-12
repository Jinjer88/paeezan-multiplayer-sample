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
}