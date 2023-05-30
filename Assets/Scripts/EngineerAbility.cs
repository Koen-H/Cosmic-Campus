using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineerAbility : Ability
{
    public override void Activate(Vector3 origin, Vector3 direction)
    {
        base.Activate(origin,  direction);

        GameObject target = GetTarget(origin, direction);

        target.transform.forward = player.playerObj.transform.forward;
        Walker walker = target.AddComponent<Walker>();
        walker.owner = player;
    }
}
