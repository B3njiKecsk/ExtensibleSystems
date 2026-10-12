using UnityEngine;

public class FootballZombie : Zombie, IMagnetizeable
{
    const int helmetMaxHP = 1400;
    Armor helmet;
    public FootballZombie(int row) : base(row, stats)
    {
        helmet = new Armor(helmetMaxHP);

        GiveEquipment(helmet);
    }

    public static readonly ZombieStats stats = new ZombieStats(
        displayName: "Football Zombie",
        maxHP: 200,
        speed: 1 / 2.5f,
        wavePointCost: 7,
        waveWeight: 2000
        );

    public Equipment Magnetize(Entity source)
    {
        RemoveEquipment(helmet);
        return helmet;
    }

    float damageTaken => helmet.MaxHP - helmet.HP;

    protected override int CalculateHPState()
    {
        Debug.Log("Took a Pea of damage");
        int shieldState = (int)(damageTaken / (helmet.MaxHP / 3));

        int zombieState = HP > (stats.MaxHP / 2) ? 0 : 1;

        if (helmet.HP > 0)
            return shieldState;
        return shieldState + zombieState;
    }
}
