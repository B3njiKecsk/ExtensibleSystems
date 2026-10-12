using System;
using System.IO;
using UnityEngine;

public class PlantDefinition
{

    public readonly PlantStats stats;
    public readonly Sprite seedPacket;
    public readonly Sprite mainSprite;
    public readonly GameObject prefab;
    private readonly Func<int, int, Plant> creator;

    public PlantDefinition(PlantStats plantStats, string plantPrefabPath, Func<int, int, Plant> plantCreator)
    {
        stats = plantStats;
        prefab = Resources.Load<GameObject>(plantPrefabPath);
        if (prefab == null)
            throw new InvalidOperationException($"Failed registering plant with given name '{plantStats.DisplayName}': prefab at '{plantPrefabPath}' was not found");
        PlantActor actor = prefab.GetComponent<PlantActor>();
        if (actor == null)
            throw new InvalidDataException($"Failed registering plant with given name '{plantStats.DisplayName}': The prefab at path {plantPrefabPath} does not contain a {actor.GetType().Name} component");
        seedPacket = actor.GetSeedPacketSprite();
        mainSprite = actor.GetMainSprite();
        creator = plantCreator;
    }

    public Plant Create(int row, int col)
    {
        return creator(row, col);
    }
}