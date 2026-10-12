using System.Linq;
using UnityEngine;

public class Snowpea : Plant
{
    public Snowpea(int row, int col) : base(row, col, stats)
    {
    }

    public static readonly PlantStats stats = new PlantStats
    (
        displayName: "Snow Pea",
        sunCost: 175,
        rechargeTime: 7.5f,
        maxHP: 300,
        attackDamage: 20,
        timeBetweenTicks: 1.425f
    );

    protected override void BehaviourPerTick()
    {
        if (AnyZombiesInFront())
        {
            ShootProjectile(ProjectileType.FrozenPea, ActorFace.PlantShooterMouth);
        }
    }
}
