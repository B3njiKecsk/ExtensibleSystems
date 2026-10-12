using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ZombieRegistry
{
    //Since ZombieType is an Enum, this could be an array. However, we can afford it due to the amount of zombies being only a few hundred at most and it pays off since adding a new zombie is harder to mess up, array would need them to be in the same order as in the enum
    private static readonly Dictionary<ZombieType, ZombieDefinition> registry = new();
    
    public static void RegisterZombies()
    {
        if (registry.Count > 0)
            throw new InvalidOperationException($"{nameof(ZombieRegistry)} has already been initialized.");


        registry[ZombieType.Browncoat] = new ZombieDefinition(
            Browncoat.stats,
            GameManager.ZombiePrefabsPath + "Browncoat",
            (int row) => new Browncoat(row));

        registry[ZombieType.Flag] = new ZombieDefinition(
            FlagZombie.stats,
            GameManager.ZombiePrefabsPath + "Flag",
            (int row) => new FlagZombie(row));

        registry[ZombieType.Conehead] = new ZombieDefinition(
            Conehead.stats,
            GameManager.ZombiePrefabsPath + "Conehead",
            (int row) => new Conehead(row));

        registry[ZombieType.Buckethead] = new ZombieDefinition(
            Buckethead.stats,
            GameManager.ZombiePrefabsPath + "Buckethead",
            (int row) => new Buckethead(row));

        registry[ZombieType.Polevaulter] = new ZombieDefinition(
            Polevaulter.stats,
            GameManager.ZombiePrefabsPath + "Polevaulter",
            (int row) => new Polevaulter(row));

        registry[ZombieType.Newspaper] = new ZombieDefinition(
            NewspaperZombie.stats,
            GameManager.ZombiePrefabsPath + "Newspaper",
            (int row) => new NewspaperZombie(row));

        registry[ZombieType.Screendoor] = new ZombieDefinition(
            Screendoor.stats,
            GameManager.ZombiePrefabsPath + "Screendoor",
            (int row) => new Screendoor(row));

        registry[ZombieType.Football] = new ZombieDefinition(
            FootballZombie.stats,
            GameManager.ZombiePrefabsPath + "Football",
            (int row) => new FootballZombie(row));

        registry[ZombieType.Dancing] = new ZombieDefinition(
            Dancing.stats,
            GameManager.ZombiePrefabsPath + "Dancing",
            (int row) => new Dancing(row));

        registry[ZombieType.BackupDancer] = new ZombieDefinition(
            BackupDancer.stats,
            GameManager.ZombiePrefabsPath + "BackupDancer",
            (int row) => new BackupDancer(row));
    }

    private static ZombieDefinition GetDefinition(ZombieType zombieType)
    {
        return registry[zombieType];
    }

    public static Zombie Create(ZombieType type, int row)
    {
        return GetDefinition(type).Create(row);
    }
    public static GameObject GetGameObject(ZombieType type)
    {
        return GetDefinition(type).prefab;
    }
    public static ZombieStats GetStats(ZombieType type)
    {
        return GetDefinition(type).stats;
    }
    public static int GetWavePoints(ZombieType type)
    {
        return GetDefinition(type).stats.WavePointCost;
    }
    public static int GetWaveWeight(ZombieType type)
    {
        return GetDefinition(type).stats.WaveWeight;
    }

    public static ZombieType[] GetAllZombieTypes()
    {
        return ((ZombieType[])Enum.GetValues(typeof(ZombieType))) //gives an array from the PlantType Enum
            .Where(p => p != ZombieType.NullZombie)               //excludes NullPlant, since its not a plant
            .ToArray();
    }

    public static bool CanSpawnOnLane(ZombieType z, LaneType type)
    {
        return GetDefinition(z).stats.CanSpawnOnLane(type);
    }
}
