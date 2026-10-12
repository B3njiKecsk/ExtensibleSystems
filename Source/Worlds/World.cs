using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public struct WorldBorders
{
    public float cellWidth;
    public float cellHeight;

    public float gridStartY;
    public float gridStartX;
    public float CellSpritePivotY;
    public float CellSpritePivotX;

    public int zombieSpawnCol;

    public int rowN;
    public int colN;
}
[Flags]
public enum WorldTag
{
    None = 0,
    MushroomsAwake = 1 << 0,
    DontWarnOnGraveBuster = 1 << 1,
    WarningOnNoLilypad = 1 << 2,
    WarningOnNoPot = 1 << 3,
}
public abstract class World
{
    protected System.Random random;
    readonly protected int levelCount;
    public World(System.Random random, int numOfLevels = 10)
    {
        this.random = random;
        levelCount = numOfLevels;
        InitiateLevels(numOfLevels);
    }
    protected virtual WorldTag Tags => WorldTag.None;
    public abstract WorldBorders geometry { get; }
    public abstract string SceneID { get; }
    protected LevelDefinition[] levels { private set; get; }

    private void InitiateLevels(int numOfLevels)
    {
        levels = new LevelDefinition[numOfLevels];
        for (int i = 0; i < numOfLevels; i++)
        {
            levels[i] = new LevelDefinition(geometry.rowN, geometry.colN, random, GetLevelReward(i+1));
        }

        IReadOnlyList<ZombieType>[] zombieSpawns = GetZombieSpawns();
        int[] waves = GetWaves();
        var scriptedWaves = GetScriptedWaves();

        if (zombieSpawns == null || zombieSpawns.Length != levels.Length)
            throw new InvalidOperationException($"the length of the array returned in {nameof(GetZombieSpawns)} was different than the level count for the world {this.GetType().Name}");
        if (waves == null || waves.Length != levels.Length)
            throw new InvalidOperationException($"the length of the array returned in {nameof(GetWaves)} was different than the level count for the world {this.GetType().Name}");

        for (int i = 0; i < levels.Length; i++)
            levels[i].SetLevelWaves(waves[i], zombieSpawns[i], scriptedWaves);
    }
    public LevelDefinition GetLevel(int levelNumber)
    {
        if (levelNumber >= levels.Length)
            return null;
        return levels[levelNumber];
    }

    public LaneType GetLaneType(int level, int lane)
    {
        return DefineLaneTypeForLevel(level, lane);
    }

    protected virtual LaneType DefineLaneTypeForLevel(int level, int lane)
    {
        return LaneType.Normal;
    }

    public abstract LevelReward GetLevelReward(int levelNumber);

    public (int, int) WorldPosToTileCoords(float xpos, float ypos)
    {
        float xCell = (xpos - geometry.gridStartX) / geometry.cellWidth;
        float yCell = -(ypos - geometry.gridStartY) / geometry.cellHeight;

        return ((int) xCell, (int) yCell);
    }

    public Vector3 TileCoordsToWorldPos(int xCell, int yCell)
    {
        float xPos = (xCell * geometry.cellWidth) + geometry.gridStartX + geometry.CellSpritePivotX;
        float yPos = (yCell * -geometry.cellHeight) + geometry.gridStartY + geometry.CellSpritePivotY;


        return new Vector3(xPos, yPos, 0);
    }

    public Vector3 TileCoordsToWorldPos(float xPos, float yPos)
    {
        float xPosOut = (xPos * geometry.cellWidth) + geometry.gridStartX;
        float yPosOut = (yPos * -geometry.cellHeight) + geometry.gridStartY;

        return new Vector3(xPosOut, yPosOut, 0);
    }

    protected virtual void UpdateBehaviour(float deltaTime)
    {

    }
    public void Update(float deltaTime)
    {
        UpdateBehaviour(deltaTime);
    }
    public bool HasTag(WorldTag tag)
    {
        return (Tags & tag) != 0;
    }

    protected void DropSunFromSky(int fallingSunAmount)
    {
        GameManager.Instance.levelManager.SpawnFallingSun(
            fallingSunAmount,
            (float)(random.NextDouble() * geometry.colN),
            (float)(random.NextDouble() * geometry.rowN));
    }

    protected virtual void StartBehaviour(int levelIndex, LevelManager levelManager)
    {
        
    }
    public void StartLevel(int levelIndex, LevelManager levelManager)
    {
        StartBehaviour(levelIndex, levelManager);
    }
    protected abstract IReadOnlyList<ZombieType>[] GetZombieSpawns();
    protected abstract int[] GetWaves();
    protected virtual IReadOnlyList<(int wave, List<ZombieType> zombies)> GetScriptedWaves()
    {
        return null;
    }

    public IReadOnlyList<(int lane, float xPos)> GetAmbushPositions()
    {
        return CalculateAmbushPositions();
    }
    protected virtual IReadOnlyList<(int lane, float xPos)> CalculateAmbushPositions()
    {
        return new List<(int lane, float xPos)>();
    }

    protected virtual void StartAmbushAnimationOverride(ZombieActor actor)
    {
        actor.RiseAnimation();
    }
    public void StartAmbushAnimation(ZombieActor actor)
    {
        StartAmbushAnimationOverride(actor);
    }
    protected virtual List<ZombieType> CalculateAmbushZombies()
    {
        return new List<ZombieType>
        {
            ZombieType.Browncoat,
            ZombieType.Conehead,
            ZombieType.Buckethead,
        };
    }
    public List<ZombieType> GetAmbushZombies()
    {
        return CalculateAmbushZombies();
    }
}
