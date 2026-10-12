using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Puffshroom : Plant
{
    public Puffshroom(int row, int col) : base(row, col, stats)
    {
    }

    public static readonly PlantStats stats = new PlantStats
    (
        displayName: "Puff-shroom",
        sunCost: 0,
        rechargeTime: 7.5f,
        maxHP: 300,
        attackDamage: 20,
        timeBetweenTicks: 1.425f,
        tags: PlantTag.Mushroom,
        range: 3
    );
    List<Zombie> targets = new();
    protected override void BehaviourPerTick()
    {
        if (ZombiesInRange().Count() > 0)
            ShootProjectile(ProjectileType.Puff, ActorFace.PlantShooterLowMouth);
    }
    
}
