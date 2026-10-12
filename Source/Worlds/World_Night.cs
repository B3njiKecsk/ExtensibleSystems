using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class World_Night : World
{
    public World_Night(System.Random random) : base(random)
    {
    }
    protected override WorldTag Tags => WorldTag.MushroomsAwake | WorldTag.DontWarnOnGraveBuster;
    public override WorldBorders geometry => new WorldBorders
    {
        cellWidth = 1.275f,
        cellHeight = 1.6f,

        gridStartX = -4.8f,
        gridStartY = 3.6f,
        CellSpritePivotX = 0.675f,
        CellSpritePivotY = -1.6f,

        rowN = 5,
        colN = 9,

        zombieSpawnCol = 10, //gridStartX + (colN) * cellWidth;
    };

    public override string SceneID => "World_Night";

    protected override void StartBehaviour(int levelIndex, LevelManager levelManager)
    {
        int numOfGraves = 0;


        if (levelIndex < 4)
            numOfGraves = 4;
        else if (levelIndex == 4 || levelIndex == 6 || levelIndex == 8)
            numOfGraves = 7;
        else if (levelIndex == 5)
            MiniGame();
        else if (levelIndex < 10)
            numOfGraves = 11;
        else
            numOfGraves = 13;


        while (numOfGraves-- > 0)
            SummonGrave(levelManager);
    }
    private List<(int row, int col)> graves = new();
    private void SummonGrave(LevelManager levelManager)
    {
        graves.Add(levelManager.SetRandomTileInRange(0, geometry.colN / 2, geometry.rowN - 1, geometry.colN - 1, TileObject.Grave));
    }

    private void MiniGame()
    {

    }

    protected override IReadOnlyList<ZombieType>[] GetZombieSpawns()
    {
        return new[]
        {
            new[]
            {
                ZombieType.Browncoat,
                ZombieType.Newspaper,
            },
            new[]
            {
                ZombieType.Browncoat,
                ZombieType.Newspaper,
                ZombieType.Conehead,
                ZombieType.Buckethead,
            },
            new[]
            {
                ZombieType.Browncoat,
                ZombieType.Conehead,
                ZombieType.Screendoor,
            },
            new[]
            {
                ZombieType.Browncoat,
                ZombieType.Conehead,
                ZombieType.Polevaulter,
                ZombieType.Screendoor,
            },
            new[]
            {
                ZombieType.Browncoat,
                ZombieType.Conehead,
                ZombieType.Buckethead,
            },
            new[]
            {
                ZombieType.Browncoat,
                ZombieType.Conehead,
                ZombieType.Football,
            },
            new[]
            {
                ZombieType.Browncoat,
                ZombieType.Conehead,
                ZombieType.Football,
                ZombieType.Screendoor,
            },
            new[]
            {
                ZombieType.Browncoat,
                ZombieType.Conehead,
                ZombieType.Dancing,
            },
            new[]
            {
                ZombieType.Browncoat,
                ZombieType.Conehead,
                ZombieType.Screendoor,
                ZombieType.Dancing,
            },
            new[]
            {
                ZombieType.Browncoat,
                ZombieType.Conehead,
                ZombieType.Screendoor,
                ZombieType.Football,
                ZombieType.Dancing,
            },

        };
    }

    protected override int[] GetWaves()
    {
        return new[]
        {
            10,20,10,20,10,10,20,10,20,20
        };
    }

    protected override IReadOnlyList<(int lane, float xPos)> CalculateAmbushPositions()
    {
        List<(int lane, float xPos)> positions = new();

        foreach (var grave in graves)
            if (GameManager.Instance.levelManager.TileHasObject(TileObject.Grave, grave.row, grave.col))
                positions.Add(grave);

        return positions;
    }

    public override LevelReward GetLevelReward(int levelNumber)
    {
        switch (levelNumber)
        {
            case 1: return new LevelReward { rewardType = RewardType.SeedPacket, rewardPlant = PlantType.Sunshroom };
            case 2: return new LevelReward { rewardType = RewardType.SeedPacket, rewardPlant = PlantType.Fumeshroom };
            case 3: return new LevelReward { rewardType = RewardType.SeedPacket, rewardPlant = PlantType.Gravebuster };

            case 4: return new LevelReward { rewardType = RewardType.Almanac };

            case 5: return new LevelReward { rewardType = RewardType.SeedPacket, rewardPlant = PlantType.Hypnoshroom };
            case 6: return new LevelReward { rewardType = RewardType.SeedPacket, rewardPlant = PlantType.Scaredyshroom };
            case 7: return new LevelReward { rewardType = RewardType.SeedPacket, rewardPlant = PlantType.Iceshroom };
            case 8: return new LevelReward { rewardType = RewardType.SeedPacket, rewardPlant = PlantType.Doomshroom };

            case 9: return new LevelReward { rewardType = RewardType.Note2 };

            case 10: return new LevelReward { rewardType = RewardType.Note3 };
            //case 10: return new LevelReward { rewardType = RewardType.SeedPacket, rewardPlant = PlantType.Lilypad };
        }
        throw new InvalidOperationException();
    }
}
