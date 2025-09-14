using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMatchPage : UIPage
{
    [SerializeField] private ServerController serverController;
    [SerializeField] private GameController gameController;
    [SerializeField] private UICardItem meleeCard;
    [SerializeField] private UICardItem rangedCard;
    [SerializeField] private Image manaProgress;
    [SerializeField] private TextMeshProUGUI manaCountText;
    [SerializeField] private Slider myTowerHealthSlider;
    [SerializeField] private TextMeshProUGUI myTowerHealthText;
    [SerializeField] private Slider opponentTowerHealthSlider;
    [SerializeField] private TextMeshProUGUI opponentTowerHealthText;

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
        foreach (var tower in gameController.GameStateHandler.Towers)
        {
            if (tower.Key == serverController.Session.UserId)
            {
                myTowerHealthSlider.value = 1f;
                myTowerHealthText.text = tower.Value.ToString();
            }
            else
            {
                opponentTowerHealthSlider.value = 1f;
                opponentTowerHealthText.text = tower.Value.ToString();
            }
        }

        gameController.OnUnitSpawned += OnUnitSpawned;
        gameController.OnTowerAttack += OnTowerAttack;
    }

    public override void OnPageClose()
    {
        gameController.OnUnitSpawned -= OnUnitSpawned;
        gameController.OnTowerAttack -= OnTowerAttack;
    }

    private void OnTowerAttack(int health, bool isMine)
    {
        int towerMaxHealth = gameController.GameConfig.towers.health;
        Debug.Log($"UIMatchPage - OnTowerAttack, health: {health}, max health: {towerMaxHealth}, value: {health / towerMaxHealth}");
        if (isMine)
        {
            if (health <= 0)
            {
                myTowerHealthSlider.value = 0;
                myTowerHealthText.text = "0";
            }
            else
            {
                myTowerHealthSlider.value = (float)health / towerMaxHealth;
                myTowerHealthText.text = health.ToString();
            }
        }
        else
        {
            if (health <= 0)
            {
                opponentTowerHealthSlider.value = 0;
                opponentTowerHealthText.text = "0";
            }
            else
            {
                opponentTowerHealthSlider.value = (float)health / towerMaxHealth;
                opponentTowerHealthText.text = health.ToString();
            }
        }
    }

    private void OnUnitSpawned(Unit unit, bool isMine)
    {
        if (unit == null || !isMine)
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
