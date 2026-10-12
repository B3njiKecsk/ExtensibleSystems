public abstract class StatusEffect
{
    protected float timeRemaining;
    protected bool doesntExpire = false;
    public bool expired {  get; private set; }
    protected Entity target { get; private set; }
    protected abstract void EffectOnApplied();
    public void Apply(Entity target, float duration = -1)
    {
        this.target = target;
        timeRemaining = duration;
        if (duration < 0)
            doesntExpire = true;

        EffectOnApplied();
    }
    protected virtual void EffectOnUpdate() { }
    public void Update(float deltaTime)
    {
        if(doesntExpire)
        {
            EffectOnUpdate();
            return;
        }

        timeRemaining -= deltaTime;
        if (timeRemaining < 0)
        {
            OnEffectEnd();
            expired = true;
            RemoveExpired();
        }
        EffectOnUpdate();
    }
    protected abstract void OnEffectEnd();
    protected virtual void OnEffectRemoved() { OnEffectEnd(); }
    internal void RemoveExpired()
    {
        if (expired)
            target.RemoveEffect(this);
    }
    public void RemoveEffect()
    {
        OnEffectRemoved();
        expired = true;
    }
    public void Reapply(float duration)
    {
        EffectOnReapplied(duration);
    }
    protected virtual void EffectOnReapplied(float duration) 
    {
        timeRemaining = timeRemaining > duration ? timeRemaining : duration;
    }
}
