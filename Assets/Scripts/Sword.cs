using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : Weapon
{

    /// <summary>
    /// When the player starts with the input
    /// </summary>
    public override void OnAttackInputStart()
    {
        Aim();
        playerController.AttackServerRpc();
        Attack();
        playerController.ToggleMovement(false);
    }
    /// <summary>
    /// While the player is holding the input
    /// </summary>
    public override void OnAttackInputHold()
    {
        Aim();
    }
    /// <summary>
    /// When the player lets go of the input
    /// </summary>
    public override void OnAttackInputStop()
    {
        Aim();
        playerController.ToggleMovement(true);
    }

    /// <summary>
    /// For when the enemy stuns/cancels the player during the attack.
    /// </summary>
    public override void CancelAttack()
    {

    }

    public override void Attack()
    {
        base.Attack();


        // calculate raycast direction
        Vector3 rayDirection = weaponObj.transform.TransformDirection(Vector3.forward);

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
                if(playerController.IsOwner)DealDamage(hit.transform.gameObject);
            }
        }
    }
    void DealDamage(GameObject enemy)
    {
        enemy.transform.parent.GetComponent<Enemy>().TakeDamage(weaponData.damage);
    }
}
