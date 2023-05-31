using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    private Transform target;

    //Nockback things
    private bool isKnockedBack = false;
    private Vector3 knockbackDirection;
    private float knockbackForce;
    private float knockbackDuration;
    private float knockbackTimer;

    private NavMeshAgent agent;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void SetTarget(Transform Target) { if (Target != null) target = Target; }
    public void RemoveTarget(){target = null;}
    public void SetSpeed(float speed) { if (speed >= 0) agent.speed = speed;}


    public void ApplyKnockback(Vector3 direction, float force, float duration)
    {
        knockbackDirection = direction;
        knockbackForce = force;
        knockbackDuration = duration;
        knockbackTimer = 0f;
        isKnockedBack = true;
    }

    private void Update()
    {
        if (isKnockedBack)
        {
            // Apply knockback force
            agent.velocity = knockbackDirection.normalized * knockbackForce;

            // Update the timer and check if knockback duration is over
            knockbackTimer += Time.deltaTime;
            if (knockbackTimer >= knockbackDuration)
            {
                // Knockback duration is over, resume normal movement
                isKnockedBack = false;
                agent.velocity = Vector3.zero;
            }
        }
        else if (target != null)
        {
            agent.destination = target.position;
        }
    }


}
