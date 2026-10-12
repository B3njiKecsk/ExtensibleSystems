using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;

public enum LaneType
{
    Dirt = -1,
    Normal = 0,
    Water = 1,
}
public class Lane
{
    public LaneType Type {  get; private set; }
    public TileData[] tiles {  get; private set; }
    //dunno if it should be a queue, for now can change it as long as its IEnumerable
    private List<Zombie> zombies = new List<Zombie>();
    public IReadOnlyList<Zombie> Zombies => zombies;
    private readonly int row;
    private readonly int ZombieSpawnWave;
    public Lane(int row, int columns, LaneType type, int zombieSpawnWave = 0)
    {
        tiles = new TileData[columns];
        for (int i = 0; i < tiles.Length; i++) { tiles[i] = new TileData(row, i); }
        Type = type;
        ZombieSpawnWave = zombieSpawnWave;
    }

    public void AddZombie(Zombie z)
    {
        zombies.Add(z);
    }


    public IEnumerable<Zombie> GetZombiesAtRange(float xPosStart, float xPosEnd)
    {
        foreach (Zombie zombie in new List<Zombie>(zombies))
            if(zombie.xPos >= xPosStart &&  zombie.xPos <= xPosEnd)
                yield return zombie;
        yield break;
    }
    public IEnumerable<Zombie> GetZombiesInFrontOf(float xPos)
    {
        foreach (Zombie zombie in new List<Zombie>(zombies))
            if (zombie.xPos >= xPos)
                yield return zombie;
        yield break;
    }
    public IEnumerable<Zombie> GetZombiesNearPosition(float xPos, float collisionLength)
    {
        return GetZombiesAtRange(xPos - (collisionLength / 2), xPos + (collisionLength / 2));
    }
    /// <summary>
    /// ordered by distance from point
    /// </summary>
    public IEnumerable<Zombie> GetZombiesNearPositionOrdered(float xPos, float collisionLength)
    {
        return GetZombiesNearPosition(xPos, collisionLength).OrderBy(z => Math.Abs(z.xPos - xPos));
    }
    public void RemoveZombie(Zombie zombie)
    {
        zombies.Remove(zombie);
    }

    public bool CanSpawnZombies(int wave)
    {
        return wave >= ZombieSpawnWave;
    }
}