using System.Linq;
using UnityEngine;

public class Repeater : Plant
{
    public Repeater(int row, int col) : base(row, col, stats)
    {
    }

    public static readonly PlantStats stats = new PlantStats
    (
        displayName: "Repeater",
        sunCost: 200,
        rechargeTime: 7.5f,
        maxHP: 300,
        attackDamage: 20,
        timeBetweenTicks: 1.425f
    );

    bool firing = false;
    protected override void BehaviourPerTick()
    {
        if (firing)
        {
            firing = false;
            ShootProjectile(ProjectileType.Pea, ActorFace.PlantShooterMouth);
        }
        else if (AnyZombiesInFront())
        {
            ShootProjectile(ProjectileType.Pea, ActorFace.PlantShooterMouth);
            SetTimeBeforeNextTick(0.2f);
            firing = true;
        }
    }
}
