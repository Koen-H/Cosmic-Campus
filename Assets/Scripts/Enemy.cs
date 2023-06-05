using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public class Enemy : NetworkBehaviour
{
    [Header("Global Enemy Variables")]
    [SerializeField] public GameObject avatar;
    //TODO: Replace with slider?
    [SerializeField] TextMeshPro healthText;

    [SerializeField] EnemySO enemySO;
    public EnemyState enemyState = EnemyState.IDLING;

    [Header("Enemy statistics")]
    [SerializeField] NetworkVariable<float> health = new(10);
    private float moveSpeed;
    protected float detectionRange;
    private float trackingRange;
    private float damage;
    private float meleeRange;
    protected float attackCooldown;
    private float projectileSpeed;


    protected bool canAttack = true;

    [Header("Movement")]
    private EnemyMovement enemyMovement;

    [Header("Targetting")]
    private EnemyTargettingBehaviour targetBehaviour;
    public event System.Action<Transform> OnTargetChange;
    private Transform currentTarget;
    public Transform CurrentTarget
    {
        get { return currentTarget; }
        set
        {
            if (currentTarget == value) return;
            currentTarget = value;
            OnTargetChange?.Invoke(currentTarget);
        }
    }

    [Header("Attacking")]
    private EnemyAttackBehaviour attackBehaviour;




    #region Initialization methods
    private void Awake()
    {
        enemyMovement = GetComponent<EnemyMovement>();
        targetBehaviour = GetComponent<EnemyTargettingBehaviour>();
        attackBehaviour = GetComponent<EnemyAttackBehaviour>();
    }

    public override void OnNetworkSpawn()
    {
        health.OnValueChanged += OnHealthChange;

        SetSOData();
        SetNavMeshData();
    }

    public override void OnNetworkDespawn()
    {
        health.OnValueChanged -= OnHealthChange;
    }
    void SetSOData()
    {
        health.Value = enemySO.health;

        if (!IsServer) return;//Everything below this line is handled on the server only!
        moveSpeed = enemySO.moveSpeed;
        detectionRange = enemySO.detectionRange;
        trackingRange = enemySO.trackingRange;
        damage = enemySO.damage;
        attackCooldown = enemySO.attackCooldown;

        if (enemySO.enemyType == EnemySO.EnemyType.Melee)
        {
            meleeRange = enemySO.meleeRange;
        }
        if (enemySO.enemyType == EnemySO.EnemyType.Range)
        {
            projectileSpeed = enemySO.projectileSpeed;
        }
    }

    #endregion

    #region Health related methods

    /// <summary>
    /// Deal damage to the enemy.
    /// </summary>
    /// <param name="damageInc">The amount of damage</param>
    public void TakeDamage(float damageInc)
    {
        TakeDamgeServerRpc(damageInc);

    }

    /// <summary>
    /// The enemy only takes damage on the server side because the server owns the enemy.
    /// </summary>
    /// <param name="damageInc">The damage received</param>
    [ServerRpc(RequireOwnership = false)]
    void TakeDamgeServerRpc(float damageInc)
    {
        health.Value -= damageInc;
    }

    /// <summary>
    /// Whenever the enemy heals or take damage, the value is changed. This code happens on all the clients!
    /// This is where some sound effect should be played or a attack/heal vfx/animation.
    /// </summary>
    /// <param name="prevHealth"></param>
    /// <param name="newHealth"></param>
    void OnHealthChange(float prevHealth, float newHealth)
    {
        healthText.text = health.Value.ToString();
        if (prevHealth > newHealth)//Do thing where the enemy takes damage!
        {
            if (health.Value <= 0) Die();
        }
        else if (prevHealth < newHealth)//Do things where the enemy gained health!
        {

        }
        else { Debug.LogError("Networking error?"); }
    }

    /// <summary>
    /// When the enemy dies
    /// </summary>
    private void Die()
    {
        //TODO: Drop debris and invoke the death animation.
        if (IsOwner) Destroy(gameObject);
    }

    #endregion



    /// <summary>
    /// Because we use a navmesh agent and it rotates the whole gameobject, we need to counter rotate the healtbar each frame.
    /// </summary>
    void FixHealthBar()
    {
        //TODO: Fix.
        healthText.transform.rotation = Quaternion.Euler(0,-transform.rotation.eulerAngles.y,0);
    }

    public virtual void Update()
    {
        FixHealthBar();
        if (enemyState == EnemyState.ATTACKING) return;
        targetBehaviour.FindTarget();
        attackBehaviour.TryAttack();
    }

    public virtual void AttackLogic(Transform target)
    {
        if ((target.position - transform.position).magnitude < meleeRange && canAttack)
        {
            Attack(target);
            canAttack = false;
        }
    }

    public virtual void Attack(Transform target)
    {
        DealDamage(damage, target.GetComponent<PlayerCharacterController>());
        StartCoroutine(AttackCoolDown(attackCooldown));
    }

    void DealDamage(float damage, PlayerCharacterController to)
    {
        to.TakeDamage(damage);
    }



    void SetNavMeshData()
    {
        enemyMovement.SetSpeed(moveSpeed);
    }



    protected IEnumerator AttackCoolDown(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        canAttack = true;
    }
}
public enum EnemyState { IDLING, CHASING, FIGHTING, RUNNING, ATTACKING }