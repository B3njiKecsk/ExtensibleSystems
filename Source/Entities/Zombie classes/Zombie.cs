using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

[Flags]
public enum ZombieTag
{
    None = 0,
    ImmuneToLawnMowers = 1 << 0,
    Summoned = 1 << 1,
}

public abstract class Zombie: Entity
{
    //should be 1.0f = 1 cell
    //public float distanceTravelled { get; protected set; }
    public int row { get; protected set; }
    protected LaneType currentLaneType { get; private set; }
    private ZombieStats base_stats;
    public ZombieStats Stats => base_stats;
    protected int HPState;
    protected bool eating { get; private set; } = false;
    private bool movesRight = false;
    private int direction => movesRight ? 1 : -1;
    public bool MovesRight => movesRight;

    //private float tickTimeLeft;
    private float ZombieTooFarRight => GameManager.Instance.currentWorld.geometry.zombieSpawnCol+1;

    #region construction
    public Zombie(int row, ZombieStats stats): base(Team.Zombies, stats.Speed, stats.MaxHP, stats.TimeBetweenTicks)
    {
        base_stats = stats;
        this.row = row;
        currentLaneType = GameManager.Instance.currentWorld.GetLaneType(GameManager.Instance.currentLevelIndex + 1, row);
        xPos = GameManager.Instance.currentWorld.geometry.zombieSpawnCol;
    }

    public sealed override bool CanBeHit(Projectile projectile)
    {
        if (projectile is LawnMower)
            return !base_stats.HasTag(ZombieTag.ImmuneToLawnMowers);
        else
            return CanBeHitByProjectile(projectile);
    }
    #endregion

    #region update

    protected override void BehaviourPerFrame(float timePassed)
    {
        SetEating(AttemptToEat(timePassed));
        if (eating)
            return; //if a zombie can eat a plant (unless overriden), itll stop to do so

        Walk(timePassed*MovementSpeed);
    }
    protected virtual void Walk(float distance)
    {
        AddToPosition(distance);
    }

    protected void AddToPosition(float distance)
    {
        int col = this.col;
        xPos += distance * direction;
        int newCol = this.col;
        actor.UpdatePosition(xPos, row);
        if(newCol < -1)
        {
            GameManager.Instance.Lose(this);
        }
        if(col != newCol)
            GameManager.Instance.levelManager.UpdateZombieTile(this, col, newCol);

        if (xPos > ZombieTooFarRight)
            Suicide();
    }
    public void FlipDirection() { SetDirection(!movesRight); }
    public void SetDirection(bool right)
    {
        if (movesRight == right)
            return;

        movesRight = right;
        actor.FlipHorizontal(right);
        FlipEquipment();
        OnDirectionChanged(right);
    }
    protected virtual void OnDirectionChanged(bool movesRight) { }
    private void FlipEquipment()
    {
        foreach (Armor item in equipment.Where(e => e is Armor))
            item.FlipDirection();
    }
    protected virtual bool AttemptToEat(float timePassed)
    {
        if (AttemptToEatZombie(timePassed))
            return true;

        return AttemptToEatPlant(timePassed);
    }
    protected virtual bool AttemptToEatPlant(float timePassed)
    {
        Plant plantToEat = FindPlantToEat();
        if (plantToEat == null)
            return false;
        EatPlant(plantToEat, Stats.Dps * timePassed);
        return true;
    }
    protected virtual bool AttemptToEatZombie(float timePassed)
    {
        Zombie zombieToEat = FindZombieToEat();
        if (zombieToEat == null)
            return false;
        EatZombie(zombieToEat, Stats.Dps * timePassed);
        return true;
    }

    #endregion

    #region damage and death
    protected override void OnDamageTaken(DamageInfo info)
    {
        UpdateHPState();
        GameManager.Instance.levelManager.NotifyWaveHP(this);
    }
    
    protected override sealed void DestroySelf()
    {
        GameManager.Instance.levelManager.RemoveEntity(this);
    }

    #endregion

    #region visual state
    /// <summary>
    /// Called after the zombie takes damage, should return what the new value of HPState should be, advancing to the next spriteset
    /// </summary>
    protected virtual int CalculateHPState()
    {
        return HP > (base_stats.MaxHP/2) ? 0 : 1;
    }

    private void UpdateHPState()
    {
        int state = CalculateHPState();

        if (state == HPState)
            return;

        HPState = state;
        UpdateSprite();
    }
    private void SetEating(bool value)
    {
        if (value == eating)
            return;
        eating = value;
        UpdateSprite();
    }

    /// <summary>
    /// calls SetSprite, with a default formula for determining the spriteID
    /// </summary>
    protected virtual void UpdateSprite()
    {
        SetSprite(HPState * 2 + (eating ? 1 : 0));
    }
    #endregion

    #region get info

    protected static int DistanceTravelledToColumn(float dt)
    {
        return (int)DistanceTravelledToXPos(dt);
    }
    protected static float DistanceTravelledToXPos(float dt)
    {
        return GameManager.Instance.currentWorld.geometry.zombieSpawnCol - dt;
    }
    public float xPos;
    public sealed override float PosX => xPos;
    public int col => (int)xPos;
    #endregion

    #region plants basics
    protected virtual Plant FindPlantToEat()
    {
        Plant P2E;
        
        P2E = PlantOnColumn((int)(xPos+(GameManager.ZombieTileBackReach)*direction));
        if (P2E!=null && P2E.CanBeEaten(this) && IsEnemy(P2E))
            return P2E;

        P2E = PlantOnColumn((int)xPos);
        if (P2E!=null && P2E.CanBeEaten(this) && IsEnemy(P2E))
            return P2E;

        return null;
    }

    
    protected void EatPlant(Plant plant, float amount)
    {
        plant.Bite(new DamageInfo(amount, this, plant));
    }
    

    protected Plant PlantOnColumn(int col)
    {
        if (col >= GameManager.Instance.currentWorld.geometry.colN || col < 0)
            return null;
        return GameManager.Instance.levelManager.GetTopPlantAtCell(row, col);
    }
    #endregion

    #region eating zombies

    protected virtual Zombie FindZombieToEat()
    {
        Zombie Z2E;

        
        float xPosStart = xPos - (movesRight? GameManager.ZombieTileBackReach : GameManager.ZombieReachZombie);
        float xPosEnd = xPos + (movesRight? GameManager.ZombieReachZombie : GameManager.ZombieTileBackReach);
        

        Z2E = GameManager.Instance.levelManager.GetZombiesAtRange(row, xPosStart, xPosEnd).Where(z => IsEnemy(z)).FirstOrDefault();
        
        return Z2E;
    }
    protected void EatZombie(Zombie zombie, float amount)
    {
        GameManager.Instance.levelManager.EnqueueDamage(new DamageInfo(amount, this, zombie, DamageInfo.Direction.directionless));
        //NOT zombie.Damage(new DamageInfo(amount, this, zombie));
        //we enqueue damage, for zvz action, as same types against each other should tie
    }

    #endregion

    #region Subclass specific
    protected void ShootProjectile(ProjectileType type, ActorFace face)
    {
        base.ShootProjectile(type, face, col, xPos);
    }

    public void MoveToLane(int lane)
    {
        GameManager.Instance.levelManager.ZombieSwitchLanes(this, row, lane);
        row = lane;
        currentLaneType = GameManager.Instance.currentWorld.GetLaneType(GameManager.Instance.currentLevelIndex + 1, row);
        actor.SetLane(lane, EntityActor.LaneRenderLayer.Zombie);
    }

    protected Zombie SummonMinion(ZombieType type, int row, float col)
    {
        return GameManager.Instance.levelManager.AttemptZombieMinionSpawn(type, row, col);
    }
    #endregion

    #region Plant Scanning

    protected Plant PlantAtRange(int colStart, int colEnd)
    {
        //force colStart to be leftest
        if (colStart > colEnd)
        {
            int t = colStart;
            colStart = colEnd;
            colEnd = t;
        }
        Plant target;
        for (int i = colEnd; i < colStart; i++)
        {
            target = PlantOnColumn(i);
            if (target != null)
                return target;
        }
        return null;
    }
    protected Plant PlantInRange(int range)
    {
        return PlantAtRange(col, col - range);
    }

    #endregion

    protected override void OnTeamChange(Team oldTeam, Team newTeam)
    {
        Debug.Log($"zombie switched teams from {oldTeam} to {newTeam}");
        base.OnTeamChange(oldTeam, newTeam);
        if (newTeam != Team.Zombies)
            GameManager.Instance.levelManager.NotifyWaveHP(this);
    }
}
