using UnityEngine;

public class Buckethead : Zombie, IMagnetizeable
{
    const int bucketMaxHP = 1080;
    Armor bucket;
    public Buckethead(int row) : base(row, stats)
    {
        bucket = new Armor(bucketMaxHP);
        GiveEquipment(bucket);
    }
    public static readonly ZombieStats stats = new ZombieStats
    (
        displayName: "BucketHead",
        maxHP: 200,
        dps: 100,
        speed: 1 / 4.7f,
        wavePointCost: 4
    );

    public Equipment Magnetize(Entity source)
    {
        RemoveEquipment(bucket);
        return bucket;
    }

    protected override int CalculateHPState()
    {
        int bucketState = (int)((bucket.MaxHP - bucket.HP) / (bucket.MaxHP / 3));

        int zombieState = HP > (stats.MaxHP / 2) ? 0 : 1;

        if (bucket.HP > 0)
            return bucketState;

        return bucketState + zombieState;
    }
}
