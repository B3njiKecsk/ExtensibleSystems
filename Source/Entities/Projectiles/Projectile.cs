using System;
using UnityEngine;

public abstract class Projectile: Entity
{
    const float OOBDist = 20f;
    protected ProjectileStats base_stats { get; private set; }
    public ProjectileStats Stats => base_stats;
    private float xPos;
    private float yPos;
    private float distanceTravelled;
    public sealed override float PosX => xPos;
    public Projectile(float xPos, float yPos, ProjectileStats stats): base(Team.Plants, stats.Speed, stats.MaxHP, stats.TimeBetweenTicks)
    {
        this.xPos = xPos;
        this.yPos = yPos;
        base_stats = stats;
    }
    protected override void BehaviourPerFrame(float deltaTime)
    {
        Move(deltaTime*base_stats.Speed);
        CheckCollisions();
    }
    private void Move(float distance)
    {
        MovementBehaviour(distance);
        
        OnDistanceTravelled(distanceTravelled);
        actor.UpdatePosition(xPos, yPos);
    }
    private void CheckCollisions()
    {
        Entity hit = (Entity)GameManager.Instance.levelManager.ProjectileHitZombie(this, (int)yPos, xPos) ?? GameManager.Instance.levelManager.ProjectileHitPlant(this, (int)yPos, (int)xPos);
        if (hit != null)
            OnHit(hit);
    }
    protected int lane => (int)yPos;
    protected virtual void MovementBehaviour(float distance)
    {
        distanceTravelled += distance;
    }
    protected void MoveRight(float distance)
    {
        xPos = xPos + distance;
        CheckIfInBounds();
    }

    protected void MoveUp(float distance)
    {
        yPos = yPos + distance;
        CheckIfInBounds();
    }

    protected void MoveTowardsZombie(float distance, Zombie zombie)
    {
        float dx = zombie.xPos - xPos;
        float dy = zombie.row - yPos;

        double length = Math.Sqrt(dx * dx + dy * dy);

        if (length > 0f)
        {
            float scale = (float) (distance / length);
            xPos += dx * scale;
            yPos += dy * scale;
        }
        CheckIfInBounds();
    }

    private void MoveAt(float distance, float angle)
    {
        CheckIfInBounds();
    }
    protected override sealed void DestroySelf()
    {
        GameManager.Instance.levelManager.RemoveEntity(this);
    }

    private void CheckIfInBounds()
    {
        if (xPos < -OOBDist || xPos > OOBDist || yPos < -OOBDist || yPos > OOBDist)
            OutOfBounds();
    }
    private void OutOfBounds()
    {
        Suicide();
    }

    protected virtual void OnDistanceTravelled(float tiles)
    {
        if (base_stats.Range >= 0)
            if (tiles >= base_stats.Range)
                Suicide();
    }

    protected virtual void OnHit(Entity target)
    {
        target.Damage(new DamageInfo(base_stats.Damage, this, target));
        Suicide();
    }

    protected void KillEntity(Entity z)
    {
        z.InstaKill(new DamageInfo(Stats.Damage, this, z));
    }

    public sealed override bool CanBeHit(Projectile projectile)
    {
        return CanBeHitByProjectile(projectile);
    }

    protected override bool CanBeHitByProjectile(Projectile projectile)
    {
        return false;
    }
}
