using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Bow : Weapon    
{
    public float maxChargeTime = 2f; // Maximum time to charge the bow

    private bool isCharging;
    private float chargeStartTime;

    /// <summary>
    /// When the player starts with the input
    /// </summary>
    public override void OnAttackInputStart()
    {
        if (weaponState != WeaponState.READY) return;
        playerController.ToggleMovement(false);
        playerController.AttackStartServerRpc();
        AttackStart();
    }
    /// <summary>
    /// While the player is holding the input
    /// </summary>
    public override void OnAttackInputHold()
    {
        if (weaponState == WeaponState.READY)
        {
            playerController.ToggleMovement(false);
            playerController.AttackStartServerRpc();
            AttackStart();
        }
        if (weaponState != WeaponState.HOLD) return;
        Aim();
        playerController.ToggleMovement(false);
    }
    /// <summary>
    /// When the player lets go of the input
    /// </summary>
    public override void OnAttackInputStop()
    {
        if (weaponState != WeaponState.HOLD) return;
        Aim();
        if (isCharging)
        {
            playerController.AttackServerRpc();
            Attack();
        }
        playerController.ToggleMovement(true);
    }

    public override void Attack()
    {
        
        ShootArrow();
        base.Attack();
    }
    
    public override void AttackStart()
    {
        StartCharge();
    }
    private void StartCharge()
    {
        weaponState = WeaponState.HOLD;
        isCharging = true;
        chargeStartTime = Time.time;
    }


    private void ShootArrow()
    {
        float chargeLevel = Mathf.Clamp01((Time.time - chargeStartTime) / maxChargeTime);
        chargeLevel = Mathf.Lerp(weaponData.minProjectileSpeed, weaponData.maxProjectileSpeed, chargeLevel);
        isCharging = false;

        GameObject arrow = Instantiate(weaponData.projectilePrefab, weaponObj.transform.position, weaponObj.transform.rotation);
        arrow.GetComponent<Rigidbody>().AddForce(weaponObj.transform.forward * chargeLevel);
        ArrowManager arrowManager = arrow.GetComponent<ArrowManager>();
        arrowManager.damage = weaponData.damage.GetRandomValue();
        arrowManager.playerController = playerController;
    }
}
