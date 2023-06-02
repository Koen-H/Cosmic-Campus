using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyAttackBehaviour : MonoBehaviour
{
    protected Enemy enemy;

    [SerializeField] bool canMoveDuringAttack = false;
    protected NavMeshAgent agent;
    [SerializeField] protected float damage;

    protected void Awake()
    {
        enemy = GetComponent<Enemy>();
        agent = GetComponent<NavMeshAgent>();
    }

    /// <summary>
    /// Try to attack
    /// </summary>
    public virtual void TryAttack()
    {
        return;
    }

    protected virtual void Attack()
    {
        enemy.enemyState = EnemyState.ATTACKING;
        if (!canMoveDuringAttack) agent.isStopped = true;
        
    }

    protected IEnumerator AfterAttackAnim(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        AfterAttack();
    }
    protected virtual void AfterAttack()
    {
        enemy.enemyState = EnemyState.IDLING;
        agent.isStopped = false;
    }
}
