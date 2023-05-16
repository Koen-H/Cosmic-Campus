using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    private int weaponID;
    [SerializeField] protected int damage;

    [SerializeField] private float cooldown;
    private bool canAttack = true;
    internal PlayerCharacterController playerController;

    private void Awake()
    {
        playerController = transform.parent.transform.parent.transform.parent.GetComponent<PlayerCharacterController>();
    }


    /// <summary>
    /// When the player starts with the input
    /// </summary>
    public virtual void OnAttackInputStart()
    {

    }
    /// <summary>
    /// While the player is holding the input
    /// </summary>
    public virtual void OnAttackInputHold()
    {

    }
    /// <summary>
    /// When the player lets go of the input
    /// </summary>
    public virtual void OnAttackInputStop()
    {

    }

    /// <summary>
    /// For when the enemy stuns/cancels the player during the attack.
    /// </summary>
    public virtual void CancelAttack()
    {

    }



    public virtual void Attack()
    {
        canAttack = false;
        StartCoroutine(Cooldown(cooldown));
    }

    IEnumerator Cooldown(float time)
    {
        yield return new WaitForSeconds(time);
        canAttack = true;
    }


}
