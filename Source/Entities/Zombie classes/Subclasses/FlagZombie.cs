using UnityEngine;

public class FlagZombie : Zombie
{
    public FlagZombie(int row) : base(row, stats)
    {
    }

    public static readonly ZombieStats stats = new ZombieStats(
        displayName: "Flag zombie",
        maxHP: 200, //wiki says 270 fsr, but in game it takes 10 peas (which do 20 damage) to kill a browncoat
        dps: 100,
        speed: 1 / 3.7f, //wiki displays 4.7 as seconds per tile, while my speed is measured in tiles per second
        wavePointCost: 1
        );
}
