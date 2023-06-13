using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingEffect : Effect
{
    [Tooltip("How much percentage it should heal per second")]
    public float healPercentage;

    public void Awake()
    {
    }

    public override void CopyFrom(Effect original)
    {
        base.CopyFrom(original);
        HealingEffect orig = (HealingEffect)original;
        healPercentage = orig.healPercentage;
    }

    public override void EverySecond()
    {
        if (manager.isEnemy) manager.enemy.Heal(healPercentage * strength);
        else manager.player.HealPlayerClientRPC(healPercentage * strength);
    }
}
