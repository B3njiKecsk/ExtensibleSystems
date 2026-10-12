using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public static class PlantRegistry
{
    //Since PlantType is an Enum, this could be an array. However, we can afford it due to the amount of plants being only a few hundred at most (52 in pvz1) and it pays off since adding a new plant is harder to mess up, array would need them to be in the same order as in the enum
    private static readonly Dictionary<PlantType, PlantDefinition> registry = new();
    

    public static void RegisterPlants()
    {
        if (registry.Count > 0)
            throw new InvalidOperationException($"{nameof(PlantRegistry)} has already been initialized.");


        registry[PlantType.Peashooter] = new PlantDefinition(
            Peashooter.stats,
            GameManager.PlantPrefabsPath + "Peashooter",
            (int row, int col) => new Peashooter(row, col));

        registry[PlantType.Sunflower] = new PlantDefinition(
            Sunflower.stats,
            GameManager.PlantPrefabsPath + "Sunflower",
            (int row, int col) => new Sunflower(row, col));

        registry[PlantType.CherryBomb] = new PlantDefinition(
            CherryBomb.stats,
            GameManager.PlantPrefabsPath + "CherryBomb",
            (int row, int col) => new CherryBomb(row, col));

        registry[PlantType.Wallnut] = new PlantDefinition(
            Wallnut.stats,
            GameManager.PlantPrefabsPath + "Wallnut",
            (int row, int col) => new Wallnut(row, col));

        registry[PlantType.PotatoMine] = new PlantDefinition(
            PotatoMine.stats,
            GameManager.PlantPrefabsPath + "PotatoMine",
            (int row, int col) => new PotatoMine(row, col));

        registry[PlantType.Snowpea] = new PlantDefinition(
            Snowpea.stats,
            GameManager.PlantPrefabsPath + "Snowpea",
            (int row, int col) => new Snowpea(row, col));

        registry[PlantType.Chomper] = new PlantDefinition(
            Chomper.stats,
            GameManager.PlantPrefabsPath + "Chomper",
            (int row, int col) => new Chomper(row, col));

        registry[PlantType.Repeater] = new PlantDefinition(
            Repeater.stats,
            GameManager.PlantPrefabsPath + "Repeater",
            (int row, int col) => new Repeater(row, col));

        registry[PlantType.Puffshroom] = new PlantDefinition(
            Puffshroom.stats,
            GameManager.PlantPrefabsPath + "Puffshroom",
            (int row, int col) => new Puffshroom(row, col));

        registry[PlantType.Sunshroom] = new PlantDefinition(
            Sunshroom.stats,
            GameManager.PlantPrefabsPath + "Sunshroom",
            (int row, int col) => new Sunshroom(row, col));

        registry[PlantType.Fumeshroom] = new PlantDefinition(
            Fumeshroom.stats,
            GameManager.PlantPrefabsPath + "Fumeshroom",
            (int row, int col) => new Fumeshroom(row, col));

        registry[PlantType.Gravebuster] = new PlantDefinition(
            Gravebuster.stats,
            GameManager.PlantPrefabsPath + "Gravebuster",
            (int row, int col) => new Gravebuster(row, col));
        
        registry[PlantType.Hypnoshroom] = new PlantDefinition(
            Hypnoshroom.stats,
            GameManager.PlantPrefabsPath + "Hypnoshroom",
            (int row, int col) => new Hypnoshroom(row, col));
        
        registry[PlantType.Scaredyshroom] = new PlantDefinition(
            Scaredyshroom.stats,
            GameManager.PlantPrefabsPath + "Scaredyshroom",
            (int row, int col) => new Scaredyshroom(row, col));
        
        registry[PlantType.Iceshroom] = new PlantDefinition(
            Iceshroom.stats,
            GameManager.PlantPrefabsPath + "Iceshroom",
            (int row, int col) => new Iceshroom(row, col));
        
        registry[PlantType.Doomshroom] = new PlantDefinition(
            Doomshroom.stats,
            GameManager.PlantPrefabsPath + "Doomshroom",
            (int row, int col) => new Doomshroom(row, col));
    }

    private static PlantDefinition GetDefinition(PlantType plantType)
    {
        if (plantType == PlantType.NullPlant)
            throw new ArgumentException($"PlantRegistry cannot operate on a {PlantType.NullPlant}");
        return registry[plantType];
    }

    public static Plant Create(PlantType type, int row, int col)
    {
        return GetDefinition(type).Create(row, col);
    }
    public static GameObject GetGameObject(PlantType type)
    {
        return GetDefinition(type).prefab;
    }
    public static Sprite GetSeedSprite(PlantType type)
    {
        return GetDefinition(type).seedPacket;
    }
    public static PlantStats GetStats(PlantType type)
    {
        return GetDefinition(type).stats;
    }
    public static Sprite GetMainSprite(PlantType type)
    {
        return GetDefinition(type).mainSprite;
    }

    public static PlantType[] GetAllPlantTypes()
    {
        return ((PlantType[])Enum.GetValues(typeof(PlantType))) //gives an array from the PlantType Enum
            .Where(p => p != PlantType.NullPlant)               //excludes NullPlant, since its not a plant
            .ToArray();
    }
}
