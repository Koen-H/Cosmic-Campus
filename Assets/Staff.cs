using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.Netcode;
using UnityEngine;

public class Staff : Weapon
{

    /// <summary>
    /// When the player starts with the input
    /// </summary>
    public override void OnAttackInputStart()
    {
        Aim();
        Attack();
        playerController.ToggleMovement(false);
    }
    /// <summary>
    /// While the player is holding the input
    /// </summary>
    public override void OnAttackInputHold()
    {
        Aim();
        playerController.AttackServerRpc();//Tell the server, that we are attacking!
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
        FindClosestEnemy();
    }

    void FindClosestEnemy()
    {
        Vector3 playerPosition = playerController.playerObj.transform.position;
        Vector3 playerForward = playerController.playerObj.transform.forward;
        int numRaycasts = 5;
        float maxDistance = weaponData.range;
        float angle = weaponData.accuracy;
        float startAngle = -angle * (numRaycasts - 1) * 0.5f;

        float closestDistance = Mathf.Infinity;
        GameObject closestEnemy = null;

        //Shoot 5 rays forward
        // Iterate over the number of raycasts
        for (int i = 0; i < numRaycasts; i++)
        {
            float currentAngle = startAngle + i * angle;

            Vector3 raycastDirection = Quaternion.Euler(0f, currentAngle, 0f) * playerForward;
            RaycastHit hit;
            Debug.DrawRay(playerPosition + Vector3.up, raycastDirection * maxDistance, Color.red, 0.01f);

            if (Physics.Raycast(playerPosition + Vector3.up, raycastDirection, out hit, maxDistance))
            {
                Debug.Log($"Hit {hit.collider.name}");
                if (hit.collider.CompareTag("Enemy"))
                {
                    float distance = Vector3.Distance(playerPosition, hit.collider.transform.position);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = hit.collider.gameObject;
                    }
                }
            }
        }

        if (closestEnemy != null)
        {
            Debug.Log(" ENEMY!!");

            //draw fancy laser line
            Debug.DrawLine(playerPosition + Vector3.up, closestEnemy.transform.position + Vector3.up, Color.yellow, 1f);
            ShowStaffBeam(weaponObj.transform,closestEnemy.transform);
            if (playerController.IsOwner) DealDamage(closestEnemy);//Only on the client that owns the weapon, we do damage!
        }

    }

    void ShowStaffBeam(Transform p1, Transform p2)
    {

        //TEMPORARY AWFULL
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0, p1.position);
        lineRenderer.SetPosition(1, p2.position);
    }

    void DealDamage(GameObject enemy)
    {
        Debug.Log("dealing damage");
        Debug.Log(enemy.name);
        enemy.transform.parent.GetComponent<Enemy>().TakeDamage(weaponData.damage);
        base.Attack();//Handle cooldown?
    }
}

