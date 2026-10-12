using UnityEngine;

public class LongPuff : Projectile
{
    public LongPuff(float xPos, float yPos) : base(xPos, yPos, stats)
    {
    }

    public static readonly ProjectileStats stats = new ProjectileStats
    (
        displayName: "Scaredy-shroom's puff",
        damage: 20,
        speed: 5f
    );

    protected override void MovementBehaviour(float distance)
    {
        base.MovementBehaviour(distance);
        MoveRight(distance);
    }
}
