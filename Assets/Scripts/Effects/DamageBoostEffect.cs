using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageBoostEffect : Effect
{
    [Range(1,2), Tooltip("1 means no extra damage, 2 is double damage. 0.5 is half the damage")]
    public float increase;

    public void Awake()
    {
        effectTypes = EffectType.ATTACK;
    }

    public override void CopyFrom(Effect original)
    {
        base.CopyFrom(original);
        DamageBoostEffect orig = (DamageBoostEffect)original;
        increase = orig.increase;
    }

    public override float ApplyAttackEffect(float damage)
    {
        float newDamage = damage * (increase * strength);
        return newDamage;
    }


}
