using System;
using System.Linq;
using UnityEngine;

public class Tumbleweed : Plant
{
    public Tumbleweed(int row, int col, PlantStats stats) : base(row, col, stats)
    {
    }
    //tag canplaceonplant
    public override bool CanPlantHere(TileType type, Plant basePlant, Plant mainPlant, Plant overlayPlant, TileObject tileObject)
    {
        if (tileObject == TileObject.DirtPile)
            return true;
        return base.CanPlantHere(type, basePlant, mainPlant, overlayPlant, tileObject);
    }

    protected override void OnSecondClick(int row, int col)
    {
        Plant topPlant = null;
        foreach (Plant plant in PlantsOnTile())
        {
            if (plant == this)
                continue;
            
            topPlant = plant;
        }

        //If there are plants, move THAT
        if (topPlant == null)
        {
            if (topPlant.AttemptMovingPlant(row, col) == PlantingResult.Success)
                MoveNRoll(row, col);
        }
        //if there arent plants, 
        else if (TileObjectAt(this.row, this.col) != TileObject.None)
        {
            if (AttemptMovingTileObjects(this.row, this.col, row, col) == PlantingResult.Success)
                MoveNRoll(row, col);
        }
        else
            MoveNRoll(row, col);
    }
    private void MoveNRoll(int row, int col)
    {
        AttemptMovingPlant(row, col);
    }
    
}
