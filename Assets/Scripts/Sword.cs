using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : Weapon
{

    private void Start()
    {
        GetComponentInChildren<SwordCollider>().swrod = this;
    }
    /// <summary>
    /// When the player starts with the input
    /// </summary>
    public override void OnAttackInputStart()
    {
        if (weaponState != WeaponState.READY) return;
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
        if (weaponState != WeaponState.READY) return;
        playerController.AttackServerRpc();
        Attack();
        playerController.ToggleMovement(false);
    }
    /// <summary>
    /// When the player lets go of the input
    /// </summary>
    public override void OnAttackInputStop()
    {
        //Aim();
        //playerController.ToggleMovement(true);
    }

    /// <summary>
    /// For when the enemy stuns/cancels the player during the attack.
    /// </summary>
    public override void CancelAttack()
    {
        AfterAttack();
    }

    public override void Attack()
    {
        base.Attack();

        weaponAnimation = GetComponentInChildren<Animator>();
        weaponAnimation.SetTrigger("Animate");

        StartCoroutine(AfterAnim(weaponAnimation.GetCurrentAnimatorStateInfo(0).length));
            
/*        // calculate raycast direction
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
        }*/
    }

/*    private void weaponCollider.OnTriggerEnter(Collider other)
    {
        if (weaponAnimation.GetCurrentAnimatorStateInfo(0).IsName("SwordSwing"))
        {
            DealDamage(other.transform.gameObject);
        }
    }*/
    public void DealDamage(GameObject enemyObject)
    {
        if (!weaponAnimation) return;
        if (weaponAnimation.GetCurrentAnimatorStateInfo(0).IsName("SwordSwing"))
        {
            Enemy enemy = enemyObject.transform.parent.GetComponent<Enemy>();
            Debug.Log(enemy);
            if (enemy == null) return;
            enemy.TakeDamage(weaponData.damage);
        }
    }

    IEnumerator AfterAnim(float duration)
    {
        Debug.Log("anim started with duration of " + duration);
        yield return new WaitForSeconds(duration);
        Debug.Log("anim ended");
        AfterAttack();
    }
}
