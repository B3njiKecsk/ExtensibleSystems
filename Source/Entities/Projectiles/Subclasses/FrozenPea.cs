using UnityEngine;

public sealed class FrozenPea : Projectile
{
    public FrozenPea(float xPos, float yPos) : base(xPos, yPos, stats)
    {
    }

    public static readonly ProjectileStats stats = new ProjectileStats
    (
        displayName: "Snowpea's frozen pea",
        damage: 20,
        speed: 5f
    );

    protected override void MovementBehaviour(float distance)
    {
        base.MovementBehaviour(distance);
        MoveRight(distance);
    }

    protected override void OnHit(Entity target)
    {
        base.OnHit(target);
        target.ApplyEffect(new SlowedEffect(), 8f);
    }
}
