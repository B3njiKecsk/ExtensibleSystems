using System.Collections.Generic;
using UnityEngine;

public abstract class StatusController
{
    private List<StatusEffect> effects = new();
    private float stat_Speed;
    protected float movementSpeedMultiplier { get; private set; } = 1;
    protected int frozenMovementEffects { get; private set; } = 0;
    protected float MovementSpeed => frozenMovementEffects > 0 ? 0 : stat_Speed * movementSpeedMultiplier;
    protected float behaviourSpeedMultiplier { get; private set; } = 1;
    protected int pausedBehaviourEffects { get; private set; } = 0;
    protected float BehaviourSpeed => pausedBehaviourEffects > 0 ? 0 : behaviourSpeedMultiplier;

    public StatusController(float speed)
    {
        stat_Speed = speed;
    }

    public void AddMovementSpeedModifier(float multiplier)
    {
        if (multiplier <= 0)
            frozenMovementEffects++;
        else
            movementSpeedMultiplier *= multiplier;
    }
    public void RemoveMovementSpeedModifier(float multiplier)
    {
        if (multiplier <= 0)
            frozenMovementEffects--;
        else
            movementSpeedMultiplier /= multiplier;
    }
    public void AddBehaviourSpeedModifier(float multiplier)
    {
        if (multiplier <= 0)
            pausedBehaviourEffects++;
        else
            behaviourSpeedMultiplier *= multiplier;
    }
    public void RemoveBehaviourSpeedModifier(float multiplier)
    {
        if (multiplier <= 0)
            pausedBehaviourEffects--;
        else
            behaviourSpeedMultiplier /= multiplier;
    }


    public void ApplyEffect(StatusEffect effect, float duration)
    {
        OnEffectReceived(effect);
        if (!CanGetEffect(effect))
            return;
        StatusEffect currentEffect = GetEffect(effect);
        if (currentEffect != null)
            currentEffect.Reapply(duration);
        else
        {
            effects.Add(effect);
            //effect.Apply(this, duration);
        }
    }
    public void RemoveEffect(StatusEffect effect)
    {
        OnEffectRemoved(effect);
        effects.Remove(effect);
    }
    public StatusEffect GetEffect<Effect>() where Effect : StatusEffect
    {
        StatusEffect match = null;
        foreach (var effect in effects)
        {
            match = effect.GetType() == typeof(Effect) ? effect : match;

        }
        return match;
    }
    public StatusEffect GetEffect(StatusEffect Effect)
    {
        StatusEffect match = null;
        foreach (var effect in effects)
        {
            match = effect.GetType() == Effect.GetType() ? effect : match;
        }
        return match;
    }

    public virtual void OnEffectReceived(StatusEffect effect) { }
    public virtual void OnEffectRemoved(StatusEffect effect) { }
    public virtual bool CanGetEffect(StatusEffect Effect) { return true; }

}
