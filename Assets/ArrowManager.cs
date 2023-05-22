using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ArrowManager : MonoBehaviour
{
    private bool attached = false;
    private Transform attachedObject;
    public float damage = 0;
    public PlayerCharacterController playerController;

    private void OnTriggerEnter(Collider other)
    {
        if (attached) return;
        if(other.tag == "Enemy")
        {
            if(playerController.IsOwner)
            other.transform.parent.GetComponent<Enemy>().TakeDamage(damage);
        }
        attached = true;
        attachedObject = other.gameObject.transform;
        this.transform.parent = attachedObject;
        GetComponent<Rigidbody>().isKinematic = true;
    }
    private void Update()
    {
        if (attached)
        {
            if (attachedObject == null)//Destroy if the object is gone.
            {
                Destroy(gameObject);
                return;
            }

        }
    }
}
