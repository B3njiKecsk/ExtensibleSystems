using System;
using System.IO;
using UnityEngine;

public class ZombieDefinition
{
    public readonly ZombieStats stats;
    public readonly GameObject prefab;
    private readonly Func<int, Zombie> creator;

    public ZombieDefinition(ZombieStats zombieStats, string zombiePrefabPath, Func<int, Zombie> zombieCreator)
    {
        stats = zombieStats;
        prefab = Resources.Load<GameObject>(zombiePrefabPath);
        if (prefab == null)
            throw new InvalidOperationException($"Failed registering plant with given name '{zombieStats.DisplayName}': prefab at '{zombiePrefabPath}' was not found");
        ZombieActor actor = prefab.GetComponent<ZombieActor>();
        if (actor == null)
            throw new InvalidDataException($"Failed registering plant with given name '{zombieStats.DisplayName}': The prefab at path {zombiePrefabPath} does not contain a {actor.GetType().Name} component");
        creator = zombieCreator;
    }

    public Zombie Create(int row)
    {
        return creator(row);
    }
}
