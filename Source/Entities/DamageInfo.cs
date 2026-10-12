using System;
using UnityEngine;

public readonly struct DamageInfo 
{
    public enum Direction
    {
        left,
        right,
        below,
        above,
        side,
        directionless
    }
    public float Amount { get; }
    public Entity Source { get; }
    public Entity Target { get; }
    public Direction direction { get; }

    public DamageInfo(float amount, Entity source, Entity target)
    {
        Amount = amount;
        Source = source;
        Target = target;
        direction = CalculateSouceDir(source, target);
    }
    public DamageInfo(float amount, Entity source, Entity target, Direction sourceDirection)
    {
        Amount = amount;
        Source = source;
        Target = target;
        direction = sourceDirection;
    }

    private static Direction CalculateSouceDir(Entity source, Entity target)
    {
        if (source.PosX < target.PosX)
            return Direction.left;

        if (source.PosX > target.PosX)
            return Direction.right;

        return Direction.side;
    }
}
