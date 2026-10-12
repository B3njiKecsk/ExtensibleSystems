using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PotatoMine : Plant
{
    public PotatoMine(int row, int col) : base(row, col, stats)
    {
    }

    public static readonly PlantStats stats = new PlantStats
    (
        displayName: "Potato Mine",
        sunCost: 25,
        rechargeTime: 30f,
        attackDamage: 1800,
        timeBetweenTicks: 14f,
        maxHP: 300
    );

    bool armed = false;

    public override bool CanBeEaten(Zombie origin)
    {
        return !armed;
    }

    protected override void BehaviourPerTick()
    {
        if(!armed)
        {
            SetSprite(1);
            armed = true;
            Explode();
        }
    }
    private void Explode()
    {
        IEnumerable<Zombie> zombies = ZombiesOnTile(row, col);
        if (zombies.Count() <= 0)
            return;
        
        foreach (Zombie zombie in zombies)
            DamageZombie(zombie);

        Suicide();
    }
    protected override IEnumerable<(int row, int col)> GetNotifiedOnTiles()
    {
        yield return (row, col);
    }
    protected override void OnZombieEnterTile(int x, int y)
    {
        if(armed)
            Explode();
    }
}
