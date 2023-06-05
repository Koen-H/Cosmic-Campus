using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyTargettingBehaviour : MonoBehaviour
{
    private Enemy enemy;

    protected Transform target;
    [SerializeField] protected float detectionRange;
    [SerializeField] protected float trackingRange;

    [SerializeField] protected Transform enemyEyes;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
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
}
