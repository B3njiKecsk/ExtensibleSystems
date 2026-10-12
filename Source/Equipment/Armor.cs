using System;
using UnityEngine;

public class Armor : Equipment
{
    private DamageInfo.Direction shieldDir;
    public Armor(int hp, DamageInfo.Direction protectsFromDirection = DamageInfo.Direction.directionless) : base(hp)
    {
        shieldDir = protectsFromDirection;
    }

    protected override bool ProtectAgainst(DamageInfo info)
    {
        if (shieldDir == DamageInfo.Direction.directionless)
            return true;    //always absorb

        return info.direction == shieldDir;
    }

    public void FlipDirection()
    {
        if (shieldDir != DamageInfo.Direction.left && shieldDir != DamageInfo.Direction.right)
            return;

        shieldDir = shieldDir == DamageInfo.Direction.left? DamageInfo.Direction.right : DamageInfo.Direction.left;
    }
}
