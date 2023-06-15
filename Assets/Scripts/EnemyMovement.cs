using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    private Enemy enemy;
    private Transform target;

    
    [SerializeField] bool canWander = true;
    [SerializeField] float wanderDistance = 5f;
    [SerializeField] Range wanderDelay = new Range(0,1);

    //Nockback things
    [SerializeField] private bool canBeNockedBack = true;
    
    private bool isKnockedBack = false;
    private Vector3 knockbackDirection;
    private float knockbackForce;
    private float knockbackDuration;
    private float knockbackTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemy = GetComponent<Enemy>();
        enemy.OnTargetChange+= OnTargetChange;

    }

    private void Start()
    {
        if (!enemy.IsOwner) return;
        agent.enabled = true;
        if (canWander) StartCoroutine(Wander());
    }

    private void OnDisable()
    {
        enemy.OnTargetChange -= OnTargetChange;
    }

    void OnTargetChange(Transform newTarget)
    {
        target = newTarget;
    }


    public void SetSpeed(float speed) { if (speed >= 0) agent.speed = speed;}

    #region Nockback
    public void ApplyKnockback(Vector3 direction, float force, float duration)
    {
        if (!canBeNockedBack) return;
        ApplyNockbackServerRpc(direction, force, duration);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ApplyNockbackServerRpc(Vector3 direction, float force, float duration)
    {
        knockbackDirection = direction;
        knockbackForce = force;
        knockbackDuration = duration;
        knockbackTimer = 0f;
        isKnockedBack = true;
    }

    private void Nockback()
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

    #endregion

    private void Update()
    {
        if (!enemy.IsOwner) return;
        if (isKnockedBack) Nockback();
        else if (enemy.enemyState != EnemyState.ATTACKING) Move();
        if(DestinationReached()) enemy.enemyAnimationState.Value = EnemyAnimationState.IDLE;
    }
    bool DestinationReached()
    {
        if ((transform.position - agent.destination).magnitude <= agent.stoppingDistance)
        {
            return true;
        }
        return false;
    }

    protected virtual void Move()
    {
        //Simple walk towards target
        if (target == null) return;
        enemy.enemyAnimationState.Value = EnemyAnimationState.RUNNING;
        agent.SetDestination(target.position);
    }

    /// <summary>
    /// Wanders a random direction and distance.
    /// </summary>
    IEnumerator Wander()
    {
        if (enemy.enemyState == EnemyState.IDLING)
        {
            // Generate a random direction
            Vector3 randomDirection = Random.insideUnitSphere;

            // Set the Y component to 0 to ensure the agent walks on a flat surface
            randomDirection.y = 0f;

            // Normalize the direction to maintain consistent movement speed
            randomDirection.Normalize();

            // Set the destination based on the random direction
            Vector3 targetPosition = transform.position + randomDirection * wanderDistance;
            enemy.enemyAnimationState.Value = EnemyAnimationState.RUNNING;
            // Set the NavMeshAgent destination
            agent.SetDestination(targetPosition);
        }

        // Resume wandering after a short delay
        yield return new WaitForSeconds(wanderDelay.GetRandomValue());

        // Start wandering again
        StartCoroutine(Wander());
    }

}
