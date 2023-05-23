using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSlamManager : MonoBehaviour
{
    public PlayerCharacterController owner;
    Rigidbody rb;
    private bool hasFallen = false;
    float raycastDistance = 2;
    float damage = 1;

    private void Awake()
    {
        if (!TryGetComponent(out rb)) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        raycastDistance *= transform.lossyScale.z;
        //TODO: Make damage better/correct?
        damage += transform.lossyScale.z;
    }
    private void Update()
    {
        rb.velocity += Vector3.down * transform.lossyScale.y;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!hasFallen)
        {
            hasFallen = true;
            GroundSlam();
        }
    }
    private void GroundSlam()
    {
        //Do fancy particle stuff
        //If the current client is the owner, we deal the damage
        if (owner.IsOwner)
        {
            List<GameObject> enemiesHit = new List<GameObject>();
            for (int i = 0; i < 360; i += 10)
            {
                Vector3 direction = Quaternion.Euler(0f, i, 0f) * transform.forward;
                Ray ray = new Ray(transform.position, direction);
                RaycastHit hit;
                Debug.DrawRay(transform.position, direction * raycastDistance, Color.red, 5f);

                if (Physics.Raycast(ray, out hit, raycastDistance))
                {
                    if (hit.collider.CompareTag("Enemy"))
                    {
                        if (!enemiesHit.Contains(hit.collider.gameObject)) enemiesHit.Add(hit.collider.gameObject);
                    }
                }
            }

            foreach (GameObject enemyObj in enemiesHit)
            {
                enemyObj.GetComponentInParent<Enemy>().TakeDamage(damage);
                Debug.Log($"Did {damage} damage!");
            }
        }
        Destroy(this);
    }

}
