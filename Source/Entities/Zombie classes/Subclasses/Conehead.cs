using System;

public sealed class Conehead : Zombie
{
    const int coneMaxHP = 375;
    Armor cone;
    public Conehead(int row) : base(row, stats)
    {
        cone = new Armor(coneMaxHP);
        GiveEquipment(cone);
    }

    //public override ZombieTag Tags => ZombieTag.Instant | ZombieTag.Secondary;
    public static readonly ZombieStats stats = new ZombieStats
    (
        displayName: "Conehead",
        maxHP: 200,
        dps: 100,
        speed: 1 / 4.7f,
        wavePointCost: 2
    );

    protected override int CalculateHPState()
    {
        int coneState = (int)((cone.MaxHP - Math.Max(cone.HP, 0)) / (cone.MaxHP / 3));

        int zombieState = HP > (stats.MaxHP / 2) ? 0 : 1;

        if (cone.HP > 0)
            return coneState;

        return coneState + zombieState;
    }
}
