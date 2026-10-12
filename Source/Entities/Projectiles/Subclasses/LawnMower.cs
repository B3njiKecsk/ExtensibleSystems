using UnityEngine;

public sealed class LawnMower : Projectile
{
    public LawnMower(float xPos, float yPos) : base(xPos, yPos, stats)
    {
    }

    public static readonly ProjectileStats stats = new ProjectileStats
    (
        displayName: "Lawn mower",
        damage: 10000,
        speed: 5f
    );

    private bool triggered;
    protected override void MovementBehaviour(float distance)
    {
        if (!triggered)
            return;

        base.MovementBehaviour(distance);
        MoveRight(distance);
    }

    protected override void OnHit(Entity target)
    {
        base.OnHit(target);
        triggered = true;
        GameManager.Instance.levelManager.TriggeredMower(lane);
        KillEntity(target);
    }
}
