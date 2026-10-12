using UnityEngine;

public class Gravebuster : Plant
{
    public Gravebuster(int row, int col) : base(row, col, stats)
    {
    }

    public static PlantStats stats = new PlantStats(
        displayName: "Grave Buster",
        maxHP: Toughness.Normal,
        sunCost: 75,
        rechargeTime: Recharge.Fast,
        timeBetweenTicks: 5f,
        tags: PlantTag.GraveBuster
        );

    protected override void BehaviourPerTick()
    {
        RemoveTileObject(row, col);
        SummonCoin((int)CoinType.GoldCoin);
        Suicide();
    }

    public override bool CanPlantHere(TileType type, Plant basePlant, Plant mainPlant, Plant overlayPlant, TileObject tileObject)
    {
        if (tileObject == TileObject.Grave)
            return true;
        return false;
    }
}
