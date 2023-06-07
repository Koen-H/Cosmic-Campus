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
    public RangeEnemy rangeEnemy; 

    private void OnTriggerEnter(Collider other)
    {
        if (rangeEnemy)
        {
            Debug.Log("range Enemy is not null");
            PlayerCharacterController player = other.transform.GetComponent<PlayerCharacterController>();
            Debug.Log("Player : " + player + "  : name =  " + other.gameObject.name);
            DealDamage(damage, player);
            return;
        }
        if (!playerController) return;
        if (attached) return;
        if(other.tag == "Enemy")
        {
            if(playerController.IsOwner)
            other.transform.parent.GetComponent<Enemy>().TakeDamage(damage, GetComponent<Rigidbody>().velocity/15);
        }
        attached = true;
        attachedObject = other.gameObject.transform;
        this.transform.parent = attachedObject;
        GetComponent<Rigidbody>().isKinematic = true;
        Destroy(GetComponent<Collider>());
        StartCoroutine(KillAfterDuration(10)); 
    }

    IEnumerator KillAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject); 
    }


    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision! As OnCollisionEnter");
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

    void DealDamage(float damage, PlayerCharacterController to)
    {
        if(to)
        {
            Debug.Log("to is not null");
            to.TakeDamage(damage);
        }
    }

}
