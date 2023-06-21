using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private bool attached = false;
    private Transform attachedObject;
    public float damage = 0;
    public Enemy enemy;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        if (attached) return;
        if (other.tag == "Player")
        {
            //if (enemy.IsOwner)
                other.transform.GetComponent<PlayerCharacterController>().TakeDamage(damage);
            Destroy(this.gameObject);
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
        if (to)
        {
            Debug.Log("to is not null");
            to.TakeDamage(damage);
        }
    }
}
