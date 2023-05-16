using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    private int weaponID;
    [SerializeField] protected int damage;

    [SerializeField] private float cooldown;
    private bool canAttack = true; 




    public virtual void Attack()
    {
        canAttack = false;
        StartCoroutine(Cooldown(cooldown));


    }

    IEnumerator Cooldown(float time)
    {
        yield return new WaitForSeconds(time);
        canAttack = true;
    }


}
