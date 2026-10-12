using UnityEngine;

public class Polevaulter : Zombie
{
    public Polevaulter(int row) : base(row, stats)
    {
    }

    public static readonly ZombieStats stats = new ZombieStats
    (
        displayName: "Pole Vaulting Zombie",
        maxHP: 333,
        dps: 100,
        speed: 1 / 4.7f,
        wavePointCost: 2,
        waveWeight: 2000
    );
    float timeToJump = 0.8f;
    const float leapStartRange = 1f;
    const float secondsPerGridRunning = 2.5f;
    readonly float RunningSpeedMultiplier = 1 / (secondsPerGridRunning * stats.Speed);
    const float cellsToJump = 1f + GameManager.ZombieTileBackReach + leapStartRange;
    int phase = 0;
    bool running => phase == 0;
    bool jumping => phase == 1;
    Plant jumpingPlant;
    protected override void Walk(float distance)
    {
        if (running)
            base.Walk(distance * RunningSpeedMultiplier);
        else if (jumping)
            return;
        else
            base.Walk(distance);
    }

    protected override void BehaviourPerFrame(float timePassed)
    {
        if (jumping)
        {
            timeToJump -= timePassed;
            if (timeToJump < 0)
            {
                phase++;
                UpdateSprite();
                if (jumpingPlant.Stats.HasTag(PlantTag.NotLeapable))
                    return;
                AddToPosition(cellsToJump);
            }
            return;
        }
        base.BehaviourPerFrame(timePassed);


    }
    protected override bool AttemptToEatPlant(float timePassed)
    {
        if(running)
        {
            Plant plant = PlantOnColumn(col);
            if (plant == null)
                plant = PlantOnColumn((int)(xPos-leapStartRange));
            if (plant!=null)
            {
                phase++;
                UpdateSprite();
                jumpingPlant = plant;
            }
            return false;
        }
        return base.AttemptToEatPlant(timePassed);
    }
    protected override bool CanBeHitByProjectile(Projectile projectile)
    {
        return !jumping;
    }
    protected override void UpdateSprite()
    {
        if (jumping)
            SetSprite(6);
        else
            SetSprite(phase + (eating? 2:0) + HPState);
    }
}
