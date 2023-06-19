using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "WeaponData")]

//Scriptable objects has references to the avatars an weapons.
public class WeaponData : ScriptableObject
{
    public string nickName = "";
    public GameObject weaponPrefab = null;
    public Vector3 weaponObjOffset = Vector3.zero;
    public WeaponType weaponType = WeaponType.UNSET;

    public Range damage = new Range(0,1);
    public Range damageHeal = new Range(0,1);
    [Tooltip("How long does the attack take?")]
    public float speed = 0;
    [Tooltip("How long till the attack can happen again?")]
    public float cooldown = 0;

    public float range = 0;

    public GameObject projectilePrefab = null;
    public float minProjectileSpeed = 0;
    public float maxProjectileSpeed = 0;

    public float accuracy = 0;

    public GameObject beamVfx = null;
}
