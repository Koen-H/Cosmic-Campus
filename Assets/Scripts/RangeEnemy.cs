using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeEnemy : Enemy
{
    //[SerializeField] ArrowManager arrow;
    //[SerializeField] float arrowForce;
    //[SerializeField] float arrowDamage;

    //public override void Update()
    //{
    //    base.Update();

    //    if (currentTarget)
    //    {
    //        transform.forward = (currentTarget.position - transform.position).normalized;
    //    }
    //}


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


}
