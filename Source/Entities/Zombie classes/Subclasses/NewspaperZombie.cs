using UnityEngine;

public class NewspaperZombie : Zombie
{
    const int newspaperMaxHP = 150;
    Armor newspaper;
    public NewspaperZombie(int row) : base(row, stats)
    {
        newspaper = new Armor(newspaperMaxHP, DamageInfo.Direction.left);
        GiveEquipment(newspaper);
    }

    public static readonly ZombieStats stats = new ZombieStats(
        displayName:"Newspaper Zombie",
        maxHP: 200,
        wavePointCost: 2,
        waveWeight: 1000
        );

    bool enraged = false;
    const float secondsPerGridEnraged = 1.8f;
    readonly float RunningSpeedMultiplier = 1 / (secondsPerGridEnraged * stats.Speed);
    protected override void Walk(float distance)
    {
        base.Walk(enraged? distance* RunningSpeedMultiplier : distance);
    }

    protected override void OnEquipmentRemoved(Equipment item)
    {
        if(item != newspaper)
        {
            base.OnEquipmentRemoved(item);
            return;
        }


        SetSprite(6);
        ApplyEffect(new ConfusedEffect(), 1f);
    }
    /*
    protected override void OnDamageTaken(DamageInfo info)
    {
        if (HP > 200 || enraged)
            return;
        if (pausedBehaviourEffects > 0)
            return;

        //just lost newspaper
        SetSprite(6);
        ApplyEffect(new ConfusedEffect(), 1f);
    }
    */
    public override void OnEffectRemoved(StatusEffect effect)
    {
        if (effect is not ConfusedEffect)
            return;

        enraged = true;
        UpdateSprite();
    }
    protected override int CalculateHPState()
    {
        return HP >= 200 ? 0
              : HP >= 100 ? 1
              : 2;
    }
    protected override void UpdateSprite()
    {
        if (PausedBehaviourEffects > 0)
            return;
        base.UpdateSprite();
    }
}
