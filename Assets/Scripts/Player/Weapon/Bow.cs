using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Bow : Weapon    
{
    public float maxChargeTime = 2f; // Maximum time to charge the bow

    private bool isCharging;
    private float chargeStartTime;
    [SerializeField] Vector3 TESTING; 


    private void Start()
    {
        weaponObj.transform.parent = FindDeepChild(this.transform, "Tool_bone");
        ResetChildTransforms(weaponObj.transform, 2);
        weaponAnimation = GetComponentInChildren<Animator>();
    }
    private void Update()
    {
        ResetChildTransforms(weaponObj.transform, 2);
    }

    public void ResetChildTransforms(Transform parentTransform, int depth)
    {
        // Reset the local position, rotation, and scale of the parent first
        parentTransform.localPosition = Vector3.zero;
        //Vector3(10.699996, 169, 58.6)
        parentTransform.localRotation = Quaternion.Euler(-119,0,0);// Set(169, -11, -121,0);//   new Vector3(10.699996f, 169f, 58.6f); Quaternion.EulerAngles(196 * Mathf.Deg2Rad, (-83 + 20) * Mathf.Deg2Rad, (-156 -90) * Mathf.Deg2Rad);
        //parentTransform.localScale = Vector3.one;
    }
    /// <summary>
    /// When the player starts with the input
    /// </summary>
    public override void OnAttackInputStart()
    {
        if (weaponState != WeaponState.READY) return;
        base.OnAttackInputStart();
        playerController.ToggleMovement(false);
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
            base.OnAttackInputStart();
            AttackStart();
        }
        if (weaponState != WeaponState.HOLD) return;
        Aim(true);
        playerController.ToggleMovement(false);
    }
    /// <summary>
    /// When the player lets go of the input
    /// </summary>
    public override void OnAttackInputStop()
    {
        if (weaponState != WeaponState.HOLD) return;
        Aim(true);
        if (isCharging)
        {
            ShootArrow();
            base.Attack();
        }
        playerController.ToggleMovement(true);
    }

    public override void Attack()
    {
        
        
    }
    
    public override void AttackStart()
    {
        StartCharge();
        playerController.playerSounds.bowPullBack.Play();
        if (playerController.IsOwner) playerController.playerAnimationState.Value = PlayerAnimationState.BOW;
    }
    private void StartCharge()
    {
        weaponState = WeaponState.HOLD;
        isCharging = true;
        chargeStartTime = Time.time;
    }


    private void ShootArrow()
    {
        if (playerController.IsOwner) playerController.playerAnimationState.Value = PlayerAnimationState.IDLE;
        playerController.playerSounds.bowShot.Play();
        float chargeLevel = Mathf.Clamp01((Time.time - chargeStartTime) / weaponData.maxChargeTime);
        float chargeSpeedLevel = Mathf.Lerp(weaponData.chargeProjectileSpeed.min, weaponData.chargeProjectileSpeed.max, chargeLevel);
        float chargeDamageLevel = Mathf.Lerp(weaponData.chargeProjectileDamage.min, weaponData.chargeProjectileDamage.max, chargeLevel);
        isCharging = false;

        GameObject arrow = Instantiate(weaponData.projectilePrefab, weaponObj.transform.position, weaponObj.transform.rotation);
        arrow.GetComponent<Rigidbody>().AddForce(weaponObj.transform.forward * chargeSpeedLevel);
        ArrowManager arrowManager = arrow.GetComponent<ArrowManager>();
        arrowManager.damage = playerController.effectManager.ApplyAttackEffect(weaponData.damage.GetRandomValue() + chargeDamageLevel);
        arrowManager.playerController = playerController;
    }
}
