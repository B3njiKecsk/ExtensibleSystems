using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Flags]
public enum PlantTag
{
    None = 0,
    Secondary = 1 << 0,
    Base = 1 << 1,
    CanPlantOnWater = 1 << 2,
    CanPlantOnWaterOnly = 1 << 3,
    CanPlantOnAny = 1 << 4,
    Instant = 1 << 5,
    CanPlaceOnPlant = 1 << 6,
    Upgrade = 1 << 7,
    SunProducer = 1 << 8,
    GraveBuster = 1 << 9,
    Lilypad = 1 << 10,
    Pot = 1 << 11,
    Mushroom = 1 << 12,
    NeedRechargeOnStart = 1 << 13,
    NotLeapable = 1 << 14,
}

public abstract class Plant : Entity
{
    public static class Recharge
    {
        public const float Fast = 7.5f;
        public const float Slow = 30f;
        public const float VerySlow = 50f;
    }
    public static class Toughness
    {
        public const int Normal = 300;
        public const int High = 4000;
        public const int VeryHigh = 8000;
    }
    protected World currentWorld;
    protected LevelDefinition currentLevel;
    protected PlantStats base_stats { get; private set; }
    public PlantStats Stats => base_stats;
    public int row { get; private set; }
    public int col { get; private set; }
    List<Action> unsubscribeActions = new();
    protected int ZombiesInTiles { get; private set; } = 0;
    public sealed override float PosX => col;


    #region construction
    protected Plant(int row, int col, PlantStats stats): base(Team.Plants, 0, stats.MaxHP, stats.TimeBetweenTicks)
    {
        currentWorld = GameManager.Instance.currentWorld;
        currentLevel = GameManager.Instance.currentLevel;
        base_stats = stats;
        this.row = row;
        this.col = col;
        ValidateStats();
        unsubscribeActions.AddRange(
            GameManager.Instance.levelManager.NotifyPlantForTiles(this, GetNotifiedOnTiles(), OnZombieEnterTile, OnZombieLeftTile));
    }
    private void ValidateStats()
    {
        if (string.IsNullOrEmpty(base_stats.DisplayName))
            Debug.LogError($"The Plant subclass '{this.GetType().Name}' has its stat.displayName, not setup. Its the only stat variable that is mandatory to be overwritten");
    }
    protected virtual void BehaviourOnStart()
    {

    }
    public void Start()
    {
        BehaviourOnStart();
    }
    #endregion

    #region Zombie notifications

    /// <summary>
    /// gets called whenever a new zombie spawns in the same lane this plant has been planted in
    /// </summary>
    /// <param name="zombie">The exact zombie that spawned</param>
    protected virtual void OnZombieEntersLane(Zombie zombie)
    {
        Debug.Log($"I, a {base_stats.DisplayName} got alerted that a zombie entered the lane!");
    }
    public void AlertZombieSpawned(Zombie zombie)
    {
        OnZombieEntersLane(zombie);
    }

    protected IEnumerable<Zombie> ZombiesOnTile(int row, int col)
    {
        return GameManager.Instance.levelManager.GetZombiesOnTile(row, col);
    }
    /// <summary>
    /// Defines the coordinates on the board at which OnZombieEnterTile and OnZombieLeftTile will be called
    /// </summary>
    /// <returns>Collection of (Row, Column) coordinates for which the plant should be notified</returns>
    protected virtual IEnumerable<(int row, int col)> GetNotifiedOnTiles()
    {
        return Enumerable.Empty<(int, int)>();
    }
    /// <summary>
    /// When a zombie enters tile at row,col this function gets called
    /// </summary>
    /// <remarks>Will only be called at coordinates defined in return of GetNotifiedOnTiles()</remarks>
    /// <param name="row"></param>
    /// <param name="col"></param>
    protected virtual void OnZombieEnterTile(int row, int col)
    {
        ZombiesInTiles++;
    }
    /// <summary>
    /// When a zombie leaves tile at row,col this function gets called
    /// </summary>
    /// <remarks>Will only be called at coordinates defined in return of GetNotifiedOnTiles()</remarks>
    /// <param name="row"></param>
    /// <param name="col"></param>
    protected virtual void OnZombieLeftTile(int row, int col)
    {
        ZombiesInTiles--;
    }
    #endregion

    #region find zombie
    protected IEnumerable<Zombie> AllZombies(TeamTarget targets = TeamTarget.Enemy)
    {
        return FilterTargets(
            GameManager.Instance.levelManager.GetAllZombies(), 
            targets);
    }
    protected IEnumerable<Zombie> ZombiesInRange(TeamTarget targets = TeamTarget.Enemy)
    {
        if (base_stats.Range < 0)
            return ZombiesInFront(targets);

        return FilterTargets(
            GameManager.Instance.levelManager.GetZombiesAtRange(row, col+0.5f, col+1f+base_stats.Range), 
            targets);
    }
    protected IEnumerable<Zombie> ZombiesInFront(TeamTarget targets = TeamTarget.Enemy)
    {
        return FilterTargets(
            GameManager.Instance.levelManager.GetZombiesInLaneFrom(row, col + 0.5f), 
            targets);
    }
    protected IEnumerable<Zombie> ZombiesInArea(int rowStart, int rowEnd, int colStart, int colEnd, TeamTarget targets = TeamTarget.Enemy)
    {
        return FilterTargets(
            GameManager.Instance.levelManager.GetZombiesAtArea(rowStart, rowEnd, colStart, colEnd),
            targets);
    }
    protected IEnumerable<Zombie> ZombiesInArea(int rowStart, int rowEnd, float xPosStart, float xPosEnd, TeamTarget targets = TeamTarget.Enemy)
    {
        return FilterTargets(
            GameManager.Instance.levelManager.GetZombiesAtArea(rowStart, rowEnd, xPosStart, xPosEnd), 
            targets);
    }
    protected IEnumerable<Zombie> ZombiesInRadius(float radius, TeamTarget targets = TeamTarget.Enemy)
    {
        return ZombiesInRadius(row, col, radius, targets);
    }
    protected IEnumerable<Zombie> ZombiesInRadius(int rowCenter, int colCenter, float radius, TeamTarget targets = TeamTarget.Enemy)
    {
        return FilterTargets(
            GameManager.Instance.levelManager.GetZombiesInRadius(rowCenter, colCenter, radius),
            targets);
    }
    protected bool AnyZombiesInFront(bool enemiesOnly = true, TeamTarget targets = TeamTarget.Enemy)
    {
        return ZombiesInFront(targets).Any();
    }
    #endregion

    #region Damage and Death
    protected override sealed void DestroySelf()
    {
        foreach (var unsubscribe in unsubscribeActions)
            unsubscribe();
        GameManager.Instance.levelManager.RemoveEntity(this);
    }
    public void Bite(DamageInfo damageInfo)
    {
        OnBitten(damageInfo);
        Damage(damageInfo);
    }
    protected virtual void OnBitten(DamageInfo damageInfo) { }

    protected void DamageZombie(Zombie z)
    {
        DamageZombie(z, Stats.AttackDamage);
    }
    protected void DamageZombie(Zombie z, DamageInfo.Direction sourceDirection)
    {
        DamageZombie(z, Stats.AttackDamage, sourceDirection);
    }
    protected void DamageZombie(Zombie z, float amount)
    {
        z.Damage(new DamageInfo(amount, this, z));
    }
    protected void DamageZombie(Zombie z, float amount, DamageInfo.Direction sourceDirection)
    {
        z.Damage(new DamageInfo(amount, this, z, sourceDirection));
    }

    protected void DamagePlant(Plant p)
    {
        DamagePlant(p, Stats.AttackDamage);
    }
    protected void DamagePlant(Plant p, DamageInfo.Direction sourceDirection)
    {
        DamagePlant(p, Stats.AttackDamage, sourceDirection);
    }
    protected void DamagePlant(Plant p, float amount)
    {
        p.Damage(new DamageInfo(amount, this, p));
    }
    protected void DamagePlant(Plant p, float amount, DamageInfo.Direction sourceDirection)
    {
        p.Damage(new DamageInfo(amount, this, p, sourceDirection));
    }

    public void ShovelUp()
    {
        OnShoveledUp();
        DestroySelf();
    }
    protected virtual void OnShoveledUp() { }
    #endregion

    #region Get info
    /// <summary>
    /// Given a Tile's data, should return true if the plant can be placed on that tile.<br></br> 
    /// The conditions are defined by the individual plant, be it the tile's terrain, or the plants already on the tile
    /// </summary>
    /// <param name="tile"></param>
    /// <returns>True if the plant can be planted here, False if the plant CANT be planted here</returns>
    public virtual bool CanPlantHere(TileType type, Plant basePlant, Plant mainPlant, Plant overlayPlant, TileObject tileObject)
    {
        return (type == TileType.Normal
            || basePlant != null && (
                type == TileType.Water && basePlant.Stats.HasTag(PlantTag.Lilypad) ||
                type == TileType.Roof && basePlant.Stats.HasTag(PlantTag.Pot)
                ))
                && tileObject == TileObject.None;
                ;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns>Every plant on the current tile, excluding this</returns>
    protected IEnumerable<Plant> PlantsOnTile()
    {
        foreach (Plant plant in GameManager.Instance.levelManager.GetAllPlantsAtCell(row, col))
            if (plant != this && plant != null)
                yield return plant;
    }

    protected IEnumerable<Plant> PlantsOnTile(int col, int row)
    {
        foreach (Plant plant in GameManager.Instance.levelManager.GetAllPlantsAtCell(row, col))
            if (plant != this && plant != null)
                yield return plant;
    }

    /// <summary>
    /// Given the zombie trying to munch on this plant, should return true if said zombie is allowed to eat.<br></br>
    /// By default, returns true only if the plant has the Instant tag
    /// </summary>
    /// <param name="origin"></param>
    /// <returns></returns>
    public virtual bool CanBeEaten(Zombie origin)
    {
        if (base_stats.HasTag(PlantTag.Instant))
            return false;
        return true;
    }

    public sealed override bool CanBeHit(Projectile projectile)
    {
        return CanBeHitByProjectile(projectile);
    }

    protected override bool CanBeHitByProjectile(Projectile projectile)
    {
        return true;
    }

    #endregion

    #region SubClass Specific

    protected void ShootProjectile(ProjectileType type, ActorFace face)
    {
        base.ShootProjectile(type, face, col, row);
    }
    protected void SummonSun(int amount)
    {
        (float col, float row) origin = PlantActor.AdjustWorldPosWithFace((col, row), ActorFace.Center);
        GameManager.Instance.levelManager.SpawnSun(amount, origin.col, origin.row);
    }

    protected void SummonCoin(int amount)
    {
        (float col, float row) origin = PlantActor.AdjustWorldPosWithFace((col, row), ActorFace.Center);
        GameManager.Instance.levelManager.SpawnCoins(amount, origin.col, origin.row);
    }

    protected void RemoveTileObject(int row, int col)
    {
        GameManager.Instance.levelManager.SetTile(row, col, TileObject.None);
    }
    protected void ModifyTileObject(int row, int col, TileObject type)
    {
        GameManager.Instance.levelManager.SetTile(row,col, type);
    }

    public PlantingResult AttemptMovingPlant(int row, int col)
    {
        return GameManager.Instance.levelManager.AttemptMovingPlant(this, row, col);
    }

    protected PlantingResult AttemptMovingTileObjects(int rowFrom, int colFrom, int rowTo, int colTo)
    {
        return GameManager.Instance.levelManager.AttemptSwappingTileObjects(rowFrom, colFrom, rowTo, colTo, true);
    }

    public void UpdateTile(int row, int col)
    {
        this.row = row;
        this.col = col;
    }
    protected TileObject TileObjectAt(int col, int row)
    {
        return GameManager.Instance.levelManager.TileObjectAt(row, col);
    }

    #endregion

    protected bool CurrentWorldHasTag(WorldTag tag)
    {
        return currentWorld.HasTag(tag);
    }

    #region clicks
    public bool Click()
    {
        return OnClicked();
    }

    /// <summary>
    /// Called when clicked on while the player is not holding anything
    /// </summary>
    /// <returns>True if clicking on the plant should make it selected, having the players next click call OnSecondClick(col, row)</returns>
    protected virtual bool OnClicked()
    {
        return false;
    }

    public void TileClick(int row, int col)
    {
        OnSecondClick(row, col);
    }

    protected virtual void OnSecondClick(int row, int col)
    {

    }
#endregion
}
