using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Bow : Weapon    
{
    public float maxChargeTime = 2f; // Maximum time to charge the bow
    public GameObject arrowPrefab; // Prefab of the arrow

    private bool isCharging;
    private float chargeStartTime;

    /// <summary>
    /// When the player starts with the input
    /// </summary>
    public override void OnAttackInputStart()
    {
        StartCharge();

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
        float chargeLevel = Mathf.Clamp01((Time.time - chargeStartTime) / maxChargeTime) * 1000;

        ShootArrowServerRpc(chargeLevel);
    }



    void Aim()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 clickPoint = hit.point;
            Transform playerObj = playerController.playerObj.transform;
            playerObj.LookAt(new Vector3(playerObj.position.x + clickPoint.x, playerObj.position.y, playerObj.position.z + clickPoint.z));
            //transform.LookAt(new Vector3(transform.position.x + clickPoint.x, transform.position.y, transform.position.z + clickPoint.z));
        }
    }

    private void StartCharge()
    {
        isCharging = true;
        chargeStartTime = Time.time;
    }

    [ServerRpc]
    private void ShootArrowServerRpc(float chargeLevel)
    {
        if (!isCharging)
            return;

        isCharging = false;

        GameObject arrow = Instantiate(arrowPrefab, transform.position, transform.rotation);
        arrow.GetComponent<NetworkObject>().Spawn();
        arrow.GetComponent<Rigidbody>().AddForce(transform.forward * chargeLevel);
        //ArrowController arrowController = arrow.GetComponent<ArrowController>();

        // Set the charge level of the arrow
        //arrowController.SetChargeLevel(chargeLevel);

        // Apply any other logic to the arrow (e.g., add force, apply damage, etc.)
        // arrowController.ApplyForce(...);
        // arrowController.ApplyDamage(...);
    }
}
