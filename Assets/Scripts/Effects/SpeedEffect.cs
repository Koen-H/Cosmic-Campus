using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedEffect : Effect
{
    public float speed;

    public void Awake()
    {
        effectTypes = EffectType.MOVEMENT;
    }

    public override void CopyFrom(Effect original)
    {
        base.CopyFrom(original);
        SpeedEffect orig = (SpeedEffect)original;
        speed = orig.speed;
    }

    public override float ApplyMovementEffect(float movementSpeed)
    {
        movementSpeed = movementSpeed * (speed * strength);
        return movementSpeed;
    }


}
