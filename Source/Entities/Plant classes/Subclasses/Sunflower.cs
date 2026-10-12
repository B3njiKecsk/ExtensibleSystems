using Unity.VisualScripting;
using UnityEngine;

public class Sunflower : Plant
{
    public Sunflower(int row, int col) : base(row, col, stats)
    {
        SetTimeBeforeNextTick(7f);
    }

    public static readonly PlantStats stats = new PlantStats
    (
        displayName: "Sunflower",
        sunCost: 50,
        rechargeTime: 7.5f,
        maxHP: 300,
        timeBetweenTicks: 24f,
        tags: PlantTag.SunProducer
    );

    protected override void BehaviourPerTick()
    {
        SummonSun(25);
    }
}
