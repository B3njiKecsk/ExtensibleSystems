using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Scaredyshroom : Mushroom
{
    public Scaredyshroom(int row, int col) : base(row, col, stats)
    {
    }

    public static readonly PlantStats stats = new PlantStats(
        displayName: "Scaredy-shroom",
        sunCost: 25,
        rechargeTime: Plant.Recharge.Fast,
        maxHP: Plant.Toughness.Normal,
        attackDamage: 20,
        timeBetweenTicks: 1.425f,
        tags: PlantTag.Mushroom
        );

    bool hiding;

    protected override void BehaviourPerTick()
    {
        if (hiding)
            return;

        if (AnyZombiesInFront())
            ShootProjectile(ProjectileType.LongPuff, ActorFace.PlantShooterMidMouth);
    }

    protected override void BehaviourOnStart()
    {
        if (ZombiesInArea(row - 1, row + 1, col - 1, col + 1).Count() > 0)
            hiding = true;
    }

    protected override IEnumerable<(int row, int col)> GetNotifiedOnTiles()
    {
        yield return (row-1, col-1);
        yield return (row,   col-1);
        yield return (row+1, col-1);

        yield return (row-1, col);
        yield return (row,   col);
        yield return (row+1, col);

        yield return (row-1, col+1);
        yield return (row,   col+1);
        yield return (row+1, col+1);
    }

    protected override void OnZombieEnterTile(int row, int col)
    {
        if (ZombiesInTiles <= 0)
            SetHiding(true);

        base.OnZombieEnterTile(row, col);
    }

    protected override void OnZombieLeftTile(int row, int col)
    {
        base.OnZombieLeftTile(row, col);

        if (ZombiesInTiles <= 0)
            SetHiding(false);
    }

    void SetHiding(bool value)
    {
        hiding = value;
        SetSprite(value? 2 : 0);
    }
}
