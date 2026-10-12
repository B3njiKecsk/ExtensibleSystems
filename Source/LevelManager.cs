using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;


public class LevelManager
{
    const float EntityCollisionLength = GameManager.ZombieCollisionLength;
    System.Random random;
    Lane[] Lanes;
    private int rowN = 5;
    private int colN = 9;
    List<Projectile> projectiles = new();
    public int sunAvailable { get; private set; } = GameManager.startingSun;

    private List<Zombie> zombieUpdateBuffer = new();
    private List<Plant> plantsUpdateBuffer = new();
    private List<Projectile> projectilesUpdateBuffer = new();

    private Queue<DamageInfo> pendingDamage = new();
    

    private LevelUI GUI;

    public LevelManager(int rowN, int colN, LevelUI levelUI, System.Random random, World currentWorld, LevelDefinition currentLevel)
    {
        this.rowN = rowN;
        this.colN = colN;
        GUI = levelUI;
        this.random = random;

        Lanes = new Lane[rowN];
        for (int i = 0; i < Lanes.Length; i++) { Lanes[i] = new Lane(i, colN, currentWorld.GetLaneType(GameManager.Instance.currentLevelIndex+1, i)); }

        GenerateLawnMowers();
        StartLevel(GameManager.Instance.currentLevelIndex);
        _WaveManager = new ZombieWaveManager(levelUI.ProgressBar, random, SpawnZombie, SpawnAmbushZombie, Lanes.Select(w => w.Type).ToArray(), currentLevel.Waves, currentLevel.AllowedZombies, currentLevel.ScriptedWaves);
    }
    private void StartLevel(int level)
    {
        GameManager.Instance.currentWorld.StartLevel(level, this);
    }

    private void GenerateLawnMowers()
    {
        float yPos = 0.75f;
        float xPos = -0.5f;
        for (int i = 0; i < Lanes.Length;i++)
        {
            SpawnProjectile(null, ProjectileType.LawnMower, xPos, yPos + i);
        }
    }
    const int WavesPausedOnMowerTriggered = 2;
    public void TriggeredMower(int lane)
    {
        _WaveManager.PauseZombieSpawns(lane, WavesPausedOnMowerTriggered);
    }

    #region zombie waves

    private ZombieWaveManager _WaveManager;
    public void NotifyWaveHP(Zombie z)
    {
        _WaveManager.UpdateDamage(z);
    }
    public void _DBG_NextWave()
    {
        _WaveManager._DBG_NextWave();
    }

    #endregion


    public void Update(float deltaTime)
    {
        _WaveManager.Update(deltaTime);
        GameManager.Instance.currentWorld.Update(deltaTime);
        Hotbar.UpdateSlots(deltaTime);

        //copy entities lists
        foreach (Lane row in Lanes)
        {
            zombieUpdateBuffer.AddRange(row.Zombies);

            foreach (TileData tile in row.tiles)
                plantsUpdateBuffer.AddRange(tile.AllPlants);
        }
        projectilesUpdateBuffer.AddRange(projectiles);

        //update them trough copy lists
        //cant be foreach, because we change these elements to null once they die, to not refer to them next frame
        for (int i = 0; i < zombieUpdateBuffer.Count; i++)
            zombieUpdateBuffer[i]?.Update(deltaTime);
        for (int i = 0; i < plantsUpdateBuffer.Count; i++)
            plantsUpdateBuffer[i]?.Update(deltaTime);
        for (int i = 0; i < projectilesUpdateBuffer.Count; i++)
            projectilesUpdateBuffer[i]?.Update(deltaTime);
            
        //clearup pending damage
        while(pendingDamage.Count > 0)
        {
            DamageInfo damage = pendingDamage.Dequeue();
            if (damage.Target != null && !damage.Target.IsDead)
                damage.Target.Damage(damage);
        }

        //clear lists
        zombieUpdateBuffer.Clear();
        plantsUpdateBuffer.Clear();
        projectilesUpdateBuffer.Clear();
    }
    public void EnqueueDamage(DamageInfo info)
    {
        pendingDamage.Enqueue(info);
    }

    #region Spawn entities
    public Zombie SpawnZombie(ZombieType type, int row)
    {
        Zombie newZombie = ZombieRegistry.Create(type, row);
        ZombieActor actor = GameManager.Instance.SpawnZombiePrefab(type, row);
        newZombie.SetActor(actor);
        Lanes[row].AddZombie(newZombie);
        return newZombie;
    }
    private Zombie SpawnZombieAt(ZombieType type, int row, float xPos)
    {
        Zombie newZombie = ZombieRegistry.Create(type, row);
        newZombie.xPos = xPos;
        ZombieActor actor = GameManager.Instance.SpawnZombiePrefab(type, row, xPos);
        newZombie.SetActor(actor);
        Lanes[row].AddZombie(newZombie);
        UpdateZombieTile(newZombie, -999, (int)xPos);
        return newZombie;
    }
    public Zombie SpawnAmbushZombie(ZombieType type, int row, float xPos)
    {
        Zombie newZombie = SpawnZombieAt(type, row, xPos);
        GameManager.Instance.currentWorld.StartAmbushAnimation((ZombieActor)newZombie.actor);
        return newZombie;
    }
    public PlantingResult PlacePlant(int row, int col, PlantType plantType)
    {
        if (row >= rowN || col >= colN || row < 0 || col < 0)
            return PlantingResult.OutOfBounds;

        if (PlantRegistry.GetStats(plantType).SunCost > sunAvailable)
            return PlantingResult.NotEnoughSun;

        Plant plant = PlantRegistry.Create(plantType, row, col);
        PlantingResult result = Lanes[row].tiles[col].AttemptToPlacePlant(plant);

        if (result != PlantingResult.Success)
            return result;

        PlantActor actor = GameManager.Instance.PlacePlantPrefab(plantType, row, col);
        plant.SetActor(actor);
        plant.Start();

        SpendSun(PlantRegistry.GetStats(plantType).SunCost);

        return result;
    }

    public PlantingResult CanPlacePlant(int row, int col, PlantType plantType)
    {
        if (row >= rowN || col >= colN || row < 0 || col < 0)
            return PlantingResult.OutOfBounds;

        if (PlantRegistry.GetStats(plantType).SunCost > sunAvailable)
            return PlantingResult.NotEnoughSun;

        Plant plant = PlantRegistry.Create(plantType, row, col);

        return Lanes[row].tiles[col].CanPlacePlant(plant);
    }

    public void SpawnProjectile(Entity origin, ProjectileType type, float xPos, float yPos)
    {
        Projectile newProjectile = ProjectileRegistry.Create(type, xPos, yPos);
        ProjectileActor actor = GameManager.Instance.SpawnProjectilePrefab(type, xPos, yPos);
        newProjectile.SetActor(actor);
        newProjectile.SetTeam(origin==null? Team.Plants : origin.team);
        projectiles.Add(newProjectile);
    }
    #endregion

    #region remove entities
    public void RemoveEntity(Plant plant)
    {
        RemoveEntityFromGame(plant);
        RemoveFromUpdateBuffer(plant);
        GameManager.Instance.RemoveActorPrefab(plant.actor);
    }
    public void RemoveEntity(Zombie zombie)
    {
        RemoveEntityFromGame(zombie);
        RemoveFromUpdateBuffer(zombie);
        GameManager.Instance.RemoveActorPrefab(zombie.actor);
    }
    public void RemoveEntity(Projectile projectile)
    {
        RemoveEntityFromGame(projectile);
        RemoveFromUpdateBuffer(projectile);
        GameManager.Instance.RemoveActorPrefab(projectile.actor);
    }
    //remove from lists
    private void RemoveEntityFromGame(Plant plant)
    {
        Lanes[plant.row].tiles[plant.col].RemovePlant(plant);
    }
    private void RemoveEntityFromGame(Zombie zombie)
    {
        Lanes[zombie.row].RemoveZombie(zombie);
        if(zombie.row < rowN && zombie.row >= 0 && zombie.col < colN && zombie.col >= 0)
            Lanes[zombie.row].tiles[zombie.col].ZombieLeavesTile(zombie);
    }
    private void RemoveEntityFromGame(Projectile projectile)
    {
        projectiles.Remove(projectile);
    }
    //remove from update buffer
    private void RemoveFromUpdateBuffer(Plant plant)
    {
        int index = plantsUpdateBuffer.IndexOf(plant);

        if (index >= 0)
            plantsUpdateBuffer[index] = null;
    }
    private void RemoveFromUpdateBuffer(Zombie zombie)
    {
        int index = zombieUpdateBuffer.IndexOf(zombie);

        if (index >= 0)
            zombieUpdateBuffer[index] = null;
    }
    private void RemoveFromUpdateBuffer(Projectile projectile)
    {
        int index = projectilesUpdateBuffer.IndexOf(projectile);

        if (index >= 0)
            projectilesUpdateBuffer[index] = null;
    }
    #endregion

    #region Sun
    public void SpawnSun(int amount, float xPos, float yPos)
    {
        GameManager.Instance.GenerateSunPrefab(amount, xPos, yPos, yPos);
    }
    public void SpawnFallingSun(int amount, float xPos, float fallToYPos)
    {
        GameManager.Instance.GenerateSunPrefab(amount, xPos, -2, fallToYPos);
    }

    public void CollectSun(int amount)
    {
        AddSunAvailable(amount);
    }
    private void SpendSun(int amount)
    {
        AddSunAvailable(-amount);
    }
    public void StealSun(int amount)
    {
        AddSunAvailable(-amount);
    }
    private void AddSunAvailable(int amount)
    {
        sunAvailable += amount;
        GUI.UpdateSunCounter(sunAvailable);
    }
    #endregion

    #region coin

    public List<CoinActor> SpawnCoins(int amount, float col, float row)
    {
        Debug.Log("Gonna spawn some coins!");
        return GameManager.Instance.GenerateCoinPrefabs(amount, col, row);
    }

    public void SpawnCoinsCollected(int amount, float col, float row)
    {
        foreach(CoinActor coin in SpawnCoins(amount, col, row))
            coin.OnMouseDown();
    }

    public void CollectCoin(int amount)
    {
        GUI.UpdateCoinCounter(GameManager.ModifyBalance(amount));
    }

    #endregion

    #region Get Zombies
    /// <returns>Every zombie that is physically on the given tile</returns>
    public IEnumerable<Zombie> GetZombiesOnTile(int row, int col)
    {
        if (row < 0 || col < 0 || row >= rowN || col >= colN)
            return Enumerable.Empty<Zombie>();
        return new List<Zombie>(Lanes[row].tiles[col].zombies);
    }
    /// <returns>Every zombie that has their tile coordinates somewhere in between the given range</returns>
    public IEnumerable<Zombie> GetZombiesAtRange(int row, float xPosStart, float xPosEnd)
    {
        return Lanes[row].GetZombiesAtRange(xPosStart, xPosEnd);
    }
    /// <returns>Every zombie in the lane, to the right of xPos</returns>
    public IEnumerable<Zombie> GetZombiesInLaneFrom(int row, float xPos)
    {
        return Lanes[row].GetZombiesInFrontOf(xPos);
    }
    /// <returns>Every zombie in an area defined by two points on the grid</returns>
    public IEnumerable<Zombie> GetZombiesAtArea(int laneStart, int laneEnd, float xPosStart, float xPosEnd)
    {
        for (int i = laneStart; i <= laneEnd; i++)
            foreach (Zombie zombie in Lanes[i].GetZombiesAtRange(xPosStart, xPosEnd))
                yield return zombie;
    }
    /// <returns>Every zombie in a square area defined by two points on the grid</returns>
    public IEnumerable<Zombie> GetZombiesAtArea(int laneStart, int laneEnd, int colStart, int colEnd)
    {
        for (int row = laneStart; row <= laneEnd; row++)
            for (int col = colStart; col <= colEnd; col++)
                foreach (Zombie zombie in GetZombiesOnTile(row, col))
                    yield return zombie;
    }
    /// <returns>Every zombie in the area of a circle defined by the given radius</returns>
    public IEnumerable<Zombie> GetZombiesInRadius(int rowCenter, int colCenter, float radius)
    {
        List<Zombie> zombies = new List<Zombie>();
        zombies.AddRange(Lanes[rowCenter].GetZombiesAtRange(colCenter - radius, colCenter + radius));

        for (int laneDistance = 1; laneDistance <= radius; laneDistance++)
        {
            float horizontalRadius = (float)Math.Sqrt(radius * radius - laneDistance * laneDistance);

            if(rowCenter + laneDistance < rowN)
                zombies.AddRange(Lanes[rowCenter + laneDistance].GetZombiesAtRange(colCenter - horizontalRadius, colCenter + horizontalRadius));
            if(rowCenter - laneDistance >= 0)
                zombies.AddRange(Lanes[rowCenter - laneDistance].GetZombiesAtRange(colCenter - horizontalRadius, colCenter + horizontalRadius));
        }

        return zombies;
    }
    /// <returns>Every spawned zombie currently alive</returns>
    public IEnumerable<Zombie> GetAllZombies()
    {
        foreach(Lane lane in Lanes)
            foreach(Zombie zombie in lane.Zombies.ToArray())
                yield return zombie;
    }

    #endregion


    public Plant GetTopPlantAtCell(int row, int col)
    {
        if(IsTileInBounds(row, col))
            return Lanes[row].tiles[col].GetTopPlant();
        return null;
    }

    public Plant[] GetAllPlantsAtCell(int row, int col)
    {
        return Lanes[row].tiles[col].AllPlants.ToArray();
    }

    public Zombie ProjectileHitZombie(Projectile projectile, int lane, float xPos, TeamTarget teamTarget = TeamTarget.Enemy)
    {
        if (lane >= rowN || lane < 0)
            return null;

        foreach (Zombie zombie in Lanes[lane].GetZombiesNearPositionOrdered(xPos, EntityCollisionLength))
        {
            if (!projectile.IsValidTeamTarget(zombie, teamTarget))
                continue;

            if (!projectile.CanTarget(zombie))
                continue;

            if (!zombie.CanBeHit(projectile))
                continue;

            return zombie;
        }

        return null;
    }
    public Plant ProjectileHitPlant(Projectile projectile, int lane, int col, TeamTarget teamTarget = TeamTarget.Enemy)
    {
        if (!IsTileInBounds(lane, col))
            return null;

        Plant plant = Lanes[lane].tiles[col].GetTopPlant();
        if (plant == null)
            return null;

        if (!projectile.IsValidTeamTarget(plant, teamTarget))
            return null;
        if (!projectile.CanTarget(plant))
            return null;
        if (!plant.CanBeHit(projectile))
            return null;

        return plant;
    }

    public void UpdateZombieTile(Zombie zombie, int col, int newCol)
    {
        if(col >= 0 && col < colN)
            Lanes[zombie.row].tiles[col].ZombieLeavesTile(zombie);
        if(newCol >= 0 && newCol < colN)
            Lanes[zombie.row].tiles[newCol].ZombieEntersTile(zombie);
    }

    public IEnumerable<Action> NotifyPlantForTiles(Plant p, IEnumerable<(int row, int col)> tilesToWatch, Action<int, int> enterTile, Action<int, int> leaveTile)
    {
        foreach (var tile in tilesToWatch)
        {
            yield return GetTileOnBoard(tile.row, tile.col).SubscribePlantForZombieMovement(p, enterTile, leaveTile);
        }
        yield break;
    }
    private TileData GetTileOnBoard(int row, int col)
    {
        if (row < 0)
            row = 0;
        else if (row >= rowN)
            row = rowN - 1;
        if (col < 0)
            col = 0;
        else if (col >= colN)
            col = 0;
        return Lanes[row].tiles[col];
    }

    public void ZombieSwitchLanes(Zombie zombie, int rowFrom, int rowTo)
    {
        Lanes[rowFrom].RemoveZombie(zombie);
        Lanes[rowTo].AddZombie(zombie);
    }

    public (int row, int col) SetRandomTileInRange(int rowFrom, int colFrom, int rowTo, int colTo, TileObject type)
    {
        var validTiles = new List<(int Row, int Col)>();

        for (int row = rowFrom; row <= rowTo; row++)
            for (int col = colFrom; col <= colTo; col++)
                if (GetTileOnBoard(row, col).tileType == TileType.Normal)
                    validTiles.Add((row, col));


        if (validTiles.Count == 0)
            throw new InvalidOperationException($"The specified area contains no valid tiles.\nArea length = {(Math.Min(rowTo,rowN-1) - Math.Max(rowFrom, 0)) * (Math.Min(colTo, colN - 1) - Math.Max(colFrom, 0))}\nValid Tiles = TileType.Normal");

        var (selectedRow, selectedCol) = validTiles[random.Next(validTiles.Count)];
        SetTile(selectedRow, selectedCol, type);
        return (selectedRow, selectedCol);
    }

    public void SetTile(int row, int col, TileObject type)
    {
        TileActor actor = GameManager.Instance.SpawnTilePrefab(row, col, type);
        GetTileOnBoard(row, col).ReplaceTileObject(type, actor);
    }

    public void RemoveTileObject(int row, int col)
    {
        GetTileOnBoard(row, col).RemoveTileObject();
    }

    public Zombie AttemptZombieMinionSpawn(ZombieType type, int row, float col)
    {
        if (!IsTileInBounds(row, (int)col) || !ZombieRegistry.CanSpawnOnLane(type, Lanes[row].Type))
            return null;

        if(row>= Lanes.Length || row<0) return null;
        if (col >= Lanes[0].tiles.Length || col < 0) return null;

        return SpawnZombieAt(type, row, col);
    }

    public bool TileHasObject(TileObject type, int row, int col)
    {
        return TileObjectAt(row, col) == type;
    }
    public TileObject TileObjectAt(int row, int col)
    {
        return Lanes[row].tiles[col].tileObject;
    }

    public bool TileIsOfType(TileType type, int row, int col)
    {
        return TileTypeAt(row, col) == type;
    }
    public TileType TileTypeAt(int row, int col)
    {
        return Lanes[row].tiles[col].tileType;
    }

    public void WinLevel(Zombie lastZombie)
    {
        Debug.Log("WIN!");
        float xPos = lastZombie.xPos;
        if (lastZombie.xPos > colN)
            xPos = colN / 2;
        GameManager.Instance.SpawnRewardPrefab(GameManager.Instance.currentLevel.reward, xPos, lastZombie.row+1);
    }

    public void DisplayReward(LevelReward reward)
    {
        if (reward.rewardType == RewardType.SeedPacket && reward.rewardPlant == null)
            throw new InvalidOperationException($"The reward for level {GameManager.Instance.currentWorldIndex + 1}-{GameManager.Instance.currentLevelIndex + 1} was either not specified, or set to a seedpacket, with the seedpacket not specified. Please correct this in {GameManager.Instance.currentWorld.GetType()}.{nameof(World.GetLevelReward)}()");

        GameManager.Instance.UnlockReward(reward);
        GUI.ShowReward(reward);
    }

    public void EndLevel()
    {
        Debug.Log("collecting lawnmower coins");
    }

    public void ShovelUpPlant(int cellX, int cellY)
    {
        if (Lanes[cellY].tiles[cellX].GetTopPlant() == null)
            return;
        Lanes[cellY].tiles[cellX].GetTopPlant().ShovelUp();
    }

    private bool IsTileInBounds(int row, int col)
    {
        if (row < 0 || col < 0)
            return false;

        if (row >= rowN || col >= colN)
            return false;

        return true;
    }

    public bool CanLaneSpawnZombie(ZombieType z, int lane, int currentWave)
    {
        return Lanes[lane].CanSpawnZombies(currentWave);
    }
    /*
    public PlantingResult AttemptSwappingTileData(int row1, int col1, int row2, int col2)
    {
        Plant[] plants1 = GetAllPlantsAtCell(row1, col1);
        Plant[] plants2 = GetAllPlantsAtCell(row2, col2);
        TileObject tile1 = TileObjectAt(row1, col1);
        TileObject tile2 = TileObjectAt(row2, col2);

        //swapping tile objects
        SetTile(row1, col1, tile2);
        SetTile(row2, col2, tile1);

        //to swap plants, we need to check if the plants can be placed there
        foreach (Plant plant in plants1)
            



    }*/

    public PlantingResult AttemptMovingPlant(Plant plant, int rowTo, int colTo)
    {
        PlantingResult result = Lanes[rowTo].tiles[colTo].AttemptToPlacePlant(plant);

        if (result == PlantingResult.Success)
        {
            Lanes[plant.row].tiles[plant.col].RemovePlant(plant);
            plant.UpdateTile(rowTo, colTo);
        }
            

        return result;
    }

    public PlantingResult AttemptSwappingPlants(int row1, int col1, int row2, int col2, bool OnlyUnoccupied)
    {
        throw new NotImplementedException();

    }

    public PlantingResult AttemptSwappingTileObjects(int row1, int col1, int row2, int col2, bool OnlyUnoccupied, bool AnyTileType = false)
    {
        if (!AnyTileType)
            if (TileTypeAt(row1, col1) != TileTypeAt(row2, col2))
                return PlantingResult.InvalidTileValues;

        if (OnlyUnoccupied)
            if (TileObjectAt(row2, col2) != TileObject.None)
                return PlantingResult.OccupiedTile;

        SetTile(row1, col1, TileObjectAt(row2, col2));
        SetTile(row2, col2, TileObjectAt(row1, col1));

        return PlantingResult.Success;
    }

    
}
