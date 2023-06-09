using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurningEffect : Effect
{
    [Tooltip("How much percentage it should burn per second")]
    public float burnPercentage;

    public void Awake()
    {
    }

    public override void CopyFrom(Effect original)
    {
        base.CopyFrom(original);
        BurningEffect orig = (BurningEffect)original;
        burnPercentage = orig.burnPercentage;
    }

    public override void EverySecond()
    {
        if (manager.isEnemy) manager.enemy.TakeDamage(burnPercentage * strength, EnemyType.ARTIST, true);
        else manager.player.TakeDamage(burnPercentage * strength, true);
    }
}
