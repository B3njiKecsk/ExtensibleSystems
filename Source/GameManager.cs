using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public enum CoinType
{
    None = 0,
    SilverCoin = 10,
    GoldCoin = 50,
    Diamond = 1000,
}
public class GameManager : MonoBehaviour
{
    public static DebugCanvasUI debug;
    [SerializeField] private GameObject SunPrefab;
    [SerializeField] private GameObject CoinPrefab;
    [SerializeField] private GameObject SeedpacketPrefab;
    [SerializeField] private GameObject rewardPrefab;
    [SerializeField] private Vector3 HotbarPosStart;
    [SerializeField] private float SpaceBetweenSeedPackets;
    [SerializeField] private Vector3 SeedPacketsPosStart;
    [SerializeField] private float SpaceBetweenRows;
    [SerializeField] private int MaxPacketsPerRow;
    [SerializeField] private Color statusTint_Frozen;
    [SerializeField] private Color statusTint_Hypnotized;
    [SerializeField] public Sprite FrozenEffectSprite;
    public Color StatusTint_Frozen => statusTint_Frozen;
    public Color StatusTint_Hypnotized => statusTint_Hypnotized;
    const float MaxDeltaTime = 0.1f;
    const float SunFallingSpeed = 1f;
    public const float sunHomeX = -8f;
    public const float sunHomeY = 4.3f;
    public const float sunLifetime = 30f;
    public const float collectibleCollectTime = 0.5f;
    public const float coinLifetime = 15f;
    public const int startingSun = 50;
    public const float ZombieTileBackReach = 0.15f;
    public const float ZombieReachZombie = 0.2f;
    public const float ZombieCollisionLength = 0.2f;
    public const string PlantPrefabsPath = "Prefabs/Plants/";
    public const string ZombiePrefabsPath = "Prefabs/Zombies/";
    public const string ProjectilesPrefabsPath = "Prefabs/Projectiles/";
    public const string TileTypesPrefabsPath = "Prefabs/MapElements/";
    public static int SeedSlots = 6;
    [SerializeField] private static int moneyBalance = 0;

    public static List<PlantType> UnlockedPlants = new();
    public static HashSet<RewardType> UnlockedRewards = new();
    public static GameManager Instance { get; private set; }

    public int currentWorldIndex { get; private set; }
    public int currentLevelIndex { get; private set; }
    public LevelManager levelManager { get; private set; }
    public static LevelUI levelUI;
    public static bool playing { get; private set; }
    static System.Random random = new();

    private static readonly World[] worlds =
    {
        new World_Day(random),
        new World_Night(random),
    };
    public LevelDefinition currentLevel { get; private set; }
    public World currentWorld { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Prevents destruction on Scene load

        UnlockPlant(PlantType.Peashooter);

        /*
        UnlockPlant(PlantType.Hypnoshroom);
        UnlockPlant(PlantType.Wallnut);
        UnlockPlant(PlantType.Fumeshroom);
        */

        /*
        //replace this with loading actual unlocked plants
        UnlockedPlants.AddRange(PlantRegistry.GetAllPlantTypes());
        */

        PlantRegistry.RegisterPlants();
        ZombieRegistry.RegisterZombies();
        ProjectileRegistry.RegisterProjectiles();
        LoadLevel(1,1); //first level of first world
    }
    private void Update()
    {
        //levelmanager gets updated only if we are in a level
        if (levelManager==null)
            return;

        if (Time.deltaTime > 1)
        {
            Debug.LogWarning($"Lagspike of {Time.deltaTime} seconds detected, clamping deltatime to {MaxDeltaTime} (MaxDeltaTime)");
            levelManager.Update(MaxDeltaTime);
        }
        else
            levelManager.Update(Time.deltaTime);

    }

    #region level loading
    public void LoadLevel(int world, int level)
    {
        world--; level--;
        currentWorldIndex = world;
        currentLevelIndex = level;
        currentWorld = worlds[world];
        currentLevel = worlds[world].GetLevel(level);
        StartCoroutine(LoadScene());
    }

    public void NextLevel()
    {
        currentLevelIndex++;
        currentLevel = worlds[currentWorldIndex].GetLevel(currentLevelIndex);
        if (currentLevel == null)
        {
            currentLevelIndex = 0;
            if (!NextWorld())   //if theres no next world, aka end of game, dont try to load the next level
                return;
            currentLevel = worlds[currentWorldIndex].GetLevel(currentLevelIndex);
        }
        Debug.Log($"loading next level... world {currentWorldIndex+1}-{currentLevelIndex+1}");

        StartCoroutine(LoadScene());

    }
    private bool NextWorld()
    {
        currentWorldIndex++;
        if (currentWorldIndex >= worlds.Length)
        {
            TriggerEndofGame();
            return false;
        }

        currentWorld = worlds[currentWorldIndex];
        return true;
    }
    private void TriggerEndofGame()
    {
        Debug.Log("Game Over, you win!");
    }
    
    private IEnumerator LoadScene()
    {
        yield return null;  //let the updates finish, to not destroy objects on load while they are being used

        levelManager = null;
        playing = false;
        levelUI = null;
        

        AsyncOperation operation = SceneManager.LoadSceneAsync(currentWorld.SceneID);
        while (!operation.isDone)
            yield return null;

        FinishSceneLoad();
    }
    public static void RegisterLevelUI(LevelUI ui)
    {
        if (levelUI != null)
            Debug.LogError("Multiple level ui have been trying to register. Make sure to only have one LevelUI component in the scene");
        levelUI = ui;
    }

    private void FinishSceneLoad()
    {
        if (levelUI == null)
            Debug.LogError("LevelUI component not found in scene. Make sure the");
        StartCYS();
    }
    #endregion

    private void StartCYS()
    {
        ChooseYourSeeds.Start(SeedSlots, UnlockedPlants, currentLevel.AllowedZombies, random);
        //StartLevel();
    }
    private void InitializeLevelManager()
    {
        if (levelManager != null)
            Debug.LogWarning("GameManager.Instance.levelManager is being overriden! levelManager should be cleared before initialization");
        if (levelUI == null)
            Debug.LogError("LevelManager cannot be initialized, due to levelUI not having been registered yet");
        levelManager = new LevelManager(currentWorld.geometry.rowN, currentWorld.geometry.colN, levelUI, random, currentWorld, currentLevel);
        playing = true;
    }
    public void StartLevel()
    {
        InitializeLevelManager();
        levelUI.StartLevel(currentWorldIndex, currentLevelIndex, currentLevel.Waves, ZombieWaveManager.flagWaves(currentLevel.Waves));
    }

    public void MoveCameraToLawn()
    {
        levelUI.MoveCameraToLawn();
    }
    public void MoveCameraToStreet()
    {
        levelUI.MoveCameraToStreet();
    }

    #region Prefab spawning

    #region Entities
    private T_Actor SpawnActorPrefab<T_Actor>(GameObject prefab, Vector3 pos) where T_Actor : EntityActor
    {
        if (prefab == null)
        {
            Debug.LogError($"Registry returned null while looking up the prefab to spawn a {nameof(T_Actor)}. It probably has not been added to the list");
            return null;
        }
        return Instantiate(prefab, pos, Quaternion.identity).GetComponent<T_Actor>();
    }
    public void RemoveActorPrefab(EntityActor Actor)
    {
        Destroy(Actor.gameObject);
    }

    public ZombieActor SpawnZombiePrefab(ZombieType type, int row, float xPos = -1)
    {
        float y = currentWorld.geometry.gridStartY - ((row+1) * currentWorld.geometry.cellHeight);    //the pivots of the zombie sprites should be BL
        float x = currentWorld.geometry.gridStartX + (currentWorld.geometry.zombieSpawnCol * currentWorld.geometry.cellWidth);
        if(xPos >= 0)
            x = currentWorld.geometry.gridStartX + (currentWorld.geometry.cellWidth * xPos);
        Vector3 pos = new Vector3(x, y, 0);

        ZombieActor actor = SpawnActorPrefab<ZombieActor>(
            ZombieRegistry.GetGameObject(type), 
            pos);

        actor?.SetLane(row, EntityActor.LaneRenderLayer.Zombie);
        return actor;
    }
    public PlantActor PlacePlantPrefab(PlantType type, int row, int col)
    {
        Vector3 pos = currentWorld.TileCoordsToWorldPos(col, row);

        PlantActor actor = SpawnActorPrefab<PlantActor>(
            PlantRegistry.GetGameObject(type), 
            pos);

        actor?.SetLane(row, EntityActor.LaneRenderLayer.Plant);
        return actor;
    }
    public ProjectileActor SpawnProjectilePrefab(ProjectileType type, float xPos, float yPos)
    {
        Vector3 pos = currentWorld.TileCoordsToWorldPos(xPos, yPos);

        ProjectileActor actor = SpawnActorPrefab<ProjectileActor>(
            ProjectileRegistry.GetGameObject(type), 
            pos);

        return actor;
    }
    #endregion

    #region seed packets

    public SeedPacketHandler SpawnSeedPacketPrefab_Hotbar(int hotbarSlot)
    {
        Vector3 position = new Vector3(HotbarPosStart.x + SpaceBetweenSeedPackets*hotbarSlot, HotbarPosStart.y, HotbarPosStart.z);
        return SpawnSeedPacketPrefab(position);
    }
    public SeedPacketHandler SpawnSeedPacketPrefab_AllSeeds(int index)
    {
        Vector3 position = new Vector3(SeedPacketsPosStart.x + (SpaceBetweenSeedPackets * (index % MaxPacketsPerRow)), SeedPacketsPosStart.y - (SpaceBetweenRows * (index/MaxPacketsPerRow)), SeedPacketsPosStart.z);
        return SpawnSeedPacketPrefab(position);
    }

    private SeedPacketHandler SpawnSeedPacketPrefab(Vector3 position)
    {
        GameObject go = Instantiate(SeedpacketPrefab, levelUI.GetCanvas().transform);
        go.transform.localPosition = position;
        return go.GetComponent<SeedPacketHandler>();
    }
    public void RemoveSeedPacketPrefab(SeedPacketHandler seedPacket)
    {
        Destroy(seedPacket.gameObject);
    }
    #endregion

    internal void GenerateSunPrefab(int amount, float xPos, float yPos, float fallToYPos)
    {
        bool smallSprite = amount < 25 ? true : false;
        Vector3 SpawnPos = currentWorld.TileCoordsToWorldPos(xPos, yPos);
        Vector3 target = currentWorld.TileCoordsToWorldPos(xPos, fallToYPos);
        SunActor sun = Instantiate(SunPrefab, SpawnPos, Quaternion.identity).GetComponent<SunActor>();

        if (yPos == fallToYPos)
            sun.SetUpSun(smallSprite, amount);
        else
            sun.SetUpSun(smallSprite, amount, SunFallingSpeed, target.y);
    }

    public List<CoinActor> GenerateCoinPrefabs(int amount, float col, float row)
    {
        Debug.Log($"Generating {amount}€ coins");
        Vector3 pos = currentWorld.TileCoordsToWorldPos(col, row);
        Dictionary<CoinType, int> coinAmounts = new();

        var coinTypes = ((CoinType[])Enum.GetValues(typeof(CoinType)))
        .OrderByDescending(c => (int)c);

        foreach (var coin in coinTypes)
        {
            int value = (int)coin;
            if (value == 0)
                continue;
            coinAmounts[coin] = amount / value;
            amount %= value;
        }

        if (amount > 0)
            Debug.LogWarning($"Coin type of value less than {(int)coinTypes.ElementAt(-1)}");

        List<CoinActor> coinActors = new();

        foreach(var coin in coinAmounts.Keys)
            for(int i = 0; i<coinAmounts[coin]; i++)
                coinActors.Add(GenerateCoinPrefab(coin, pos));

        return coinActors;
    }

    private CoinActor GenerateCoinPrefab(CoinType type, Vector3 pos)
    {
        CoinActor coin = Instantiate(CoinPrefab, pos, Quaternion.identity).GetComponent<CoinActor>();
        coin.SetUpCoin(type);
        return coin;
    }

    #endregion

    public void Lose(Zombie zombie)
    {
        Debug.LogError($"The zombies ate your brains! specifically a {zombie.Stats.DisplayName} zombie");
    }

    public bool AcceptHotbar(PlantType[] hotbarActors)
    {
        string warning = ChooseYourSeeds.GetHotbarWarning(hotbarActors, currentWorld, currentLevel);
        if (warning != null)
        {
            levelUI.WarnPlantChoice(warning);
            return false;
        }
        return true;
    }

    public TileActor SpawnTilePrefab(int row, int col, TileObject type)
    {
        Vector3 pos = currentWorld.TileCoordsToWorldPos(col, row);
        TileActor actor;

        GameObject prefab;
        switch (type)
        {
            case TileObject.Grave:
                prefab = Resources.Load<GameObject>(TileTypesPrefabsPath + "Grave");
                actor = SpawnActorPrefab<TileActor>(prefab, pos);
                actor.GetComponent<TileActor>().SetRandomSprite(random);
                break;

            default: return null;
        }

        actor?.SetLane(row, EntityActor.LaneRenderLayer.Tile);
        return actor;
    }
    
    public static int ModifyBalance(int amount)
    {
        moneyBalance += amount;
        return moneyBalance;
    }

    public void SpawnRewardPrefab(LevelReward reward, float xPos, float yPos)
    {
        Vector3 pos = currentWorld.TileCoordsToWorldPos(xPos, yPos);
        RewardActor actor = Instantiate(rewardPrefab, pos, Quaternion.identity).GetComponent<RewardActor>();
        actor.Setup(reward);
    }

    

    public void UnlockReward(LevelReward reward)
    {
        switch(reward.rewardType)
        {
            case RewardType.SeedPacket:
                UnlockPlant((PlantType)reward.rewardPlant);
                return;
            case RewardType.Note1:
            case RewardType.Note2:
            case RewardType.Note3:
            case RewardType.Note4:
            case RewardType.Note5:
            case RewardType.Note6:
                DisplayNote(reward.rewardType);
                return;
            default: UnlockedRewards.Add(reward.rewardType); return;
        }
    }
    private void DisplayNote(RewardType note)
    {
        Debug.Log("Displaying Note " + note);
    }

    private void UnlockPlant(PlantType rewardPlant)
    {
        if (UnlockedPlants.Contains(rewardPlant))
            return;

        UnlockedPlants.Add(rewardPlant);
    }

    public int balance => moneyBalance;
    public bool ShovelIsUnlocked => UnlockedRewards.Contains(RewardType.Shovel);
}
