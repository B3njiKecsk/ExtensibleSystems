using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Dancing : Zombie
{
    public Dancing(int row) : base(row, stats)
    {
    }
    public static readonly ZombieStats stats = new ZombieStats(
        displayName: "Dancing Zombie",
        maxHP: 300,
        speed: 1 / 5.5f,
        timeBetweenTicks: 3f,
        wavePointCost: 5,
        waveWeight: 1000
        );

    protected override void OnActorSet()
    {
        (actor as ZombieActor).FlipHorizontal(true);
    }

    enum MinionSide
    {
        Up,
        Down,
        Left,
        Right,
    }
    const float secondsPerGridSliding = 1.5f;
    readonly float slidingSpeedMultiplier = 1 / (secondsPerGridSliding * stats.Speed);
    Dictionary<MinionSide, BackupDancer> backups = new Dictionary<MinionSide, BackupDancer> 
    { 
        { MinionSide.Up, null }, 
        { MinionSide.Down, null }, 
        { MinionSide.Left, null }, 
        { MinionSide.Right, null } 
    };
    

    #region formation

    private (int row, int col) GetOffset(MinionSide side)
    {
        return side switch
        {
            MinionSide.Up => (1, 0),
            MinionSide.Down => (-1, 0),
            MinionSide.Left => (0, -1),
            MinionSide.Right => (0, 1),
            _ => (0, 0)
        };
    }

    private IEnumerable<BackupDancer> LivingBackups()
    {
        foreach (BackupDancer backup in backups.Values)
        {
            if (backup != null && !backup.IsDead)
                yield return backup;
        }
    }
    #endregion

    #region synchronization

    private readonly Dictionary<Zombie, float> movementRequests = new();

    internal void RequestMovement(Zombie backup, float distance)
    {
        movementRequests[backup] = distance;

        TryResolveMovement();
    }

    private void TryResolveMovement()
    {
        if (movementRequests.Count < LivingBackups().Count() + 1)   //needed movement requests are all backups plus self
            return;
        
        ApplyFormationMovement(movementRequests.Values.Min());
        movementRequests.Clear();
    }

    private void ApplyFormationMovement(float distance)
    {
        AddToPosition(distance);

        foreach (BackupDancer backup in LivingBackups())
            backup.ApplyDancerMovement(distance);
    }

    #endregion

    #region Movement

    protected override void Walk(float distance)
    {
        RequestMovement(this, distance);
    }

    #endregion


    #region Behaviour

    private enum State
    {
        Sliding,
        Walking,
        Pausing,
        Summoning
    }

    private State currentState = State.Sliding;

    protected override void BehaviourPerFrame(float timePassed)
    {
        switch(currentState)
        {
            case State.Sliding:
                Walk(timePassed * MovementSpeed * slidingSpeedMultiplier); break;
            case State.Walking:
                Walk(timePassed * MovementSpeed); break;
            case State.Pausing:
            case State.Summoning:
                Walk(0); break; //Even though we're not moving, we still submit a movement proposal to let the other members know that Dancing has participated in this movement cycle
        }
    }
    protected override void BehaviourPerTick()
    {
        switch(currentState)
        {
            case State.Sliding:
                currentState = State.Summoning;
                GatherBackups();
                actor.FlipHorizontal(false);
                break;
            case State.Summoning:
            case State.Pausing:
                currentState = State.Walking;
                break;
            case State.Walking:
                if (summonTick <= 0)
                {
                    currentState = State.Summoning;
                    GatherBackups();
                }
                else
                {
                    currentState = State.Pausing;
                }

                summonTick--;
                break;
        }
        UpdateSprite();
    }

    #endregion

    #region Summoning

    private int summonTick = 0;
    const int summonRechargeTicks = 3;
    private void GatherBackups()
    {
        summonTick = summonRechargeTicks;

        foreach(MinionSide side in backups.Keys.ToList())
        {
            BackupDancer backup = backups[side];

            if (backup != null && !backup.IsDead)
                continue;

            var offset = GetOffset(side);

            BackupDancer newBackup = SummonMinion(ZombieType.BackupDancer, row+ offset.row, xPos+ offset.col) as BackupDancer;

            if (newBackup == null)
                continue;

            newBackup.SetDancer(this);
            backups[side] = newBackup;
            
            (newBackup.actor as ZombieActor).RiseAnimation(1);

            if(GetEffect<HypnotizedEffect>() != null)
                newBackup.ApplyEffect(GetEffect<HypnotizedEffect>());
        }
    }

    #endregion

    #region Death
    internal void BackupDied(BackupDancer backup)
    {
        foreach(MinionSide side in backups.Keys.ToList())
        {
            if (backups[side] != backup)
                continue;

            backups[side] = null;
            break;
        }

        movementRequests.Remove(backup);
        TryResolveMovement();
    }
    protected override void ManageDeath()
    {
        foreach (BackupDancer backup in LivingBackups())
            backup.ReleaseFromDancer();

        movementRequests.Clear();
        base.ManageDeath();
    }
    #endregion

    protected override void UpdateSprite()
    {
        if (eating)
        {
            SetSprite(6 + HPState);
            return;
        }
        Debug.Log($"HPSTATE is currently at: {HPState}");
        switch (currentState)
        {
            case State.Sliding:
            case State.Walking: SetSprite(0 + HPState); break;
            case State.Pausing: SetSprite(2 + HPState); break;
            case State.Summoning: SetSprite(4 + HPState); break;
        }
    }
}
