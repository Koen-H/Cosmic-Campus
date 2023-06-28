using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class DesignerAbility : Ability
{
    //public float offset = 5;
    //public float distFromGround = 3;
    //public float maxScale = 30;
    //public float minScale = 0.2f;

    private GameObject target;
    public LayerMask groundLayer;
    private float scaleAmount = 1;

    Vector3 offset = new Vector3(0,2.5f,2);

    protected override void Awake()
    {
        base.Awake();
        cooldown = 5;
    }


    public override void Activate(Vector3 origin, Vector3 direction)
    {
        //base.Activate(origin,direction); //there was a reason why i didnt have base in here
        if (player.usingCart.Value) return;
        if (onCooldown) return;

        Ray ray = new Ray(origin, direction);
        LayerMask layerMask = ~(LayerMask.GetMask("Decal") | LayerMask.GetMask("Enemy") | LayerMask.GetMask("Area") | LayerMask.GetMask("Player") | LayerMask.GetMask("UI"));
        RaycastHit hit;
        GameObject target = null;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)) target = hit.collider.gameObject;
        if (target == null) return;

        if (player.IsOwner)
        {
            ServerSpawner.Instance.SpawnDesignerObjectServerRpc(hit.point);
            StartCoroutine(Cooldown(cooldown));
        }
    }
}
