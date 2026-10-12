using UnityEngine;

public abstract class Mushroom : Plant
{
    public Mushroom(int row, int col, PlantStats stats) : base(row, col, stats)
    {
    }

    protected bool isAsleep => GetEffect<AsleepEffect>() != null;

    protected override void BehaviourOnStart()
    {
        if (CurrentWorldHasTag(WorldTag.MushroomsAwake))
            return;

        ApplyEffect(new AsleepEffect());
        SetSprite(1);
    }

    public override bool CanBeEaten(Zombie origin)
    {
        if(CurrentWorldHasTag(WorldTag.MushroomsAwake))
            return base.CanBeEaten(origin);

        return true;
    }

    public override void OnEffectRemoved(StatusEffect effect)
    {
        base.OnEffectRemoved(effect);
        SetSprite(0);
    }
}
