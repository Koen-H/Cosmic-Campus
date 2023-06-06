using System.Collections;
using System.Collections.Generic;
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

    public override void Activate(Vector3 origin, Vector3 direction)
    {
        //base.Activate(origin,direction); //there was a reason why i didnt have base in here
        if (!canUse) return;

        canUse = false;
        target = GetTarget(origin,direction);
        target.transform.parent = player.playerObj.transform;
        target.transform.localPosition = new Vector3(offset.x, offset.y + target.transform.localScale.y/2, offset.z + target.transform.localScale.z);
        
        if (target.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
    }

    private void Update()
    {
        // Leave Empty
    }

    public void PutDown(Vector3 clickPoint)
    {
        player.playerObj.transform.LookAt(new Vector3(clickPoint.x,transform.position.y , clickPoint.z));
        ObjectSlamManager objectSlamManager = target.AddComponent<ObjectSlamManager>();
        objectSlamManager.owner = player;
        Debug.Log("put down");
        // Place target on the ground
        target.transform.parent = null;
        target.transform.position = new Vector3(clickPoint.x, target.transform.position.y, clickPoint.z);
        //target.transform.position = new Vector3(
        //    target.transform.position.x,
        //    target.transform.localScale.y / 2,
        //    target.transform.position.z
        //);
        canUse = true;
        target = null;
    }

    IEnumerator Cooldown(float time)
    {
        yield return new WaitForSeconds(time);
        canUse = true;
    }
}
