using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy")]
public class EnemySO : ScriptableObject
{
    [System.Serializable]
    public enum EnemyType
    {
        Melee,
        Range
    }
    public enum ProjectileType
    {
        Arrow,
        Bullet,
        RPG,
        Nuke
    }

    public enum TargetType
    {
        All,
        Bow,
        Sword,
        Staff
    }

    [SerializeField] public TargetType targetType = TargetType.All;
    [SerializeField] public float health; 
    [SerializeField] public float moveSpeed;
    [SerializeField] public float detectionRange;
    [SerializeField] public float trackingRange; 
    [SerializeField] public float attackCooldown; 
    [SerializeField] public EnemyType enemyType = EnemyType.Melee;
    [HideInInspector] public ProjectileType projectileType = ProjectileType.Arrow;
    [SerializeField] public float damage;
    [HideInInspector] public float projectileSpeed;
    [HideInInspector] public float meleeRange; 
}
