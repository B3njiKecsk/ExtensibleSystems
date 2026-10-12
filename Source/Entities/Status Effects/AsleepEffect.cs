using UnityEngine;

public class AsleepEffect : StatusEffect
{
    protected override void EffectOnApplied()
    {
        target.AddBehaviourSpeedModifier(0);
    }

    protected override void OnEffectEnd()
    {
        target.RemoveBehaviourSpeedModifier(0);
    }
}
