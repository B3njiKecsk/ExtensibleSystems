[System.Serializable]
public struct PlantStats
{
    private readonly string displayName;
    private readonly string description;
    private readonly int maxHP;
    private readonly int sunCost;
    private readonly float rechargeTime;
    private readonly float attackDamage;
    private readonly float timeBetweenTicks;
    private readonly float range;
    private readonly PlantTag tags;


    public string DisplayName { get { return displayName; } }
    public string Description { get { return description; } }
    public int MaxHP { get { return maxHP; } }
    public int SunCost { get { return sunCost; } }
    public float RechargeTime { get { return rechargeTime; } }
    public float AttackDamage { get { return attackDamage; } }
    public float TimeBetweenTicks { get { return timeBetweenTicks; } }
    public float Range { get { return range; } }

    

    public bool HasTag(PlantTag tag)
    {
        return (tags & tag) != 0;
    }

    /// <param name="tags">Assignment example: PlantTag.Instant | PlantTag.Secondary</param>
    public PlantStats(
        string displayName,
        string description = "",
        int maxHP = 0,
        int sunCost = 0,
        float rechargeTime = 0f,
        float attackDamage = 0f,
        float timeBetweenTicks = 9999f,
        float range = -1f,
        PlantTag tags = PlantTag.None)
    {
        this.displayName = displayName;
        this.description = description;
        this.maxHP = maxHP;
        this.sunCost = sunCost;
        this.rechargeTime = rechargeTime;
        this.attackDamage = attackDamage;
        this.timeBetweenTicks = timeBetweenTicks;
        this.tags = tags;
        this.range = range;
    }
    
}