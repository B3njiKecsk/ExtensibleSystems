using UnityEngine;

public class Iceshroom : Mushroom
{
    public Iceshroom(int row, int col) : base(row, col, stats)
    {
    }

    public static readonly PlantStats stats = new PlantStats(
        displayName: "Ice-shroom",
        maxHP: Toughness.Normal,
        attackDamage: 20,
        timeBetweenTicks: 1f,
        sunCost: 75,
        rechargeTime: Recharge.VerySlow,
        tags: PlantTag.Mushroom | PlantTag.Instant
        );

    protected override void BehaviourPerTick()
    {
        foreach (Zombie z in AllZombies())
        {
            z.ApplyEffect(new FrozenEffect(), 3.25f);
            z.ApplyEffect(new SlowedEffect(), 16 + 3.25f);
            DamageZombie(z);
        }
            
        Suicide();
    }
}
