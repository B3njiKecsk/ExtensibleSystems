using UnityEngine;

public class ConfusedEffect : StatusEffect
{
    protected override void EffectOnApplied()
    {
        target.AddBehaviourSpeedModifier(0);
        //target.actor.questionmarkabovehead
    }

    protected override void OnEffectEnd()
    {
        target.RemoveBehaviourSpeedModifier(0);
    }
}
