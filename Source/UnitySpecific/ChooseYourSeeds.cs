using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
public static class ChooseYourSeeds
{
    private static SeedPacketHandler[] hotbarActors;
    private static Dictionary<PlantType, SeedPacketHandler> SelectableSeeds;
    private static bool PlayerWarnedForPlantsChoice = false;
    private static System.Random _random;
    public static void Start(int slots, List<PlantType> unlockedPlants, IReadOnlyList<ZombieType> allowedZombies, System.Random random)
    {
        _random = random;
        if (unlockedPlants.Count <= slots)
        {
            StartWithoutChoice(unlockedPlants);
            return;
        }
        
        GameManager.Instance.MoveCameraToStreet();
        InstantiateHotbarSeeds(slots);
        InstantiateSelectableSeeds(unlockedPlants);
        ShowPreviewZombies(allowedZombies);
        
    }
    static Queue<ZombieActor> previewZombies = new();
    private static void ShowPreviewZombies(IReadOnlyList<ZombieType> allowedZombies)
    {
        WorldBorders world = GameManager.Instance.currentWorld.geometry;
        foreach (ZombieType zt in ZombieWaveManager.getPreviewZombies(10, allowedZombies, _random))
        {
            previewZombies.Enqueue(GameManager.Instance.SpawnZombiePrefab(zt, _random.Next() % world.rowN, world.zombieSpawnCol + (_random.Next() % 2000) / 1000f));
        }
    }

    private static void HidePreviewZombies()
    {
        while (previewZombies.Count > 0)
            GameManager.Instance.RemoveActorPrefab(previewZombies.Dequeue());
    }

    private static void StartWithoutChoice(List<PlantType> unlockedPlants)
    {
        InstantiateHotbarSeeds(unlockedPlants.Count);
        GameManager.Instance.MoveCameraToStreet();  //i know this doesnt make sense right now, but i need to show zombies at one point

        for(int i = 0; i<hotbarActors.Length; i++)
        {
            hotbarActors[i].SetPlant(unlockedPlants[i]);
        }
        
        //since theres no choice, manually begin level, without checks
        StartLevel();
    }
    private static void InstantiateHotbarSeeds(int slots)
    {
        hotbarActors = new SeedPacketHandler[slots];
        for (int i = 0; i < slots; i++)
        {
            hotbarActors[i] = GameManager.Instance.SpawnSeedPacketPrefab_Hotbar(i); //todo: set their positions
            hotbarActors[i].Initialize(PlantType.NullPlant, i);
        }
    }
    private static void InstantiateSelectableSeeds(List<PlantType> unlockedPlants)
    {
        SelectableSeeds = new Dictionary<PlantType, SeedPacketHandler>();
        foreach (PlantType plant in unlockedPlants)
        {
            SelectableSeeds[plant] = GameManager.Instance.SpawnSeedPacketPrefab_AllSeeds(((int)plant-1)); //-1 since NullPlant is not counted for positioning
            SelectableSeeds[plant].Initialize(plant);
        }
    }


    public static void DeselectPlantFromSlot(int hotbarSlot, PlantType plant)
    {
        PlayerWarnedForPlantsChoice = false;
        SetCanStart(false);
        SelectableSeeds[plant].SetChosen(false);

        hotbarActors[hotbarSlot].SetPlant(PlantType.NullPlant);
        ShiftHotbarLeft(hotbarSlot);
    }
    private static void ShiftHotbarLeft(int startIndex = 0)
    {
        for(int i = startIndex; i < hotbarActors.Length-1; i++)
            hotbarActors[i].SetPlant(hotbarActors[i + 1].plant);
        hotbarActors[hotbarActors.Length - 1].SetPlant(PlantType.NullPlant);
    }

    public static void SelectPlant(PlantType plant)
    {
        //full hotbar
        if (hotbarActors.Last().plant != PlantType.NullPlant)
            return;
        SelectableSeeds[plant].SetChosen(true);

        int slot = FindLastEmptySlot();
        hotbarActors[slot].SetPlant(plant);
        
        if (slot == hotbarActors.Length-1)
            SetCanStart(true);
    }
    private static int FindLastEmptySlot()
    {
        for (int i = hotbarActors.Length - 1; i >= 0; i--)
            if (hotbarActors[i].plant != PlantType.NullPlant)
                return i+1;

        return 0;
    }
    public static void SetCanStart(bool value)
    {
        LevelUI.Instance.SetButtonLetsRockEnabled(value);
    }

    //start level
    
    public static void LetsRock()
    {
        PlantType[] plantTypes = hotbarActors.Select(p => p.plant).ToArray();
        bool accepted = GameManager.Instance.AcceptHotbar(plantTypes);
        Debug.Log($"the hotbar has been accepted? {accepted}! and player has been warned? {PlayerWarnedForPlantsChoice}");
        if (!(GameManager.Instance.AcceptHotbar(plantTypes) || PlayerWarnedForPlantsChoice))
        {
            PlayerWarnedForPlantsChoice = true;
            return;
        }

        StartLevel();
    }
    private static void StartLevel()
    {
        RemoveSeedSelection();
        Hotbar.SetPlants(hotbarActors);
        LevelUI.Instance.RemoveLetsRockButton();

        GameManager.Instance.MoveCameraToLawn();
        GameManager.Instance.StartLevel();
        HidePreviewZombies();
    }
    public static string GetHotbarWarning(PlantType[] plants, World world, LevelDefinition level)
    {
        PlantStats plant;
        bool SunProducer = false, Lilypad = false, Pot = false;
        foreach (PlantType plantType in plants)
        {
            plant = PlantRegistry.GetStats(plantType);

            if (plant.HasTag(PlantTag.SunProducer))
                SunProducer = true;

            if (plant.HasTag(PlantTag.GraveBuster))
                if (!world.HasTag(WorldTag.DontWarnOnGraveBuster))
                    return $"You picked {plant.DisplayName} on a level where a {Enum.GetName(typeof(PlantTag), PlantTag.GraveBuster)} is not required";

            if (plant.HasTag(PlantTag.Lilypad))
                if (world.HasTag(WorldTag.WarningOnNoLilypad))
                    Lilypad = true;
                else
                    return $"You picked {plant.DisplayName} on a level where a {Enum.GetName(typeof(PlantTag), PlantTag.Lilypad)} is not required";

            if (plant.HasTag(PlantTag.Pot))
                if (world.HasTag(WorldTag.WarningOnNoPot))
                    Pot = true;
                else
                    return $"You picked {plant.DisplayName} on a level where a {Enum.GetName(typeof(PlantTag), PlantTag.Pot)} is not required";
        }
        if (!SunProducer)
            return "You haven't chosen a Sun producer";
        if (world.HasTag(WorldTag.WarningOnNoLilypad) && !Lilypad)
            return $"You haven't chosen a Lilypad";
        if (world.HasTag(WorldTag.WarningOnNoPot) && !Pot)
            return "You haven't chosen a Pot";

        return null;
    }

    private static void RemoveSeedSelection()
    {
        if (SelectableSeeds == null)
            return;

        foreach(SeedPacketHandler seed in SelectableSeeds.Values)
            GameManager.Instance.RemoveSeedPacketPrefab(seed);

        SelectableSeeds.Clear();
    }
    /*
     * to do:
     * in gamemanager, manage who gets to run StartLevel()
     * create choose your seeds
     *  spawn seed sprites
     *  show zombies in level (maybe later)
     *  make seed sprites clickable
     * 
     */
}
