using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Chomper : Plant
{
    public Chomper(int row, int col) : base(row, col, stats)
    {
        
    }

    public static readonly PlantStats stats = new PlantStats
    (
        displayName: "Chomper",
        maxHP: Toughness.Normal,
        rechargeTime: Recharge.Fast,
        attackDamage: 1800,
        sunCost: 150
    );
    private const float timeToStartEating = 0.5f;
    private const float timeToSwallow = 42f;
    private bool eating = false;
    private bool chomping = false;

    protected override IEnumerable<(int row, int col)> GetNotifiedOnTiles()
    {
        yield return (row, col);
        yield return (row, col + 1);
    }

    protected override void OnZombieEnterTile(int row, int col)
    {
        if (eating)
            return;
        Chomp();
    }
    protected override void BehaviourOnStart()
    {
        CheckZombiesInRange();
    }
    private void CheckZombiesInRange()
    {
        Zombie z = ZombiesOnTile(row, col).FirstOrDefault();
        if (z == null)
        {
            z = ZombiesOnTile(row, col + 1).FirstOrDefault();
            if (z == null)
                return;
        }
        Chomp();
    }
    private void Chomp()
    {
        chomping = true;
        SetSprite(1);
        SetTimeBeforeNextTick(timeToStartEating);
    }
    
    protected override void BehaviourPerTick()
    {
        if (chomping)
            TryEatingZombies();
        else if (eating)
            Swallow();
    }
    private void TryEatingZombies()
    {
        chomping = false;
        SetSprite(0);
        Zombie z = ZombiesOnTile(row, col).FirstOrDefault();
        if (z == null)
            z = ZombiesOnTile(row, col + 1).FirstOrDefault();
        Eat(z);
    }
    private void Eat(Zombie zombie)
    {
        if (zombie == null)
            return;
        DamageZombie(zombie);
        eating = true;
        SetSprite(2);
        SetTimeBeforeNextTick(timeToSwallow);
    }
    private void Swallow()
    {
        SetSprite(0);
        eating = false;
        CheckZombiesInRange();
    }
    
    
}
