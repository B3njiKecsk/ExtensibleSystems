using System;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Normal = 0, //default value
    Dirt,
    Water,
    Roof,
}
public enum TileObject
{
    None = 0,
    Grave,
    DirtPile,
    Crater,
}
public class TileData
{
    private readonly Plant[] allPlants = new Plant[3];
    public Plant[] AllPlants => allPlants;

    private Plant plant 
    { 
        get => allPlants[1];
        set => allPlants[1] = value;
    }
    private Plant secondary_plant
    {
        get => allPlants[2];
        set => allPlants[2] = value;
    }
    private Plant base_plant
    {
        get => allPlants[0];
        set => allPlants[0] = value;
    }


    public List<Zombie> zombies { get; private set; } = new List<Zombie>();
    //plants subscribing for zombie movement
    private readonly Dictionary<Plant, (Action<int, int> zombieEntered, Action<int, int> zombieLeft)> subscribedPlants = new();
    private readonly Queue<Plant> plantsToUnsubscribe = new();
    public TileType tileType { private set; get; }
    public TileObject tileObject { private set; get; }
    private TileActor instanceActor;
    private readonly int row, col;
    public TileData(int row, int col, TileType tileType = TileType.Normal, TileObject tileObject = TileObject.None)
    {
        this.row = row;
        this.col = col;
        this.tileType = tileType;
        this.tileObject = tileObject;
    }
    public void ReplaceTileObject(TileObject type, TileActor newActor)
    {
        tileObject = type;
        instanceActor?.Remove();
        instanceActor = newActor;
    }
    public void RemoveTileObject()
    {
        ReplaceTileObject(TileObject.None, null);
    }
    public PlantingResult AttemptToPlacePlant(Plant plant2place)
    {
        PlantingResult result = CanPlacePlant(plant2place);

        if (result == PlantingResult.Success)
            ApplyPlant(plant2place);

        return result;
    }
    public void PlacePlantForced(Plant plant2place)
    {
        ApplyPlant(plant2place);
    }

    public PlantingResult CanPlacePlant(Plant plant2place)
    {
        if (plant2place == null)
            return PlantingResult.InvalidPlant;

        PlantingResult result = CanIPlacePlantHere(plant2place);
        if (result != PlantingResult.Success)
            return result;

        return PlantingResult.Success;
    }

    private PlantingResult CanIPlacePlantHere(Plant plant2place)
    {
        //in case a plant is already occupying the spots, code 1 rappresents an occupied spot
        if (plant2place.Stats.HasTag(PlantTag.Base) && base_plant != null) return PlantingResult.OccupiedTile;  //if theres already a Base plant(lilypad, pot, ...)
        else if (plant2place.Stats.HasTag(PlantTag.Secondary) && secondary_plant != null) return PlantingResult.OccupiedTile; //if theres already a secondary plant (pumpkin, 
        else if (!plant2place.Stats.HasTag(PlantTag.CanPlaceOnPlant) && !plant2place.Stats.HasTag(PlantTag.Upgrade) && plant != null) return PlantingResult.OccupiedTile; //if theres already a base plant, and the plant isnt an upgrade, and if its not placeable on any plant

        //check tile types
        if (!plant2place.CanPlantHere(tileType, base_plant, plant, secondary_plant, tileObject))
            return PlantingResult.InvalidTileValues;

        return PlantingResult.Success;
    }
    private PlantingResult CanIPlacePlantHere(PlantType plant2place)
    {
        PlantStats plantStats = PlantRegistry.GetStats(plant2place);
        //in case a plant is already occupying the spots, code 1 rappresents an occupied spot
        if (plantStats.HasTag(PlantTag.Base) && base_plant != null) return PlantingResult.OccupiedTile;  //if theres already a Base plant(lilypad, pot, ...)
        else if (plantStats.HasTag(PlantTag.Secondary) && secondary_plant != null) return PlantingResult.OccupiedTile; //if theres already a secondary plant (pumpkin, 
        else if (!plantStats.HasTag(PlantTag.CanPlaceOnPlant) && !plantStats.HasTag(PlantTag.Upgrade) && plant != null) return PlantingResult.OccupiedTile; //if theres already a base plant, and the plant isnt an upgrade, and if its not placeable on any plant

        //this is why i cant make this a planttype instead of a plant
        /*/check tile types
        if (!plant2place.CanPlantHere(tileType, base_plant, plant, secondary_plant))
            return PlantingResult.IncorrectTerrain;*/



        return PlantingResult.Success;
    }

    private void ApplyPlant(Plant plant2place)
    {
        if (plant2place.Stats.HasTag(PlantTag.Base)) base_plant = plant2place;
        else if (plant2place.Stats.HasTag(PlantTag.Secondary)) secondary_plant = plant2place;
        else plant = plant2place;
    }

    public void RemovePlant(Plant plant2Rm)
    {
        if (plant2Rm == secondary_plant)
            secondary_plant = null;
        else if (plant2Rm == plant)
            plant = null;
        else if (plant2Rm == base_plant)
            base_plant = null;
        else 
            Debug.LogWarning($"Tried removing plant {plant2Rm.Stats.DisplayName} from Tile that doesnt own any it");
    }

    public Plant GetTopPlant()
    {
        if (secondary_plant != null)
            return secondary_plant;
        if (plant != null)
            return plant;
        
        return base_plant;
    }
    /// <summary>
    /// calls the Update() function for every plant on this tile, which in term updates the plants behaviour based on how much time passed between frames
    /// </summary>
    /// <param name="deltaTime"></param>
    public void UpdatePlants(float deltaTime)
    {
        if (base_plant != null)
            base_plant.Update(deltaTime);
        if (plant != null)
            plant.Update(deltaTime);
        if (secondary_plant != null)
            secondary_plant.Update(deltaTime);
    }

    public void ZombieEntersTile(Zombie zombie)
    {
        zombies.Add(zombie);

        foreach (var subscription in subscribedPlants.Values)
            subscription.zombieEntered(row, col);

        while(plantsToUnsubscribe.Count > 0)
            UnsubscribePlantForZombieMovement(plantsToUnsubscribe.Dequeue());
    }

    public void ZombieLeavesTile(Zombie zombie)
    {
        if (!zombies.Contains(zombie))
            Debug.LogError($"Something went completely wrong...\nCalled ZombieLeavesTile with a zombie that isnt on the tile\nZombie: {zombie.Stats.DisplayName}; x:{zombie.xPos}; lane: {zombie.row}; trying to be removed from tile at (row, col): ({row},{col})");

        zombies.Remove(zombie);


        foreach (var subscription in subscribedPlants.Values)
            subscription.zombieLeft(row, col);

        while (plantsToUnsubscribe.Count > 0)
            UnsubscribePlantForZombieMovement(plantsToUnsubscribe.Dequeue());
    }

    public Action SubscribePlantForZombieMovement(Plant p, Action<int, int> enteredTile, Action<int, int> leftTile)
    {
        subscribedPlants[p] = (enteredTile, leftTile);
        return () => QueuePlantForUnsubscribe(p);
    }
    private void QueuePlantForUnsubscribe(Plant plant)
    {
        plantsToUnsubscribe.Enqueue(plant);
    }
    private void UnsubscribePlantForZombieMovement(Plant plant)
    {
        subscribedPlants.Remove(plant);
    }
}
