using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class ZombieWaveManager
{
    public const float SecondsBeforeFirstWave = 28f; //28f;
    public const float MaxSecondsBetweenWaves = 28f; //between 25 and 31
    private float TimeForNextWave = SecondsBeforeFirstWave;
    private readonly LaneType[] Lanes;
    private int[] PausedLanes;
    private readonly Func<ZombieType, int, Zombie> _spawnZombie; //Func<ZombieType, int, Zombie> for a function that returns Zombie //Action<ZombieType, int> for one that doesnt
    private readonly Func<ZombieType, int, float, Zombie> _spawnAmbushZombie;
    private System.Random random;
    public int currentWave { private set; get; } = 0;
    public readonly int levelWaves = 10;
    private int currentWavePoints;
    private IReadOnlyList<(int wave, List<ZombieType> zombies)> scriptedWaves;
    private List<Zombie> currentWaveZombies = new();
    private int currentWaveMaxHP;
    private List<Zombie> allZombies = new();

    private IReadOnlyList<ZombieType> allowedZombies;
    private ProgressBar progressBar;
    public ZombieWaveManager(ProgressBar progressBar, System.Random random, Func<ZombieType, int, Zombie> spawnZombie, Func<ZombieType, int, float, Zombie> spawnAmbushZombie, LaneType[] laneTypes, int Waves, IReadOnlyList<ZombieType> SpawnableZombies, IReadOnlyList<(int wave, List<ZombieType> zombies)> ScriptedWaves = null)
    {
        this.random = random;
        this.progressBar = progressBar;
        _spawnZombie = spawnZombie;
        _spawnAmbushZombie = spawnAmbushZombie;
        Lanes = laneTypes;
        PausedLanes = new int[Lanes.Length];
        allowedZombies = SpawnableZombies;
        scriptedWaves = ScriptedWaves;
        levelWaves = Waves;
    }
    public void Update(float deltaTime)
    {
        if (currentWave >= levelWaves)
            return;

        TimeForNextWave -= deltaTime;
        GameManager.debug?.UpdateTimer(TimeForNextWave);
        if (TimeForNextWave <= 0)
        {
            TimeForNextWave = MaxSecondsBetweenWaves;
            NextWave();
        }
    }
    public void UpdateDamage(Zombie zombie)
    {
        if(zombie.IsDead || zombie.team != Team.Zombies)
        {
            currentWaveZombies.Remove(zombie);
            allZombies.Remove(zombie);
        }

        GameManager.debug?.UpdateZombiesOnScreen(allZombies.Count);
        //on last wave, only check if all zombies died, in which case end the level
        if(currentWave >= levelWaves)
        {
            if (allZombies.Count == 0)
                GameManager.Instance.levelManager.WinLevel(zombie);

            return;
        }

        //on other waves, before a flag wave advance early only if every zombie died
        if(isFlagWave(currentWave+1) || currentWave+1 == levelWaves)
        {
            if(allZombies.Count == 0)
                EarlyNextWave();
            return;
        }

        //on every other wave, only advance if more than half hp has been done
        if (currentWaveZombies.Sum(z => z.CalculatedHP) <= currentWaveMaxHP / 2)
            EarlyNextWave();
    }
    private bool isFlagWave(int wave)
    {
        return wave % 10 == 0;
    }
    public static int flagWaves(int waves)
    {
        return Math.Max(1,waves / 10);
    }
    private void EarlyNextWave()
    {
        Debug.Log("Wave called early");
        if (TimeForNextWave <= 2f)
            return;
        TimeForNextWave = 2f;
    }
    private void NextWave()
    {
        currentWave++;
        GameManager.debug?.UpdateWave(currentWave, levelWaves);
        currentWaveZombies.Clear();
        currentWaveMaxHP = 0;

        progressBar.nextStep();

        for (int lane = 0; lane < PausedLanes.Length; lane++)
            if (PausedLanes[lane]>0)
                PausedLanes[lane]--;



        if (scriptedWaves != null)
        {
            var wave = scriptedWaves.FirstOrDefault(w => w.wave == currentWave);
            

            if (currentWave == wave.wave)
            {
                ScriptedWave(wave.zombies);
                return;
            }
        }

        if (isFlagWave(currentWave) || currentWave == levelWaves)
            FlagWave(currentWave);
        else
            NormalWave(currentWave);
    }
    private void FlagWave(int wave)
    {
        Debug.Log("Flag wave spawning...");
        if (!isFlagWave(wave))
            currentWavePoints = UnusualLastwavePoints(wave);
        else
            if (wave == levelWaves)
                FinalWave(wave);
            else
                currentWavePoints = wave == 10 ? 10 : 17;   //UGLY hard scripted

        SpawnZombieWithPoints(ref currentWavePoints, new List<ZombieType> { ZombieType.Flag });
        while (currentWavePoints > 0)
            SpawnZombieWithPoints(ref currentWavePoints, allowedZombies);
    }
    private void FinalWave(int wave)
    {
        if (wave == 10)
            currentWavePoints = 10;
        if (wave == 20)
            currentWavePoints = 17;
        if (wave == 30)
            currentWavePoints = 25;

        SpawnAmbush();
    }
    private void NormalWave(int wave)
    {
        currentWavePoints = CalculateWavePoints(wave);
        while (currentWavePoints > 0)
            SpawnZombieWithPoints(ref currentWavePoints, allowedZombies);
    }
    private void ScriptedWave(List<ZombieType> zombies)
    {
        foreach (ZombieType z in zombies)
            SpawnZombieAnyLane(z);
    }

    private int UnusualLastwavePoints(int lastWave)
    {

        switch (lastWave)
        {
            case 4: return 2;
            case 6: return 5;
            case 8: return 7;
            default: return CalculateWavePoints(((lastWave + 9) / 10) * 10);
        }

    }
    private int CalculateWavePoints(int wave)
    {
        Debug.Log($"-- Calculating wave points for wave {wave}, points should be {(wave + 2) / 3}");
        return (wave + 2) / 3;
    }
    private ZombieType SpawnZombieFromWave(IReadOnlyList<ZombieType> allowedZombies)
    {
        ZombieType zombieType = ZombieType.NullZombie;

        var availableZombies = new List<ZombieType>(allowedZombies);
        var availableLanes = new List<int>();

        int i = 1;
        while (availableLanes.Count == 0)
        {
            Debug.Log($"Calculating zombie with allowed zombies the {i}th time, with {availableZombies.Count} zombies");
            zombieType = GetRandomZombieWeighted(availableZombies); //this guarantees assignment at least once to zombieType

            for (int lane = 0; lane < Lanes.Length; lane++)
            {
                if (CanSpawnZombieOnLane(zombieType, lane))
                    availableLanes.Add(lane);
            }
            Debug.Log($"{availableLanes.Count} available lanes found for zombie {zombieType}");
            if(availableLanes.Count == 0)
                availableZombies.Remove(zombieType);
            i++;
        }

        int spawnLane = availableLanes[random.Next(availableLanes.Count)];
        SpawnZombie(zombieType, spawnLane);
        return zombieType;
    }

    private bool CanSpawnZombieOnLane(ZombieType z, int lane)
    {
        return
            GameManager.Instance.levelManager.CanLaneSpawnZombie(z, lane, currentWave)
            && ZombieRegistry.CanSpawnOnLane(z, Lanes[lane])
            && PausedLanes[lane] <= 0;
    }

    private void SpawnZombieWithPoints(ref int currentPoints, IReadOnlyList<ZombieType> allowedZombies)
    {
        int points = currentPoints;
        ZombieType zt = SpawnZombieFromWave(allowedZombies.Where(z => points >= ZombieRegistry.GetWavePoints(z)).ToList());
        currentPoints -= ZombieRegistry.GetWavePoints(zt);
    }
    private void SpawnZombieAnyLane(ZombieType type)
    {
        int lane = random.Next() % Lanes.Length;

        while (!ZombieRegistry.CanSpawnOnLane(type, Lanes[lane]))
            if (++lane >= Lanes.Length)
                lane = 0;

        SpawnZombie(type, lane);
    }
    private ZombieType GetRandomZombieWeighted(IReadOnlyList<ZombieType> allowedZombies)
    {
        var weights = allowedZombies.Select(zombie => (type: zombie, weight: ZombieRegistry.GetWaveWeight(zombie))).ToList();

        if (weights.Count == 0)
            throw new InvalidOperationException("No valid zombies available.");


        int roll = random.Next(weights.Sum(w => w.weight));

        foreach (var (type, weight) in weights)
        {
            if (roll < weight)
                return type;

            roll -= weight;
        }

        throw new InvalidOperationException(); //this should never be reached
    }
    private Zombie SpawnZombie(ZombieType type, int lane)
    {
        Zombie z = _spawnZombie(type, lane);
        AddZombieToWave(z);
        return z;
    }
    
    private Zombie SpawnAmbushZombie(ZombieType type, int lane, float xPos)
    {
        Zombie z = _spawnAmbushZombie(type, lane, xPos);
        AddZombieToWave(z);
        return z;
    }
    private void AddZombieToWave(Zombie z)
    {
        allZombies.Add(z);
        currentWaveZombies.Add(z);
        currentWaveMaxHP += z.CalculatedMaxHP;
    }

    private void SpawnAmbush()
    {
        World world = GameManager.Instance.currentWorld;
        List<ZombieType> allowedAmbushZombies = allowedZombies.Intersect(world.GetAmbushZombies()).ToList();
        foreach (var point in world.GetAmbushPositions())
            SpawnAmbushZombie(GetRandomZombieWeighted(allowedAmbushZombies), point.lane, point.xPos);
    }

    public static List<ZombieType> getPreviewZombies(int previewZombieCount, IReadOnlyList<ZombieType> allowedZombies, System.Random random)
    {
        List<ZombieType> zombies = new List<ZombieType>();
        //add one of each zombie from the level
        zombies.AddRange(allowedZombies);

        //if that already fills the zombie count, we done
        if (zombies.Count >= previewZombieCount)
            return zombies;


        //otherwise we calculate who to put in the remaining slots
        Dictionary<ZombieType, float> weightCostRatio = new();

        foreach (ZombieType type in allowedZombies)
            weightCostRatio[type] = ZombieRegistry.GetWaveWeight(type)*2 / ZombieRegistry.GetWavePoints(type);

        int remaining = previewZombieCount - zombies.Count;
        float totalWeight = weightCostRatio.Values.Sum();

        Dictionary<ZombieType, float> remainders = new();

        foreach (var pair in weightCostRatio)
        {
            float exact = remaining * pair.Value / totalWeight;
            int count = (int)exact;

            for (int i = 0; i < count; i++)
                zombies.Add(pair.Key);

            remainders[pair.Key] = exact - count;
        }

        return zombies;
    }
    

    public void _DBG_NextWave()
    {
        TimeForNextWave = 0;
    }

    public void PauseZombieSpawns(int lane, int waves)
    {
        PausedLanes[lane] = waves;
    }
}
