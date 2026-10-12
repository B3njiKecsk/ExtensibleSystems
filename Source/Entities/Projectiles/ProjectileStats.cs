[System.Serializable]
public struct ProjectileStats
{
    private readonly string displayName;
    private readonly int damage;
    private readonly float speed;  //tilespersecond
    private readonly float range;
    private readonly int maxHP;
    private readonly float timeBetweenTicks; 
    public string DisplayName { get { return displayName; } }
    public int Damage { get { return damage; } }
    public float Speed { get { return speed; } }
    public float Range { get { return range; } }
    public int MaxHP { get { return maxHP; } }
    public float TimeBetweenTicks { get { return timeBetweenTicks; } }

    public ProjectileStats(
        string displayName,
        int damage = 0,
        float speed = 0f,
        float range = -1f,
        int maxHP = 1,
        float timeBetweenTicks = 9999)
    {
        this.displayName = displayName;
        this.damage = damage;
        this.speed = speed;
        this.range = range;
        this.maxHP = maxHP;
        this.timeBetweenTicks = timeBetweenTicks;
    }

}