using UnityEngine;

public class FrozenEffect : StatusEffect
{
    protected override void EffectOnApplied()
    {
        target.AddBehaviourSpeedModifier(0);
        target.actor.SetFrozenEffect(true);
    }

    protected override void OnEffectEnd()
    {
        target.RemoveBehaviourSpeedModifier(0);
        target.actor.SetFrozenEffect(false);
    }
}
