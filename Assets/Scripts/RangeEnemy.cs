using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeEnemy : PunchAttack
{
    [SerializeField] EnemyProjectile projectile;
    [SerializeField] float projectileSpeed;
    [SerializeField] float projectileDamage;
    [SerializeField] GameObject projectileSpawner;
    [SerializeField] float rangedAttackRange;

    //public override void AttackLogic(Transform target)
    //{
    //    if ((target.position - transform.position).magnitude < detectionRange && canAttack)
    //    {
    //        ArrowManager newArrow = Instantiate(arrow, transform.position + transform.forward, transform.rotation);
    //        newArrow.GetComponent<Rigidbody>().AddForce(transform.forward * arrowForce);
    //        ArrowManager arrowManager = newArrow.GetComponent<ArrowManager>();
    //        arrowManager.damage = arrowDamage;
    //        arrowManager.rangeEnemy = this;


    //        StartCoroutine(AttackCoolDown(attackCooldown));
    //        //Attack(target);
    //        canAttack = false;
    //    }
    //}

    public override void TryAttack()
    {
        Transform currentTarget = enemy.CurrentTarget;
        if (currentTarget == null) return;
        if ((currentTarget.position - transform.position).magnitude < attackRange)
        {
            //We are punching on close distance!
            base.Attack();
        }
        else
        {
            Vector3 toTarget = currentTarget.position - transform.position;
            float dotProduct = Vector3.Dot(toTarget.normalized, transform.forward);

            if (dotProduct > 0 && toTarget.magnitude < rangedAttackRange)
            {
                Attack();
            }
        }
        return;
    }

    protected override void Attack()
    {
        Attacked();
        if (enemy.IsOwner)
        {
            enemy.enemyAnimationState.Value = EnemyAnimationState.SWORDSLASH;//Replace with shoot?
        }
        projectileSpawner.transform.LookAt(enemy.CurrentTarget.GetComponent<PlayerCharacterController>().centerPoint);
        EnemyProjectile projectileInstance = Instantiate(projectile.gameObject,projectileSpawner.transform.position, projectileSpawner.transform.rotation).GetComponent<EnemyProjectile>();
        projectileInstance.GetComponent<Rigidbody>().AddForce(projectileInstance.transform.forward * projectileSpeed);
        projectileInstance.damage = projectileDamage;
        projectileInstance.enemy = enemy;
        MatChanger[] matChangers = projectileInstance.GetComponentsInChildren<MatChanger>();
        foreach (MatChanger matChang in matChangers) matChang.ChangeMaterial(enemy.enemyType.Value,true);
        //Todo: trail?
        StartCoroutine(AfterAttackAnim(enemy.animator.GetCurrentAnimatorStateInfo(0).length));

    }


}
