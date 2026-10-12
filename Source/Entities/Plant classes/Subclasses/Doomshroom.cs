using UnityEngine;

public class Doomshroom : Mushroom
{
    public Doomshroom(int row, int col) : base(row, col, stats)
    {

    }

    public static readonly PlantStats stats = new PlantStats(
        displayName: "Doom-shroom",
        sunCost: 125,
        tags: PlantTag.Mushroom | PlantTag.Instant,
        attackDamage: 1800,
        maxHP: 300,
        rechargeTime: Recharge.VerySlow,
        timeBetweenTicks: 1f
        );

    protected override void BehaviourPerTick()
    {
        foreach (Plant plant in PlantsOnTile())
            DamagePlant(plant);

        ModifyTileObject(row, col, TileObject.Crater);

        foreach(Zombie z in ZombiesInRadius(3.5f))
            DamageZombie(z);

        Suicide();
    }
}
