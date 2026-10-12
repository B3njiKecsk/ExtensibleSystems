
public class Wallnut : Plant
{
    public Wallnut(int row, int col) : base(row, col, stats)
    {
    }

    public static readonly PlantStats stats = new PlantStats
    (
        "Wallnut",
        maxHP: 4000,
        sunCost: 50,
        rechargeTime: 30f
    );


    int HPState = 0;
    const int maxStates = 3;
    protected override void OnDamageTaken(DamageInfo info)
    {
        if(HPState < maxStates-1 && (base_stats.MaxHP - HP) > base_stats.MaxHP * ((1+HPState) / (float)maxStates))
        {
            HPState++;
            SetSprite(HPState);
        }
    }
}

// state = maxhp - hp / (maxhp/3)
