using System.Collections.Generic;
using UnityEngine;

public class Sunshroom : Plant
{
    public Sunshroom(int row, int col) : base(row, col, stats)
    {
        SetTimeBeforeNextTick(7f);
    }

    public static readonly PlantStats stats = new PlantStats
    (
        displayName: "Sun-shroom",
        sunCost: 25,
        rechargeTime: 7.5f,
        maxHP: 300,
        timeBetweenTicks: 24f,
        tags: PlantTag.Mushroom | PlantTag.SunProducer
    );
    float timeUntilGrowth = 120f;
    bool grown = false;
    int sunAmount = 15;
    protected override void BehaviourPerFrame(float deltaTime)
    {
        if (grown)
            return;
        timeUntilGrowth -= deltaTime;
        if (timeUntilGrowth < 0)
            Grow();

    }
    private void Grow()
    {
        sunAmount = 25;
        grown = true;
        SetSprite(2);
    }
    protected override void BehaviourPerTick()
    {
        SummonSun(sunAmount);
    }

}
