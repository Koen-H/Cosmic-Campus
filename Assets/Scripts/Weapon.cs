using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    internal WeaponData weaponData;

    private bool canAttack = true;
    internal PlayerCharacterController playerController;
    internal GameObject weaponObj;//The object that exists

    protected Animator weaponAnimation;
    protected BoxCollider weaponCollider; 

    protected WeaponState weaponState = WeaponState.READY;

    private void Awake()
    {
        playerController = GetComponent<PlayerCharacterController>();


    }


    /// <summary>
    /// When the player starts with the input
    /// </summary>
    public virtual void OnAttackInputStart()
    {
        if (weaponState != WeaponState.READY) return;
        Aim();
    }
    /// <summary>
    /// While the player is holding the input
    /// </summary>
    public virtual void OnAttackInputHold()
    {
        Aim();
    }
    /// <summary>
    /// When the player lets go of the input
    /// </summary>
    public virtual void OnAttackInputStop()
    {
        Aim();
    }

    /// <summary>
    /// For when the enemy stuns/cancels the player during the attack.
    /// </summary>
    public virtual void CancelAttack()
    {

    }

    /// <summary>
    /// Aim rotates the player towards where the mouse clicks.
    /// </summary>
    internal void Aim()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 clickPoint = hit.point;
            Transform playerObj = playerController.playerObj.transform;
            playerObj.LookAt(new Vector3(clickPoint.x, playerObj.position.y, clickPoint.z));
        }
    }

    public virtual void AttackStart()
    {

    }

    public virtual void Attack()
    {
        canAttack = false;
        weaponState= WeaponState.COOLDOWN;
        StartCoroutine(Cooldown(weaponData.cooldown));
    }

    IEnumerator Cooldown(float time)
    {
        yield return new WaitForSeconds(time);
        canAttack = true;
        weaponState = WeaponState.READY;
    }


}

public enum WeaponType { UNSET, SWORD, BOW, STAFF }
public enum WeaponState { COOLDOWN, READY, START, HOLD, STOP, DISABLED}