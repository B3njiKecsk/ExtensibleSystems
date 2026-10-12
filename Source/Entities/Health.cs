using System;
using UnityEngine;

public abstract class Health
{
    public Health(float maxHP)
    {
        HP = maxHP;
    }
    public float HP { get; protected set; }
    public bool isDead => HP <= 0;

    protected void ApplyDamage(float amount)
    {
        HP = HP - amount;
        if (HP <= 0)
            ManageDeath();
    }
    public virtual void Damage(DamageInfo info)
    {
        ApplyDamage(info.Amount);
    }
    protected void Suicide()
    {
        HP = 0;
        ManageDeath();
    }
    protected virtual void ManageDeath()
    {
        DestroySelf();
    }
    public void InstaKill(DamageInfo info)
    {
        OnInstaKill(info);
    }
    protected virtual void OnInstaKill(DamageInfo info)
    {
        ManageDeath();
    }

    protected abstract void DestroySelf();
}
