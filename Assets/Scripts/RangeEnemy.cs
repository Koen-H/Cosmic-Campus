using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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
        PlayerCharacterController currentTarget = enemy.CurrentTarget;
        if (currentTarget == null) return;
        Vector3 toTarget = currentTarget.transform.position - transform.position;
        float dotProduct = Vector3.Dot(toTarget.normalized, transform.forward);

        if (dotProduct < 0) return;
        if ((currentTarget.transform.position - transform.position).magnitude < attackRange)
        {
            //We are trying to punch on close distance!
            base.Attack();
        }
        else
        {
            if (toTarget.magnitude < rangedAttackRange) Attack();
        }
        return;
    }

    protected override void Attack()
    {
        Attacked();
        if (enemy.IsOwner)
        {
            enemy.enemyAnimationState.Value = EnemyAnimationState.SWORDSLASH;//Replace with shoot?
            ShootClientRpc(enemy.CurrentTarget.GetComponent<PlayerCharacterController>().centerPoint.position);
        }

        //Todo: trail?
        float attackAnimLength = 0.917f; 
        StartCoroutine(AfterAttackAnim(attackAnimLength));

    }


    [ClientRpc]
    void ShootClientRpc(Vector3 lookAtPos)
    {
        projectileSpawner.transform.LookAt(lookAtPos);
        enemy.soundManager.enemyShoot.Play();
        EnemyProjectile projectileInstance = Instantiate(projectile.gameObject, projectileSpawner.transform.position, projectileSpawner.transform.rotation).GetComponent<EnemyProjectile>();
        projectileInstance.GetComponent<Rigidbody>().AddForce(projectileInstance.transform.forward * projectileSpeed);
        projectileInstance.damage = projectileDamage;
        projectileInstance.enemy = enemy;
        MatChanger[] matChangers = projectileInstance.GetComponentsInChildren<MatChanger>();
        foreach (MatChanger matChang in matChangers) matChang.ChangeMaterial(enemy.enemyType.Value, true);
    }


}
