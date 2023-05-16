using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : Weapon
{
    public override void Attack()
    {
        base.Attack();


        // calculate raycast direction
        Vector3 rayDirection = transform.TransformDirection(Vector3.forward);

        Debug.DrawRay(transform.position + Vector3.up, rayDirection, Color.red, 0.01f);
        // initialize a variable to store the hit information
        RaycastHit hit;

        // shoot the raycast
        if (Physics.Raycast(transform.position + Vector3.up, rayDirection, out hit, 1))
        {
            // check if the object hit has the tag "Enemy"
            if (hit.transform.CompareTag("Enemy"))
            {
                // call DealDamage function
                DealDamage(hit.transform.gameObject);
            }
        }
    }
    void DealDamage(GameObject enemy)
    {
        enemy.transform.parent.GetComponent<Enemy>().TakeDamage(damage);
    }
}
