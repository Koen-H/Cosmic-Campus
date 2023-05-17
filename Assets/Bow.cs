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
        StartCharge();
        playerController.ToggleMovement(false);
    }
    /// <summary>
    /// While the player is holding the input
    /// </summary>
    public override void OnAttackInputHold()
    {
        Aim();
        playerController.ToggleMovement(false);
    }
    /// <summary>
    /// When the player lets go of the input
    /// </summary>
    public override void OnAttackInputStop()
    {
        Aim();
        float chargeLevel =  Mathf.Clamp01((Time.time - chargeStartTime) / maxChargeTime);
        chargeLevel = Mathf.Lerp(weaponData.minProjectileSpeed, weaponData.maxProjectileSpeed, chargeLevel);
        if (isCharging)
        {
            ShootArrowServerRpc(chargeLevel, weaponData.damage);
            isCharging = false;
        }
        playerController.ToggleMovement(true);
    }




    private void StartCharge()
    {
        isCharging = true;
        chargeStartTime = Time.time;
        Debug.Log("starting"); 
    }

    [ServerRpc]
    private void ShootArrowServerRpc(float chargeLevel, float damage)
    {
        GameObject arrow = Instantiate(weaponData.projectilePrefab, weaponObj.transform.position, weaponObj.transform.rotation);
        arrow.GetComponent<NetworkObject>().Spawn();
        arrow.GetComponent<Rigidbody>().AddForce(weaponObj.transform.forward * chargeLevel);
        arrow.GetComponent<ArrowManager>().damage = damage;
    }
}
