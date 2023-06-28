using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyAttackBehaviour : NetworkBehaviour
{

    protected Enemy enemy;
    protected NavMeshAgent agent;
    [SerializeField] private bool canMoveDuringAttack = false;
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

        return;
    }

    protected void Attacked()
    {
        if (!enemy.IsOwner) return;
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
        if (!enemy.IsOwner) return;
        enemy.enemyState = EnemyState.IDLING;
        agent.isStopped = false;
    }
}
