using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Looks in front to see if a player walks in front of the enemy.
/// </summary>
public class FieldOfView : EnemyTargettingBehaviour
{

    public override void FindTarget()
    {

        if(target != null && !target.isDead.Value)
        {
                if (Vector3.Distance(transform.position, target.transform.position) <= trackingRange )//If the target is still in range and not dead
                {
                    // Visualization
                    Debug.DrawLine(transform.position, target.transform.position, Color.green, 0.01f);

                    //AttackLogic(currentTarget);
                }
                else
                {
                    target = FindClosestPlayer(90, detectionRange, 10, enemyEyes);
                }
        }
        else
        {
            target = FindClosestPlayer(90, detectionRange, 10, enemyEyes);
        }
        SetEnemyTarget();
        return;
    }

    // Update is called once per frame
    //void Update()
    //{
    //    // Check if the current target is still within the tracking range
    //    if (target != null && Vector3.Distance(transform.position, target.position) <= trackingRange)
    //    {
    //        // Visualization
    //        Debug.DrawLine(transform.position, target.position, Color.green, 0.01f);

    //        enemyMovement.SetTarget(currentTarget);
    //        //AttackLogic(currentTarget);
    //    }
    //    else
    //    {
    //        target = FindClosestPlayer(90, detectionRange, 10, enemyEyes);
    //        if (target != null)
    //        {
    //            enemyMovement.SetTarget(target);
    //            //AttackLogic(target);
    //        }
    //        else
    //        {
    //            enemyMovement.RemoveTarget();
    //        }
    //    }
    //}

    PlayerCharacterController FindClosestPlayer(float angle, float range, int amount, Transform transform)
    {
        PlayerCharacterController closest = null;
        float closestDistance = range;

        // The step size between each raycast
        float stepSize = angle / (amount - 1);

        for (int i = 0; i < amount; i++)
        {
            // The current angle of the raycast
            float currentAngle = -angle / 2 + stepSize * i;

            // Rotate the forward direction by the current angle around the up axis
            Vector3 raycastDirection = Quaternion.Euler(0, currentAngle, 0) * transform.forward;

            // Visualize the raycast direction
            Debug.DrawRay(transform.position, raycastDirection * range, Color.red, 0.01f);

            // Shoot the raycast
            Ray ray = new Ray(transform.position, raycastDirection);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, range))
            {

                // Check if the hit object is a player
                if (hit.collider.tag == "Player")
                {
                    // Check if this player is closer than the previous closest player
                    if (hit.distance < closestDistance)
                    {
                        closest = hit.collider.GetComponent<PlayerCharacterController>();
                        if (closest.isDead.Value) continue;
                        closestDistance = hit.distance;
                    }
                }
            }
        }

        // Return the closest player found, or null if no player was found
        return closest;
    }
}
