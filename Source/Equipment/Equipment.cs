using System;

public abstract class Equipment
{
    protected Entity owner { get; private set; }
    public int MaxHP { get; }
    public float HP { get; private set; }

    protected Equipment( int maxHP)
    {
        MaxHP = maxHP;
        HP = maxHP;
    }
    public void Equip(Entity owner)
    {
        this.owner = owner;
    }
    /// <summary>
    /// Called after this equipment takes damage
    /// </summary>
    /// <param name="info"></param>
    public virtual void OnDamaged(DamageInfo info) { }
    public float AbsorbDamage(DamageInfo info)
    {
        if (!ProtectAgainst(info))
            return info.Amount;

        HP -= info.Amount;
        OnDamaged(info);
        float leftoverDamage = Math.Max(0, -HP);

        if (HP <= 0)
        {
            HP = 0;
            Break();
        }

        return leftoverDamage;
    }
    /// <summary>
    /// When the owner would take incoming damage, determines weather this should take it instead
    /// </summary>
    /// <param name="info">Source of incoming damage</param>
    /// <returns>
    /// true:  the equipment will take damage <br></br>
    /// false: the owner will take damage
    /// </returns>
    protected abstract bool ProtectAgainst(DamageInfo info);

    private void Break()
    {
        owner.RemoveEquipment(this);
    }
    /// <summary>
    /// Called after this equipment has been removed from its owner
    /// </summary>
    public virtual void OnRemoved()
    {

    }
}