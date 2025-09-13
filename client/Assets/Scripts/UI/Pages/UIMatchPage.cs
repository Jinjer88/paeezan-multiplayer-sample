using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMatchPage : UIPage
{
    [SerializeField] private GameController gameController;
    [SerializeField] private UICardItem meleeCard;
    [SerializeField] private UICardItem rangedCard;
    [SerializeField] private Image manaProgress;
    [SerializeField] private TextMeshProUGUI manaCountText;

    private void Update()
    {
        if (gameController.GameConfig == null)
            return;

        manaProgress.fillAmount += gameController.GameConfig.manaRegenRate / 10f * Time.deltaTime;
        manaCountText.text = $"{Mathf.FloorToInt(manaProgress.fillAmount * 10f)}/10";
    }

    public override void OnPageOpen()
    {
        meleeCard.InitCard(gameController.GameConfig.units.melee.cost, SpawnMelee);
        rangedCard.InitCard(gameController.GameConfig.units.ranged.cost, SpawnRanged);
        manaProgress.fillAmount = 1;
        manaCountText.text = "10/10";

        gameController.OnUnitSpawned += OnUnitSpawned;
    }

    public override void OnPageClose()
    {
        gameController.OnUnitSpawned -= OnUnitSpawned;
    }

    private void OnUnitSpawned(Unit unit, bool isMine)
    {
        if(unit == null || !isMine)
            return;

        int manaCost = unit.type == "melee" ? gameController.GameConfig.units.melee.cost : gameController.GameConfig.units.ranged.cost;
        manaProgress.fillAmount -= (manaCost / 10f);
    }

    private void SpawnMelee()
    {
        int unitCost = gameController.GameConfig.units.melee.cost;
        if (manaProgress.fillAmount * 10f < unitCost)
            return;

        gameController.SpawnUnit("melee");
        float cooldown = gameController.GameConfig.units.melee.cooldown;
        meleeCard.StartCooldown(cooldown);
    }

    private void SpawnRanged()
    {
        int unitCost = gameController.GameConfig.units.ranged.cost;
        if (manaProgress.fillAmount * 10f < unitCost)
            return;

        gameController.SpawnUnit("ranged");
        float cooldown = gameController.GameConfig.units.ranged.cooldown;
        rangedCard.StartCooldown(cooldown);
    }
}
