using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "WeaponData")]

//Scriptable objects has references to the avatars an weapons.
public class WeaponData : ScriptableObject
{
    public GameObject weaponPrefab = null;
    public WeaponType weaponType = WeaponType.UNSET;

    public float damage = 0;
    public float speed = 0;
    public float cooldown = 0;

    public float range = 0;

    public GameObject projectilePrefab = null;
    public float minProjectileSpeed = 0;
    public float maxProjectileSpeed = 0;

}
