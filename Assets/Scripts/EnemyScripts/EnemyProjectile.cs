using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private bool attached = false;
    private Transform attachedObject;
    public float damage = 0;
    public Enemy enemy;

    [SerializeField] private AudioClip playerHit;
    [SerializeField] private SFXPlayer sfxPlayer;

    private void OnTriggerEnter(Collider other)
    {
        if (attached) return;
        if (other.tag == "Player")
        {
                other.transform.GetComponent<PlayerCharacterController>().TakeDamage(damage);
            Instantiate(sfxPlayer,transform.position,Quaternion.identity).audioClip= playerHit;
            Destroy(this.gameObject);
        }
        attached = true;
        attachedObject = other.gameObject.transform;
        this.transform.parent = attachedObject;
        GetComponent<Rigidbody>().isKinematic = true;
        Destroy(GetComponent<Collider>());
        StartCoroutine(KillAfterDuration(10));
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
    IEnumerator KillAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }
}
