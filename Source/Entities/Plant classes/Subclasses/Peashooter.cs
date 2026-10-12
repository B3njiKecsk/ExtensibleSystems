using System.Linq;
using UnityEngine;

public sealed class Peashooter : Plant
{
    public Peashooter(int row, int col) : base(row, col, stats)
    {
    }

    public static readonly PlantStats stats = new PlantStats
    (
        displayName: "Peashooter",
        sunCost: 100,
        rechargeTime: Plant.Recharge.Fast,
        maxHP: Plant.Toughness.Normal,
        attackDamage: 20,
        timeBetweenTicks: 0.25f//1.425f
    );


    protected override void BehaviourPerTick()
    {
        if (AnyZombiesInFront())
            ShootProjectile(ProjectileType.Pea, ActorFace.PlantShooterMouth);
    }
}
