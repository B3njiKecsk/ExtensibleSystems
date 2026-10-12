using UnityEngine;

public class SlowedEffect : StatusEffect
{
    const float slowedModifier = 0.5f;
    protected override void EffectOnApplied()
    {
        target.AddBehaviourSpeedModifier(slowedModifier);
        target.actor.SetSlowedTint(true);
    }

    protected override void OnEffectEnd()
    {
        target.RemoveBehaviourSpeedModifier(slowedModifier);
        target.actor.SetSlowedTint(false);
    }
}
