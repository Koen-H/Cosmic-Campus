using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ArrowManager : NetworkBehaviour
{
    private bool attached = false;
    private Transform attachedObject;
    public float damage = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner || attached) return;
        if(other.tag == "Enemy")
        {
            other.transform.parent.GetComponent<Enemy>().TakeDamage(damage);
            Destroy(gameObject);
        }
        if (other.gameObject.TryGetComponent<Rigidbody>(out Rigidbody rig))
        {
            Destroy(gameObject);
            return;
        }
        attached = true;
        attachedObject = other.gameObject.transform;
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
