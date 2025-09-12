using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMatchPage : UIPage
{
    [SerializeField] private UICardItem meleeCard;
    [SerializeField] private UICardItem rangedCard;
    [SerializeField] private Image manaProgress;
    [SerializeField] private TextMeshProUGUI manaCountText;

    public override void OnPageOpen()
    {
        meleeCard.InitCard(3, SpawnMelee);
        rangedCard.InitCard(2, SpawnRanged);
        manaProgress.fillAmount = 1;
        manaCountText.text = "10/10";
    }

    public override void OnPageClose()
    {

    }

    private void SpawnMelee()
    {
    }

    private void SpawnRanged()
    {
    }
}
