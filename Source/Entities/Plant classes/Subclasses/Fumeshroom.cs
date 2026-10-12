using System.Linq;
using UnityEngine;

public class Fumeshroom : Plant
{
    public Fumeshroom(int row, int col) : base(row, col, stats)
    {
    }

    public static readonly PlantStats stats = new PlantStats
    (
        displayName: "Fume-shroom",
        maxHP: Toughness.Normal,
        rechargeTime: Recharge.Fast,
        attackDamage: 20,
        sunCost: 75,
        timeBetweenTicks: 1.425f / 2, //we divide by two halves, since we damage one half, change sprites on the other
        tags: PlantTag.Mushroom,
        range: 4
    );
    bool shooting;
    protected override void BehaviourPerTick()
    {
        if (shooting)
        {
            shooting = false;
            SetSprite(0);
            return;
        }
        if (ZombiesInRange().Count() > 0)
        {
            shooting = true;
            SetSprite(2);
            foreach (Zombie zombie in ZombiesInRange())
                DamageZombie(zombie, DamageInfo.Direction.directionless);
        }
    }
}
