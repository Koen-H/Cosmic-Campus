using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineerAbility : Ability
{


    //TODO: Make Ability good with playerchar so I can remove update in here!
    public void Update()
    {
        if (!player.IsOwner) return;
        if (Input.GetMouseButtonDown(1))  // 1 is the right mouse button
        {
            if (!player.canAbility) return;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            player.ActivateServerRpc(ray.origin, ray.direction);
            Activate(ray.origin, ray.direction);
        }
    }

    public override void Activate(Vector3 origin, Vector3 direction)
    {
        if (!player.IsOwner) return;
        //base.Activate(origin, direction);
        Ray ray = new Ray(origin, direction);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) ServerSpawner.Instance.SpawnRemoteEngineerPrefabServerRpc(hit.point);
        else return;
        player.AttackStopServerRpc();
        player.engineering = true;
        player.LockPlayer(true);//Disable the player interactions.


        //GameObject target = GetTarget(origin, direction);
        //target.transform.forward = player.playerObj.transform.forward;
        //Walker walker = target.AddComponent<Walker>();
        //walker.owner = player;
    }
}
