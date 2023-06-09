using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RessistanceEffect : Effect
{
    [Range(0,1), Tooltip("0 means no damage, 1 is all damage. 0.5 is half the damage")]
    public float resistance;

    public void Awake()
    {
        effectTypes = EffectType.RESISTANCE;
    }

    public override void CopyFrom(Effect original)
    {
        base.CopyFrom(original);
        RessistanceEffect orig = (RessistanceEffect)original;
        resistance = orig.resistance;
    }

    public override float ApplyResistanceEffect(float incDamage)
    {
        float resistDamage = incDamage * (resistance * strength);
        return resistDamage;
    }


}
