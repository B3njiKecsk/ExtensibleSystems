using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;
using static UnityEngine.Rendering.DebugUI.Table;

public enum Team
{
    None,
    Plants,
    Zombies,
}
public enum TeamTarget
{
    Enemy,
    Ally,
    All,
}
public abstract class Entity
{
    public EntityActor actor { get; private set; }
    protected List<StatusEffect> effects { private set; get; } = new();
    private readonly float stat_Speed;
    private readonly int stat_HP;
    private readonly float stat_timeBetweenTicks;
    protected float MovementSpeedMultiplier { get; private set; } = 1;
    protected int FrozenMovementEffects { get; private set; } = 0;
    protected float MovementSpeed => FrozenMovementEffects > 0 ? 0 : stat_Speed * MovementSpeedMultiplier;
    protected float BehaviourSpeedMultiplier { get; private set; } = 1;
    protected int PausedBehaviourEffects { get; private set; } = 0;
    protected float BehaviourSpeed => PausedBehaviourEffects > 0 ? 0 : BehaviourSpeedMultiplier;
    private float tickTimeLeft;
    public float HP { get; protected set; }
    public bool IsDead => HP <= 0;
    /// <summary> The entity's raw HP value plus the sum of the HP from every equipment it has currently equipped</summary>
    public float CalculatedHP => HP + equipment.Sum(e => e.HP);
    public int CalculatedMaxHP => stat_HP + equipment.Sum(e => e.MaxHP);
    public Team team { get; private set; }
    protected readonly List<Equipment> equipment = new();
    public abstract float PosX { get; }

    #region construction
    public Entity(Team team, float speed, int maxHP, float timeBetweenTicks)
    {
        stat_Speed = speed;
        stat_HP = maxHP;
        HP = stat_HP;
        stat_timeBetweenTicks = timeBetweenTicks;
        tickTimeLeft = stat_timeBetweenTicks;
        this.team = team;
    }

    protected void SetTimeBeforeNextTick(float time)
    {
        tickTimeLeft = time;
    }
    public void SetActor(EntityActor actor)
    {
        if (this.actor != null)
            Debug.LogWarning($"overriding {this.actor.name} with {actor.name}");
        this.actor = actor;
        OnActorSet();
    }

    protected virtual void OnActorSet()
    {

    }
    #endregion

    #region Update
    public void Update(float dt)
    {
        for (int i = effects.Count - 1; i >= 0; i--)
        {
            effects[i].Update(dt);
        }
        BehaviourPerFrame(dt * BehaviourSpeed);
        tickTimeLeft -= dt * BehaviourSpeed;
        if (tickTimeLeft < 0)
        {
            tickTimeLeft += stat_timeBetweenTicks;
            BehaviourPerTick();
        }
    }
    protected virtual void BehaviourPerFrame(float deltaTime)
    {

    }
    protected virtual void BehaviourPerTick()
    {

    }
    #endregion

    #region Status effects
    public void AddMovementSpeedModifier(float multiplier)
    {
        if (multiplier <= 0)
            FrozenMovementEffects++;
        else
            MovementSpeedMultiplier *= multiplier;
    }
    public void RemoveMovementSpeedModifier(float multiplier)
    {
        if (multiplier <= 0)
            FrozenMovementEffects--;
        else
            MovementSpeedMultiplier /= multiplier;
    }
    public void AddBehaviourSpeedModifier(float multiplier)
    {
        if (multiplier <= 0)
            PausedBehaviourEffects++;
        else
            BehaviourSpeedMultiplier *= multiplier;
    }
    public void RemoveBehaviourSpeedModifier(float multiplier)
    {
        if (multiplier <= 0)
            PausedBehaviourEffects--;
        else
            BehaviourSpeedMultiplier /= multiplier;
    }


    public void ApplyEffect(StatusEffect effect, float duration = -1)
    {
        OnEffectApplyAttempt(effect);
        if (!CanGetEffect(effect))
            return;
        OnEffectApplied(effect);
        StatusEffect currentEffect = GetEffect(effect);
        if (currentEffect != null)
            currentEffect.Reapply(duration);
        else
        {
            effects.Add(effect);
            effect.Apply(this, duration);
        }
    }
    public void RemoveEffect(StatusEffect effect)
    {
        if (!effects.Contains(effect))
            throw new InvalidOperationException($"tried calling {nameof(RemoveEffect)}, but the effect given is not in the list. Make sure you are referencing an exact match.");

        effects.Remove(effect);
        OnEffectRemoved(effect);
    }
    /// <summary>
    /// </summary>
    /// <typeparam name="Effect"></typeparam>
    /// <returns>The effect of the given type, applied to the entity</returns>
    public StatusEffect GetEffect<Effect>() where Effect : StatusEffect
    {
        foreach (var effect in effects)
            if (effect is Effect e)
                return e;

        return null;
    }
    private StatusEffect GetEffect(StatusEffect Effect)
    {
        foreach (var effect in effects)
            if (effect.GetType() == Effect.GetType())
                return effect;
        return null;
    }

    /// <summary>
    /// Called when an effect is trying to be applied to the entity.
    /// </summary>
    /// <remarks> Gets called BEFORE checking if the entity can receive such effect</remarks>
    /// <param name="effect">The effect trying to be applied to the entity</param>
    public virtual void OnEffectApplyAttempt(StatusEffect effect) { }
    /// <summary>
    /// Called when an effect has been succesfully applied to the entity.
    /// </summary>
    /// <remarks> Gets called AFTER checking if the entity can receive such effect</remarks>
    /// <param name="effect">The effect trying to be applied to the entity</param>
    public virtual void OnEffectApplied(StatusEffect effect) { }
    /// <summary>
    /// Called when an effect is removed from the entity, most of the time, but not always, by the effect itself (such as with expiration).
    /// </summary>
    /// <param name="effect">The effect trying to be removed to the entity</param>
    public virtual void OnEffectRemoved(StatusEffect effect) { }
    /// <summary>
    /// Should return weather the entity can be affected by the given effect
    /// </summary>
    /// <param name="effect">The effect trying to be applied to the entity</param>
    public virtual bool CanGetEffect(StatusEffect effect) { return true; }

    #endregion

    #region Damage & Death
    private void ApplyDamage(float amount)
    {
        HP -= amount;
        if (HP <= 0)
            ManageDeath();
    }
    public void Damage(DamageInfo info)
    {
        float damageRemaining = info.Amount;

        for(int i = 0; i<equipment.Count && damageRemaining > 0; i++)
        {
            damageRemaining = equipment[i].AbsorbDamage(new DamageInfo(damageRemaining, info.Source, info.Target));

            if (damageRemaining <= 0)
                return;
        }
        if (damageRemaining <= 0)
            return;

        ApplyDamage(damageRemaining);
        OnDamageTaken(new DamageInfo(damageRemaining, info.Source, info.Target));
    }
    /// <summary>
    /// Called after damage has been applied to this entity
    /// </summary>
    /// <remarks>
    /// This won't be called if the damage taken has been absorbed by its equipment
    /// </remarks>
    /// <param name="info"></param>
    protected virtual void OnDamageTaken(DamageInfo info) { }
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

    /// <summary>
    /// Should return true when the zombie can be hit by the given projectile
    /// </summary>
    /// <remarks>
    /// Note: While Lawnmowers are a type of Projectile, they wont be passed trough this function.<br></br>
    /// If you wish to make a zombie immune to lawn mowers, add ZombieTag.ImmuneToLawnMowers to its list of tags
    /// </remarks>
    /// <param name="projectile"></param>
    /// <returns></returns>
    protected virtual bool CanBeHitByProjectile(Projectile projectile)
    {
        return true;
    }
    public virtual bool CanBeHit(Projectile projectile)
    {
        return CanBeHitByProjectile(projectile);
    }
    #endregion

    #region Visuals
    /// <summary>
    /// Updates the actor's sprite to the one from its sprites list, at the desired index
    /// </summary>
    /// <param name="SpriteIndex">The sprite will be chosen from the list based on this Index</param>
    protected void SetSprite(int SpriteIndex)
    {
        actor.UpdateVisualState(SpriteIndex);
    }
    #endregion

    #region Teams

    public void SetTeam(Team value)
    {
        Team oldTeam = team;
        team = value;

        if (oldTeam != team)
            OnTeamChange(oldTeam, team);
    }
    public void SwitchTeams() 
    {
        if (team == Team.Plants)
            SetTeam(Team.Zombies);

        else if (team == Team.Zombies)
            SetTeam(Team.Plants);

        else
            Debug.LogWarning($"Switching teams from team {team} is undefined. Use {nameof(SetTeam)}({nameof(Team)}) or define switching in {nameof(Entity)}.{nameof(SwitchTeams)}()");
    }
    protected virtual void OnTeamChange(Team oldTeam, Team newTeam) { }

    public bool IsAlly(Entity entity)
    {
        return entity.team == this.team;
    }

    public virtual bool IsEnemy(Entity entity) { return !IsAlly(entity); }

    public virtual bool CanTarget(Entity entity) { return true; }


    public bool IsValidTeamTarget(Entity entity, TeamTarget teamTarget)
    {
        return teamTarget switch
        {
            TeamTarget.Enemy => IsEnemy(entity),
            TeamTarget.Ally => IsAlly(entity),
            TeamTarget.All => true,
            _ => false
        };
    }
    protected IEnumerable<T> FilterTargets<T>(IEnumerable<T> entities, TeamTarget teamTarget = TeamTarget.Enemy) where T : Entity
    {
        foreach (T entity in entities)
            if (IsValidTeamTarget(entity, teamTarget))
                yield return entity;
    }

    #endregion

    #region equipment

    protected void GiveEquipment(Equipment item)
    {
        equipment.Add(item);
        item.Equip(this);
    }
    public void RemoveEquipment(Equipment item)
    {
        equipment.Remove(item);
        item.OnRemoved();
        OnEquipmentRemoved(item);
    }
    protected virtual void OnEquipmentRemoved(Equipment item) { }

    #endregion

    #region projectiles
    protected virtual void ShootProjectile(ProjectileType type, ActorFace face, int col, float xPos)
    {
        (float col, float row) origin = EntityActor.AdjustWorldPosWithFace((col, xPos), face);
        GameManager.Instance.levelManager.SpawnProjectile(this, type, origin.col, origin.row);
    }

    #endregion
}