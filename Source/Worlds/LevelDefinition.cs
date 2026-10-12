using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class LevelDefinition
{
    const int randomTileSetLimit = 20;
    
    public TileType[,] tiles { private set; get; }
    System.Random random;
    public LevelReward reward { private set; get; }

    public LevelDefinition(int rows, int cols, System.Random random, LevelReward levelReward)
    { 
        tiles = new TileType[rows, cols];
        this.random = random;
        reward = levelReward;
    }
    public void SetTile(int row, int col, TileType type)
    {
        tiles[row, col] = type;
    }

    #region Zombie Spawn

    public int Waves;
    public IReadOnlyList<ZombieType> AllowedZombies { get; private set; }
    public IReadOnlyList<(int wave, List<ZombieType> zombies)> ScriptedWaves { get; private set; }

    public void SetLevelWaves(int waveCount, IReadOnlyList<ZombieType> allowedZombies, IReadOnlyList<(int wave, List<ZombieType> zombies)> scriptedWaves = null)
    {
        Waves = waveCount;
        AllowedZombies = allowedZombies;
        ScriptedWaves = scriptedWaves;
    }

    #endregion
}
