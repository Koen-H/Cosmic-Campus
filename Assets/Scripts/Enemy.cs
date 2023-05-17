using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public class Enemy : NetworkBehaviour
{
    private EnemyMovement enemyMovement;
    private Transform currentTarget; // Add this line
    [SerializeField] private Transform enemyEyes;
    [SerializeField] EnemySO enemySO;
    [SerializeField] private GameObject avatar;
    [SerializeField] TextMeshPro healthText;

    [SerializeField] NetworkVariable<float> health = new(10);
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
    public override void OnNetworkSpawn()
    {
        health.OnValueChanged += OnHealthChange;

        SetSOData();
        SetNavMeshData();
    }

    public override void OnNetworkDespawn()
    {
        health.OnValueChanged -= OnHealthChange;
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
            currentTarget = FindClosestPlayer(90, detectionRange, 10, enemyEyes);
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

    void AttackLogic(Transform target)
    {
        if ((target.position - transform.position).magnitude < meleeRange && canAttack)
        {
            Debug.Log("ATTACKKK");
            Attack(target);
            canAttack = false;
        }
    }

    void Attack(Transform target)
    {
        DealDamage(damage, target.transform.parent.GetComponent<PlayerCharacterController>());
        StartCoroutine(AttackCoolDown(attackCooldown));
    }

    void DealDamage(float damage, PlayerCharacterController to)
    {
        to.TakeDamage(damage);
    }

    public void TakeDamage(float damage)
    {
        TakeDamgeServerRpc();
       
    }

    [ServerRpc]
    void TakeDamgeServerRpc()
    {
        health.Value -= damage;
    }


    void OnHealthChange(float prevHealth, float newHealth)
    {
        healthText.text = health.Value.ToString();
        if (prevHealth > newHealth)//Do thing where the player takes damage!
        {
            Debug.Log("Take damage!");
            if (health.Value <= 0) Die();
        }
        else if (prevHealth < newHealth)//Do things where the player gained health!
        {
            Debug.Log("Gained healht!");
        }
        else { Debug.LogError("Networking error?"); }
    }


    private void Die()
    {
        //do dead things, such as body falling apart
        Debug.Log("EnemyDied");
    }

    void SetSOData()
    {
        health.Value = enemySO.health;
        
        if (!IsServer) return;//Everything below this line is handled on the server only!
        moveSpeed = enemySO.moveSpeed;
        detectionRange = enemySO.detectionRange;
        trackingRange = enemySO.trackingRange;
        damage = enemySO.damage;
        attackCooldown = enemySO.attackCooldown;

        if (enemySO.enemyType == EnemySO.EnemyType.Melee)
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

    Transform FindClosestPlayer(float angle, float range, int amount, Transform transform)
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
