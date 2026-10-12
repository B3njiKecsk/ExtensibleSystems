using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ActionState
{
    Default = 0,
    Shovel = -1
}

public class MouseInputHandler : MonoBehaviour
{
    public new Camera camera;
    private static WorldBorders worldBorders;
    private LevelManager levelManager;
    private int debugFunction;
    //meant to rappresent selected tool as well: positive numbers is plant in hotbar, 0 is nothing selected, negative numbers is tools -1 being shovel
    public static MouseInputHandler instance;
    Plant selectedPlayingPlant = null;

    void Start()
    {
        instance = this;
        levelManager = GameManager.Instance.levelManager;
        setupClickAreas();
        worldBorders = GameManager.Instance.currentWorld.geometry;
    }

    void setupClickAreas()
    {
        
    }
    public void SelectPlant(InputAction.CallbackContext context)//, int hotbarSlot)
    {
        if (context.phase != InputActionPhase.Performed) return;
        
        debugFunction = Int32.Parse(context.control.name);
    }

    private void SelectTool(ActionState toolID)
    {
        Hotbar.SelectPlant((int) toolID);
    }

    public void onClick(InputAction.CallbackContext context)
    {
        if(levelManager == null)
            if(GameManager.Instance.levelManager == null)
                return; //not playing yet
            else
                levelManager = GameManager.Instance.levelManager;
            
        if (context.phase != InputActionPhase.Performed) return;

        Vector3 MousePos = Mouse.current.position.ReadValue();
        Vector3 WorldPos = camera.ScreenToWorldPoint(MousePos);
        (int x, int y) Cell = GameManager.Instance.currentWorld.WorldPosToTileCoords(WorldPos.x, WorldPos.y);

        if (WorldPos.x > -4.80f && WorldPos.y < 3.6f && WorldPos.y > -4.5f && WorldPos.x < 6.675f)
            if (Hotbar.selectedPlant > 0)
                PlaceSelectedPlant(Cell.x, Cell.y);
            else if (Hotbar.selectedPlant < 0)
                UseToolOnCell(Cell);
            else if (selectedPlayingPlant != null)
            {
                selectedPlayingPlant.TileClick(Cell.y, Cell.x);
            }
            else //if nothing is selected
            {
                Plant p = GameManager.Instance.levelManager.GetTopPlantAtCell(Cell.y, Cell.x);
                if (p != null)
                    if (p.Click())
                        selectedPlayingPlant = p;
                ExecuteDebugFunction(WorldPos);
                Hotbar.DeselectPlant();
                return;
            }
        Hotbar.DeselectPlant();
        selectedPlayingPlant = null;
    }
    private void ExecuteDebugFunction(Vector3 WorldPos)
    {
        (int, int) Cell = GameManager.Instance.currentWorld.WorldPosToTileCoords(WorldPos.x, WorldPos.y);
        int xCell = Cell.Item1;
        int yCell = Cell.Item2;

        //testing zombie spawning before waves exist
        if (debugFunction == 1)
        {
            levelManager.SpawnZombie(ZombieType.Newspaper, yCell);
            debugFunction = 0;
            return;
        }
        if (debugFunction == 2)
        {
            levelManager.SpawnZombie(ZombieType.Conehead, yCell);
            debugFunction = 0;
            return;
        }
        if (debugFunction == 3)
        {
            levelManager.SpawnZombie(ZombieType.Buckethead, yCell);
            debugFunction = 0;
            return;
        }
        if (debugFunction == 4)
        {
            levelManager.SpawnZombie(ZombieType.Polevaulter, yCell);
            debugFunction = 0;
            return;
        }

        if (debugFunction == 6)
        {
            Debug.Log("next wave");
            levelManager._DBG_NextWave();
            debugFunction = 0;
            return;
        }
        //test coin spawn
        if (debugFunction == 7)
        {
            Debug.Log("summoning coins");
            levelManager.SpawnCoins(100, xCell + 0.5f, yCell + 0.5f);
            debugFunction = 0;
            return;
        }
        //testing sun falling from the sky
        if (debugFunction == 8)
        {
            levelManager.SpawnFallingSun(25, xCell + 0.5f, yCell + 0.5f);
            debugFunction = 0;
            return;
        }
        //test level advance
        if (debugFunction == 9)
        {
            GameManager.Instance.levelManager.DisplayReward(GameManager.Instance.currentWorld.GetLevelReward(GameManager.Instance.currentLevelIndex+1));
            debugFunction = 0;
            return;
        }
    }
    private void UseToolOnCell((int x, int y) cell)
    {
        switch ((ActionState)Hotbar.selectedPlant)
        {
            case ActionState.Shovel:
                UseShovel(cell.x, cell.y);
                return;
            default:
                return;
        }
    }
    private void UseShovel(int cellX, int cellY)
    {
        levelManager.ShovelUpPlant(cellX, cellY);
    }
    private void PlaceSelectedPlant(int xCell, int yCell)
    {
        PlantType selectedPlantType = Hotbar.GetSelectedPlant();
        
        PlantingResult result = levelManager.PlacePlant(yCell, xCell, selectedPlantType);
        if (result == PlantingResult.Success)
            Hotbar.NotifyPlanted(Hotbar.selectedPlant);
        Debug.Log("planting " + result + "; " + (levelManager.GetTopPlantAtCell(yCell, xCell)?.Stats.DisplayName ?? "Empty"));

    }
}