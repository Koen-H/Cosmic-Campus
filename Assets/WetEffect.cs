using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WetEffect : Effect
{
    public float slowness;

    public void Awake()
    {
        effectTypes = EffectType.MOVEMENT;
    }

    public override void CopyFrom(Effect original)
    {
        base.CopyFrom(original);
        WetEffect orig = (WetEffect)original;
        slowness = orig.slowness;
    }

    public override float ApplyMovementEffect(float movementSpeed)
    {
        movementSpeed = movementSpeed * (slowness * strength);
        return movementSpeed;
    }


}
