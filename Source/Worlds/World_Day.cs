using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public sealed class World_Day : World
{
    public World_Day(System.Random random) : base(random)
    {
    }
    //protected override WorldTag Tags => WorldTag.Night | WorldTag.Pool;
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

    public override string SceneID => "World_Day";

    float sunFallingTimer = 0;
    const float timeToSpawnSun = 8f;
    const int fallingSunAmount = 25;

    protected override LaneType DefineLaneTypeForLevel(int level, int lane)
    {
        //after third level, all lanes are normal
        if (level > 3)
            return LaneType.Normal;

        //before third level...
        //center lane always normal
        if (lane == 2)
            return LaneType.Normal;

        //3 lanes are also normal, but only on levels 2 and 3
        if (level != 1)
            if (lane == 1 || lane == 3)
                return LaneType.Normal;

        return LaneType.Dirt;
    }

    protected override void UpdateBehaviour(float deltaTime)
    {
        sunFallingTimer += deltaTime;
        if (sunFallingTimer >= timeToSpawnSun)
        {
            DropSunFromSky(fallingSunAmount);
            sunFallingTimer = 0;
        }
    }

    protected override IReadOnlyList<ZombieType>[] GetZombieSpawns()
    {
        return new[]
        {
            new[]
            {
                ZombieType.Browncoat,
            },
            new[]
            {
                ZombieType.Browncoat,
                //ZombieType.Flag,
            },
            new[]
            {
                ZombieType.Browncoat,
                //ZombieType.Flag,
                ZombieType.Conehead,
            },
            new[]
            {
                ZombieType.Browncoat,
                //ZombieType.Flag,
                ZombieType.Conehead,
            },
            new[]
            {
                ZombieType.Browncoat,
                //ZombieType.Flag,
                ZombieType.Conehead,
            },
            new[]
            {
                ZombieType.Browncoat,
                //ZombieType.Flag,
                ZombieType.Conehead,
                ZombieType.Polevaulter,
            },
            new[]
            {
                ZombieType.Browncoat,
                //ZombieType.Flag,
                ZombieType.Conehead,
                ZombieType.Polevaulter,
            },
            new[]
            {
                ZombieType.Browncoat,
                //ZombieType.Flag,
                ZombieType.Conehead,
                ZombieType.Buckethead,
            },
            new[]
            {
                ZombieType.Browncoat,
                //ZombieType.Flag,
                ZombieType.Conehead,
                ZombieType.Polevaulter,
                ZombieType.Buckethead,
            },
            new[]
            {
                ZombieType.Browncoat,
                //ZombieType.Flag,
                ZombieType.Conehead,
                ZombieType.Polevaulter,
                ZombieType.Buckethead,
            },
        };
    }

    protected override int[] GetWaves()
    {
        return new[]
        {
            4,6,8,10,8,10,20,10,20,20
        };
    }

    public override LevelReward GetLevelReward(int levelNumber)
    {
        switch (levelNumber)
        {
            case 1: return new LevelReward { rewardType = RewardType.SeedPacket, rewardPlant = PlantType.Sunflower };
            case 2: return new LevelReward { rewardType = RewardType.SeedPacket, rewardPlant = PlantType.CherryBomb };
            case 3: return new LevelReward { rewardType = RewardType.SeedPacket, rewardPlant = PlantType.Wallnut };

            case 4: return new LevelReward { rewardType = RewardType.Shovel };

            case 5: return new LevelReward { rewardType = RewardType.SeedPacket, rewardPlant = PlantType.PotatoMine };
            case 6: return new LevelReward { rewardType = RewardType.SeedPacket, rewardPlant = PlantType.Snowpea };
            case 7: return new LevelReward { rewardType = RewardType.SeedPacket, rewardPlant = PlantType.Chomper };
            case 8: return new LevelReward { rewardType = RewardType.SeedPacket, rewardPlant = PlantType.Repeater };

            case 9: return new LevelReward { rewardType = RewardType.Note1 };

            case 10: return new LevelReward { rewardType = RewardType.SeedPacket, rewardPlant = PlantType.Puffshroom };
        }
        throw new InvalidOperationException();
    }
}
