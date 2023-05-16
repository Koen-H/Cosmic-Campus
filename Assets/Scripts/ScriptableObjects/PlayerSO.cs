using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player")]
public class PlayerSO : ScriptableObject
{
    public enum ClassType
    {
        Artist,
        Designer,
        Engineer
    }
    public enum WeaponType
    {
        Sword,
        Bow,
        Staff
    }



    public float health;
    public float moveSpeed;
    public ClassType classType = ClassType.Artist;
    public WeaponType weaponType = WeaponType.Sword;

    public float attackCoolDown;
    public float attackRange;
    public float attackDamage;

    [HideInInspector] public float projectileSpeed;




}
