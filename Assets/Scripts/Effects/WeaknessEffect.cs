using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaknessEffect : Effect
{
    [Range(1,2), Tooltip("1 means no extra damage, 2 is double damage. 0.5 is half the damage")]
    public float resistance;

    public void Awake()
    {
        effectTypes = EffectType.RESISTANCE;
    }

    public override void CopyFrom(Effect original)
    {
        base.CopyFrom(original);
        WeaknessEffect orig = (WeaknessEffect)original;
        resistance = orig.resistance;
    }

    public override float ApplyResistanceEffect(float incDamage)
    {
        float resistDamage = incDamage * (resistance * strength);
        return resistDamage;
    }


}
