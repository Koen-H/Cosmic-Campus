using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyMovement), typeof(EffectManager))]
public class Enemy : NetworkBehaviour
{
    [Header("Global Enemy Variables")]
    [SerializeField] public GameObject avatar;
    [SerializeField] public Transform centerPoint;
    //TODO: Replace with slider?
    [SerializeField] TextMeshPro healthText;

    [SerializeField] HealthBar healthBar;

    [SerializeField] EnemySO enemySO;
    public EnemyState enemyState = EnemyState.IDLING;

    [Header("Enemy type variables")]
    public NetworkVariable<EnemyType> enemyType = new(0);
    public float typeMatchDamageIncrease = 1.5f;//
    public float typeMatchDamagePenalty = 1;//Default is none
    public bool forceTypeMatch = false;

    [Header("Enemy statistics")]
    [SerializeField] NetworkVariable<float> health = new(10);
    private float moveSpeed;
    protected float detectionRange;
    private float trackingRange;
    private float damage;
    private float meleeRange;
    protected float attackCooldown;
    private float projectileSpeed;

    Quaternion healthBarOriginalRotation; 
    public event System.Action OnReceivedDamage;

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

    public EffectManager effectManager;



    #region Initialization methods
    private void Awake()
    {
        enemyMovement = GetComponent<EnemyMovement>();
        targetBehaviour = GetComponent<EnemyTargettingBehaviour>();
        attackBehaviour = GetComponent<EnemyAttackBehaviour>();
        effectManager = GetComponent<EffectManager>();
    }

    private void Start()
    {
        healthBarOriginalRotation = healthBar.transform.rotation; 
    }

    public override void OnNetworkSpawn()
    {
        health.OnValueChanged += OnHealthChange;
        effectManager.OnEffectChange += HandleEffectChange;
        SetSOData();
        SetNavMeshData();
        healthBar.SetMaxValue(health.Value);
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
    public void TakeDamage(float damageInc, EnemyType damageType)
    {
        TakeDamageServerRpc(damageInc, damageType);
    }

    /// <summary>
    /// The enemy only takes damage on the server side because the server owns the enemy.
    /// </summary>
    /// <param name="damageInc">The damage received</param>
    [ServerRpc(RequireOwnership = false)]
    void TakeDamageServerRpc(float damageInc, EnemyType damageType)
    {
        float totalDamage = damageInc;
        if (enemyType.Value != EnemyType.NONE)
        {
            bool typeMatch = damageType == enemyType.Value;
            if (!typeMatch && forceTypeMatch)
            {
                //We don't deal damage, we are forcing matchin enemy!
                return;
            }
            else if (typeMatch)
            {
                //The match fits! Apply the bonus damage!
                totalDamage *= typeMatchDamageIncrease;
            }
            else
            {
                //The match doesn't fit! Decrease the damage!
                totalDamage *= typeMatchDamagePenalty;
            }
        }
        totalDamage = effectManager.ApplyResistanceEffect(totalDamage);
        health.Value -= totalDamage;
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
        healthBar.UpdateBar((int)newHealth);
        if (prevHealth > newHealth)//Do thing where the enemy takes damage!
        {
            if (OnReceivedDamage != null) OnReceivedDamage.Invoke();
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

        FallApart(); 
        if (IsOwner) StartCoroutine(LateDestroy());
        gameObject.SetActive(false);
    }
    void FallApart()
    {
        List<Transform> bodyParts = GetChildren(avatar.transform);

        foreach (var bodyPart in bodyParts)
        {
            bodyPart.parent = null;
            bodyPart.gameObject.AddComponent<BoxCollider>(); 
            bodyPart.gameObject.AddComponent<Rigidbody>();
            bodyPart.tag = "Debris";
        }
    }
    List<Transform> GetChildren(Transform parent)
    {
        int childCount = parent.transform.childCount;
        List<Transform> children = new List<Transform>(); 
        for (int i = 0; i < childCount; i++)
        {
            Transform temp = parent.transform.GetChild(i);
            if (temp.name == "Weapon") continue;
            if (temp.GetComponent<MeshRenderer>()) children.Add(temp);
            if (temp.transform.childCount > 0)
            {
                List<Transform> childrenOfChild = GetChildren(temp);
                foreach (var child in childrenOfChild) { children.Add(child); }
            }

        }
        return children; 
    }

    /// <summary>
    /// Destroy the enemy later, so it can sync up with the other clients.
    /// </summary>
    IEnumerator LateDestroy()
    {
        yield return new WaitForSeconds(5);
        Destroy(gameObject);
    }

    #endregion


    /// <summary>
    /// Because we use a navmesh agent and it rotates the whole gameobject, we need to counter rotate the healtbar each frame.
    /// </summary>
    void FixHealthBar()
    {
        //TODO: Fix.
        healthBar.transform.LookAt(Camera.main.transform, -Vector3.up);
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


    void HandleEffectChange()
    {
        enemyMovement.SetSpeed(effectManager.ApplyMovementEffect(moveSpeed));
    }

}
public enum EnemyState { IDLING, CHASING, FIGHTING, RUNNING, ATTACKING }
public enum EnemyType { NONE, ARTIST, DESIGNER, ENGINEER }