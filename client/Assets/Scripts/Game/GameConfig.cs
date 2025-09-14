using System;

[Serializable]
public class GameConfig
{
    public float manaRegenRate;
    public TowersConfig towers;
    public UnitsConfig units;
}

[Serializable]
public class TowersConfig
{
    public int health;
    public int attack;
}

[Serializable]
public class UnitsConfig
{
    public UnitConfig melee;
    public UnitConfig ranged;
}

[Serializable]
public class UnitConfig
{
    public int health;
    public int attackDamage;
    public float attackSpeed;
    public float attackRange;
    public int cost;
    public float moveSpeed;
    public float cooldown;
}