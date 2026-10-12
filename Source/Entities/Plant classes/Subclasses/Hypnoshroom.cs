using UnityEngine;

public class Hypnoshroom : Mushroom
{
    public Hypnoshroom(int row, int col) : base(row, col, stats)
    {
    }

    public static readonly PlantStats stats = new PlantStats(
        displayName: "Hypno-shroom",
        sunCost: 75,
        rechargeTime: 1f,//Recharge.Slow,
        maxHP: 300
        );

    protected override void OnBitten(DamageInfo damageInfo)
    {
        if (damageInfo.Source is not Zombie)
            return;

        Zombie victim = (Zombie)damageInfo.Source;

        victim.ApplyEffect(new HypnotizedEffect(Team.Plants));

        Suicide();
    }
}
