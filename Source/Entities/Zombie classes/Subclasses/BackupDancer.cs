using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class BackupDancer : Zombie
{
    public BackupDancer(int row) : base(row, stats)
    {
    }
    
    public static readonly ZombieStats stats = new ZombieStats(
        displayName: "Backup Dancer",
        maxHP: 200,
        wavePointCost: 1,
        waveWeight: 0,
        timeBetweenTicks: 2,
        speed: 1 / 5.5f,
        tags: ZombieTag.Summoned
        );

    Dancing Dancer = null;
    public void SetDancer(Dancing dancer)
    {
        Dancer = dancer;
    }
    public void ReleaseFromDancer()
    {
        Dancer = null;
    }


    protected override void Walk(float distance)
    {
        if(Dancer != null)
        {
            Dancer.RequestMovement(this, distance);
            return;
        }

        base.Walk(distance);
    }
    internal void ApplyDancerMovement(float distance)
    {
        if (Dancer == null || IsDead)
            return;

        AddToPosition(distance);
    }


    protected override void ManageDeath()
    {
        Dancer?.BackupDied(this);
        ReleaseFromDancer();
        base.ManageDeath();
    }


    protected override void UpdateSprite()
    {
        SetSprite(HPState * 3 + (eating ? 1 : 0));
    }
    
}
