using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyTargettingBehaviour : MonoBehaviour
{
    private Enemy enemy;

    protected PlayerCharacterController target;
    [SerializeField] protected float detectionRange;
    [SerializeField] protected float trackingRange;

    [SerializeField] protected Transform enemyEyes;

    [SerializeField, Tooltip("Does it gain agro from being attacked?")]
    protected bool hasThirdEye;
    [SerializeField]
    protected float thirdEyeRange;
    [SerializeField, Tooltip("Change aggro to nearest enemy when it takes damage?")]
    protected bool useThirdEyeOnDamage;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        enemy.OnReceivedDamage += OnTakeDamage;
    }
    
    public virtual void FindTarget()
    {
        //The abstract class doesn't target. Silly enemy...
        return;
    }
    
    protected void SetEnemyTarget()
    {
        enemy.CurrentTarget= target;
    }

    protected virtual void OnTakeDamage()
    {
        if(hasThirdEye && (target == null || useThirdEyeOnDamage)) GetClosestPlayer();
    }

    protected void GetClosestPlayer()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, thirdEyeRange);
        Collider closestPlayerCollider = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                if (collider.GetComponent<PlayerCharacterController>().isDead.Value) continue;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayerCollider = collider;
                }
            }
        }

        if (closestPlayerCollider != null)
        {
            target = closestPlayerCollider.GetComponent<PlayerCharacterController>();
            SetEnemyTarget();
        }
    }
    
}
