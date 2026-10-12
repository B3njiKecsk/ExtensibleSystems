using System;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct ZombieStats
{
    private readonly string displayName;
    private readonly int maxHP;
    //Depricated
    //private readonly int noArmorHP;
    //private readonly int armorStateHP;
    private readonly float speed;
    private readonly float dps;
    private readonly float timeBetweenTicks;
    private readonly ZombieTag tags;
    private readonly LaneType[] spawnLanes;
    private int wavePointCost;
    private int waveWeight;

    public string DisplayName { get { return displayName; } }
    public int MaxHP { get { return maxHP; } }
    //Depricated
    //public int NoArmorHP { get { return noArmorHP; } }
    //public int ArmorStateHP { get { return armorStateHP; } }
    public float Speed { get { return speed; } }
    public float Dps { get { return dps; } }
    public float TimeBetweenTicks { get { return timeBetweenTicks; } }
    public int WavePointCost { get { return wavePointCost; } }
    public int WaveWeight { get { return waveWeight; } }
    public bool HasTag(ZombieTag tag)
    {
        return (tags & tag) != 0;
    }

    public bool CanSpawnOnLane(LaneType lane)
    {
        return spawnLanes.Contains(lane);
    }

    /// <param name="tags">Assignment example: PlantTag.Instant | PlantTag.Secondary</param>
    /// <param name="spawnInLanes"> By default, gets LaneType.Normal assigned to. <br></br> Beware of assigning LaneType.Dirt, currently the wave manager script ignores Dirt lanes for spawning zombies</param>
    public ZombieStats(
        string displayName,
        int maxHP = 0,
        //int noArmorHP = 0,
        //int armorStateHP = 0,
        float speed = 1 / 4.7f,
        float dps = 100f,
        float timeBetweenTicks = 999f,
        int wavePointCost = 1,
        int waveWeight = 4000,
        ZombieTag tags = ZombieTag.None,
        LaneType[] spawnInLanes = null
        )
    {
        this.displayName = displayName;
        this.maxHP = maxHP;
        //this.noArmorHP = noArmorHP;
        //this.armorStateHP = armorStateHP;
        this.speed = speed;
        this.dps = dps;
        this.timeBetweenTicks = timeBetweenTicks;
        this.tags = tags;
        this.wavePointCost = wavePointCost;
        this.waveWeight = waveWeight;

        spawnLanes = spawnInLanes;
        if (spawnLanes == null)
            spawnLanes = new LaneType[] { LaneType.Normal };
        if (CanSpawnOnLane(LaneType.Dirt))
            Debug.LogWarning($"Assigned spawn lane of type {LaneType.Dirt} for zombie {displayName} in its {nameof(ZombieStats)}; This will be ignored, zombies never spawn in lanes of type {LaneType.Dirt}");
    }

}