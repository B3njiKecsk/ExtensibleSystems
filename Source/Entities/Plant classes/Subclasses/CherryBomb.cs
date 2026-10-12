using UnityEngine;

public class CherryBomb : Plant
{

    public CherryBomb(int row, int col) : base(row, col, stats)
    {
    }
    public static readonly PlantStats stats = new PlantStats
    (
        displayName: "Cherry Bomb",
        sunCost: 150,
        rechargeTime: 50f,
        maxHP: 300,
        attackDamage: 1800,
        timeBetweenTicks: 1.2f,
        tags: PlantTag.Instant
    );

    bool exploding = false;
    protected override void BehaviourPerTick()
    {
        if (exploding)
        {
            Suicide();
            return;
        }
            
        exploding = true;

        foreach (Zombie zombie in ZombiesInArea(row - 1, row + 1, col - 1, col + 1))
            DamageZombie(zombie);

        SetSprite(1);
    }
}
