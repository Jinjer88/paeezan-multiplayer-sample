using System;

[Serializable]
public class GameConfig
{
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
    public int attack;
    public int cost;
    public int speed;
    public int range;
}