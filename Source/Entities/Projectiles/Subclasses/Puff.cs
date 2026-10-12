using UnityEngine;

public class Puff : Projectile
{
    public Puff(float xPos, float yPos) : base(xPos, yPos, stats)
    {
    }

    public static readonly ProjectileStats stats = new ProjectileStats
    (
        displayName: "Puff-shroom's puff",
        damage: 20,
        speed: 5f,
        range: 3f
    );

    protected override void MovementBehaviour(float distance)
    {
        base.MovementBehaviour(distance);
        MoveRight(distance);
    }
}
