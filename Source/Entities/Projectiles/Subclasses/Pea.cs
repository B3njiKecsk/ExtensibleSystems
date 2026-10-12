
public sealed class Pea : Projectile
{
    public Pea(float xPos, float yPos) : base(xPos, yPos, stats)
    {
    }

    public static readonly ProjectileStats stats = new ProjectileStats
    (
        displayName: "Peashooter's pea",
        damage: 20,
        speed: 5f
    );

    protected override void MovementBehaviour(float distance)
    {
        base.MovementBehaviour(distance);
        MoveRight(distance);
    }
}
