using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public class Enemy : MonoBehaviour
{
    private EnemyMovement enemyMovement;
    private Transform currentTarget; // Add this line

    [SerializeField] EnemySO enemySO;

    private float health;
    private float moveSpeed;
    private float detectionRange;
    private float trackingRange;
    private float damage;
    private float meleeRange;
    private float attackCooldown;

    private bool canAttack = true;

    private float projectileSpeed;

    private void Awake()
    {
        enemyMovement = GetComponent<EnemyMovement>();
    }

    private void Start()
    {
        SetSOData();
        SetNavMeshData();
    }

    private void Update()
    {
        // Check if the current target is still within the tracking range
        if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.position) <= trackingRange)
        {
            // Visualization
            Debug.DrawLine(transform.position, currentTarget.position, Color.green, 0.01f);

            enemyMovement.SetTarget(currentTarget);
            AttackLogic(currentTarget);
        }
        else
        {
            currentTarget = FindClosestPlayer(90, detectionRange, 10);
            if (currentTarget != null)
            {
                enemyMovement.SetTarget(currentTarget);
                AttackLogic(currentTarget);
            }
            else
            {
                enemyMovement.RemoveTarget();
            }
        }
    }

    void AttackLogic(Transform Target)
    {
        if ((Target.position - transform.position).magnitude < meleeRange && canAttack)
        {
            Attack(Target);
            canAttack = false;
        }
    }

    void Attack(Transform target)
    {
        DealDamage(damage, target.GetComponent<Player>());
        StartCoroutine(AttackCoolDown(attackCooldown));
    }

    void DealDamage(float damage, Player to)
    {
        to.TakeDamage(damage);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0) Debug.Log("EnemyDied");
    }

    void SetSOData()
    {
        health = enemySO.health;
        moveSpeed = enemySO.moveSpeed;
        detectionRange = enemySO.detectionRange;
        trackingRange = enemySO.trackingRange;
        damage = enemySO.damage;
        attackCooldown = enemySO.attackCooldown;

        if (enemySO.enemyType == EnemySO.EnemyType.Range)
        {
            meleeRange = enemySO.meleeRange;
        }
        if (enemySO.enemyType == EnemySO.EnemyType.Range)
        {
            projectileSpeed = enemySO.projectileSpeed;
        }
    }

    void SetNavMeshData()
    {
        enemyMovement.SetSpeed(moveSpeed);
    }

    Transform FindClosestPlayer(float angle, float range, int amount)
    {
        Transform closest = null;
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
                        closest = hit.collider.transform;
                        closestDistance = hit.distance;
                    }
                }
            }
        }

        // Return the closest player found, or null if no player was found
        return closest;
    }

    IEnumerator AttackCoolDown(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        canAttack = true;
    }
}
