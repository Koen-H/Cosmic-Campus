using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineerAbility : Ability
{
    public override void Activate(Vector3 origin, Vector3 direction)
    {
        if (!player.IsOwner) return;
        //base.Activate(origin, direction);
        Ray ray = new Ray(origin, direction);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) ServerSpawner.Instance.SpawnRemoteEngineerPrefabServerRpc(hit.point);
        else return;

        player.LockPlayer(true);//Disable the player interactions.


        //GameObject target = GetTarget(origin, direction);
        //target.transform.forward = player.playerObj.transform.forward;
        //Walker walker = target.AddComponent<Walker>();
        //walker.owner = player;
    }
}
