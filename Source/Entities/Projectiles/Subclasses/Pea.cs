
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
    Entity pierced = null;
    protected override void OnHit(Entity target)
    {
        pierced ??= target;
        if(target != pierced)
            base.OnHit(target);
    }
}
