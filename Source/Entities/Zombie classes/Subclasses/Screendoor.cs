using UnityEngine;

public class Screendoor : Zombie, IMagnetizeable
{
    const int screendoorShieldMaxHP = 1100;
    Armor screendoorShield;

    public Screendoor(int row) : base(row, stats)
    {
        screendoorShield = new Armor(
            hp: screendoorShieldMaxHP, 
            protectsFromDirection: DamageInfo.Direction.left);

        GiveEquipment(screendoorShield);
    }

    public static readonly ZombieStats stats = new ZombieStats(
        displayName: "Screen Door Zombie",
        maxHP: 200,
        wavePointCost: 4,
        waveWeight: 3500
        );

    
    float damageTaken => screendoorShield.MaxHP - screendoorShield.HP;

    protected override int CalculateHPState()
    {
        int shieldState = (int)(damageTaken / (screendoorShield.MaxHP / 3));

        int zombieState = HP > (stats.MaxHP / 2) ? 0 : 1;

        if(screendoorShield.HP > 0)
            return shieldState;
        return shieldState + zombieState;
    }

    public Equipment Magnetize(Entity source)
    {
        RemoveEquipment(screendoorShield);
        return screendoorShield;
    }
}