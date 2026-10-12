using System;
using System.Linq;
using UnityEngine;

public static class Hotbar 
{
    static SeedPacketHandler[] slots;
    public static int selectedPlant {  get; private set; }

    public static PlantType PlantInSlot(int slot)
    {
        if (slot < 1 || slot > slots.Length+1)
        {
            Debug.LogError("Tried selecting plant in slot " + slot + "; Should be an integer in range [1:" + slots.Length+1 + "]");
            return PlantType.NullPlant;
        }
        return slots[slot - 1].plant;
    }

    public static void SetPlants(SeedPacketHandler[] plants)
    {
        slots = plants;
        foreach (var p in slots)
            p.StartGame();
    }

    public static void SelectPlant(int hotbarSlot)
    {
        selectedPlant = hotbarSlot;
    }
    public static PlantType GetSelectedPlant()
    {
        return PlantInSlot(selectedPlant);
    }
    public static void DeselectPlant()
    {
        if (selectedPlant > 0)
            slots[selectedPlant - 1].DeselectPlantFromHotbar();
        selectedPlant = 0;
    }

    public static void UpdateSlots(float deltaTime)
    {
        foreach (var slot in slots)
            slot.PassTime(deltaTime);
    }
    public static void UpdateSunCount(int value)
    {
        foreach (var slot in slots)
            slot.UpdateSunAvailable(value);
    }
    public static void NotifyPlanted(int hotbarSlot)
    {
        slots[hotbarSlot-1].Planted();
    }
}
