using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyMovement), typeof(EffectManager))]
public class Enemy : NetworkBehaviour
{
    [Header("Global Enemy Variables")]
    [SerializeField] public GameObject avatar;
    [SerializeField] private GameObject enemyDebrisDrops;
    [SerializeField] public Transform centerPoint;
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private EnemySO enemySO;
    [SerializeField] private Animator animator;
    public EnemyState enemyState = EnemyState.IDLING;
    public EffectManager effectManager;
    public EnemySoundManager soundManager;

    [Header("Enemy type variables")]
    public EnemyType enemyTypeInsp = EnemyType.NONE;
    [HideInInspector] public NetworkVariable<EnemyType> enemyType = new NetworkVariable<EnemyType>(default);
    [SerializeField] private bool startWithRandomType = false;
    [SerializeField] private float typeMatchDamageIncrease = 1.5f;//
    [SerializeField] private float typeMatchDamagePenalty = 1;//Default is none
    [SerializeField] private bool forceTypeMatch = false;
    [SerializeField] private bool changeTypeOnCorrectHit;
    private ulong lastClientDamageID;


    [Header("Enemy statistics")]
    [SerializeField] NetworkVariable<float> health = new(10);
    public event System.Action OnReceivedDamage;
    private float maxHealth;
    private float moveSpeed;


    protected bool canAttack = true;

    [Header("Movement")]
    private EnemyMovement enemyMovement;

    [Header("Targetting")]
    private EnemyTargettingBehaviour targetBehaviour;
    private PlayerCharacterController currentTarget;
    public PlayerCharacterController CurrentTarget
    {
        get { return currentTarget; }
        set
        {
            if (currentTarget == value) return;
            currentTarget = value;
            if(OnTargetChange != null) OnTargetChange?.Invoke(currentTarget);
        }
    }
    public event System.Action<PlayerCharacterController> OnTargetChange;

    [Header("Attacking")]
    private EnemyAttackBehaviour attackBehaviour;
    [HideInInspector] public NetworkVariable<EnemyAnimationState> enemyAnimationState = new(EnemyAnimationState.IDLE, default, NetworkVariableWritePermission.Owner);

    void OnPlayerStateChanged(EnemyAnimationState pervAnimationState, EnemyAnimationState newAnimationState)
    {
        switch (newAnimationState)
        {
            case EnemyAnimationState.RUNNING:
                animator.SetBool("Running", true);
                break;
            case EnemyAnimationState.SWORDSLASH:
                animator.SetTrigger("SwordSlash");
                break;
            default:
                animator.SetBool("Running", false);
                break;
        }
    }



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
        enemyDebrisDrops.SetActive(false);
        if (IsOwner) enemyType.Value = enemyTypeInsp;
        if (IsOwner && startWithRandomType) enemyType.Value = enemyType.Value = GameManager.Instance.GetEnemyType();
        StartCoroutine(EnemyLogic());
    }



    public override void OnNetworkSpawn()
    {
        health.OnValueChanged += OnHealthChange;
        effectManager.OnEffectChange += HandleEffectChange;
        enemyType.OnValueChanged+= OnEnemyTypeChange;
        enemyAnimationState.OnValueChanged += OnPlayerStateChanged;
        SetSOData();
        SetNavMeshData();
        maxHealth = health.Value;
        healthBar.SetMaxValue(maxHealth);
    }

    public override void OnNetworkDespawn()
    {
        health.OnValueChanged -= OnHealthChange;
        effectManager.OnEffectChange -= HandleEffectChange;
        enemyType.OnValueChanged -= OnEnemyTypeChange;
        soundManager.PlayDeathSFX();
        FallApart();
    }
    void SetSOData()
    {
        health.Value = enemySO.health;

        if (!IsServer) return;//Everything below this line is handled on the server only!
        moveSpeed = enemySO.moveSpeed;


    }

    #endregion

    #region Health related methods

    /// <summary>
    /// Heal the player based on percentage of max health.
    /// </summary>
    public void Heal(float percentage)
    {
        if (!IsOwner) return;
        float addedHealth = maxHealth * (percentage / 100);
        if (health.Value + addedHealth > maxHealth) health.Value = maxHealth;
        else health.Value += addedHealth;
    }


    /// <summary>
    /// Deal damage to the enemy.
    /// </summary>
    /// <param name="damageInc">The amount of damage</param>
    public void TakeDamage(float damageInc, EnemyType damageType, bool inPercentage = false)
    {
        TakeDamageServerRpc(damageInc, damageType, inPercentage);
    }

    /// <summary>
    /// The enemy only takes damage on the server side because the server owns the enemy.
    /// </summary>
    /// <param name="damageInc">The damage received</param>
    [ServerRpc(RequireOwnership = false)]
    void TakeDamageServerRpc(float damageInc, EnemyType damageType = EnemyType.NONE, bool inPercentage = false , ServerRpcParams serverRpcParams = default)
    {
        float totalDamage =  inPercentage ? maxHealth * (damageInc / 100) :damageInc;
        if (enemyType.Value != EnemyType.WHITE)
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
                if (changeTypeOnCorrectHit) enemyType.Value = GameManager.Instance.GetEnemyType(enemyType.Value);
            }
            else
            {
                //The match doesn't fit! Apply the penalty!
                totalDamage *= typeMatchDamagePenalty;
            }
        }else if (changeTypeOnCorrectHit) enemyType.Value = GameManager.Instance.GetEnemyType(enemyType.Value); ;//Color is white, change it!
        totalDamage = effectManager.ApplyResistanceEffect(totalDamage);
        health.Value -= totalDamage;
        lastClientDamageID = serverRpcParams.Receive.SenderClientId;
    }

    /// <summary>
    /// Whenever the enemy heals or take damage, the value is changed. This code happens on all the clients!
    /// This is where some sound effect should be played or a attack/heal vfx/animation.
    /// </summary>
    /// <param name="prevHealth"></param>
    /// <param name="newHealth"></param>
    void OnHealthChange(float prevHealth, float newHealth)
    {
        healthBar.UpdateBar((int)newHealth);
        if (prevHealth > newHealth)//Do thing where the enemy takes damage!
        {
            CanvasManager.Instance.SpawnDamageText(this.transform.position, (int)(newHealth- prevHealth));
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

        //if (IsOwner) StartCoroutine(LateDestroy());
        if (IsOwner)
        {
            Destroy(this.gameObject);
            LobbyManager.Instance.GetClient(lastClientDamageID).golemsKilled.Value++;
        }
    }
    void FallApart()
    {
        enemyDebrisDrops.SetActive(true);
        List<Transform> bodyParts = GetChildren(enemyDebrisDrops.transform);
        MatChanger[] matChangers = enemyDebrisDrops.GetComponentsInChildren<MatChanger>();
        foreach (MatChanger matChang in matChangers) matChang.ChangeMaterial(enemyType.Value, true);
        foreach (var bodyPart in bodyParts)
        {
            bodyPart.parent = null;
            //bodyPart.gameObject.AddComponent<MeshRenderer>();
            bodyPart.gameObject.AddComponent<BoxCollider>(); 
            bodyPart.gameObject.AddComponent<Rigidbody>().mass = 0.01f;
            bodyPart.tag = "Debris";
            bodyPart.gameObject.layer = LayerMask.NameToLayer("Debris");
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
        healthBar.transform.LookAt(Camera.main.transform, -Vector3.up);
    }


    /*    public void Update()
        {
            FixHealthBar();
            if (enemyState == EnemyState.ATTACKING) return;
            targetBehaviour.FindTarget();
            attackBehaviour.TryAttack();

            if (Input.GetKeyDown(KeyCode.Alpha1)) enemyType.Value = EnemyType.NONE;
            if (Input.GetKeyDown(KeyCode.Alpha2)) enemyType.Value = EnemyType.ARTIST;
            if (Input.GetKeyDown(KeyCode.Alpha3)) enemyType.Value = EnemyType.DESIGNER;
            if (Input.GetKeyDown(KeyCode.Alpha4)) enemyType.Value = EnemyType.ENGINEER;
        }
    */

    IEnumerator EnemyLogic()
    {
        while (true)
        {
            LessThanFixedUpdate();

            yield return new WaitForSeconds(0.1f);
        }
    }
    public virtual void LessThanFixedUpdate()
    {
        FixHealthBar();
        targetBehaviour.FindTarget();
        if (enemyState == EnemyState.ATTACKING) return;
        attackBehaviour.TryAttack();
    }


    //public virtual void AttackLogic(Transform target)
    //{
    //    if ((target.position - transform.position).magnitude < meleeRange && canAttack)
    //    {
    //        Attack(target);
    //        canAttack = false;
    //    }
    //}

    //public virtual void Attack(Transform target)
    //{
    //    DealDamage(damage, target.GetComponent<PlayerCharacterController>());
    //    StartCoroutine(AttackCoolDown(attackCooldown));
    //}

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




    void OnEnemyTypeChange(EnemyType prevType, EnemyType newType)
    {
        MatChanger[] matChangers = GetComponentsInChildren<MatChanger>();
        foreach (MatChanger matChang in matChangers) matChang.ChangeMaterial(newType);
    }

}
public enum EnemyState { IDLING, CHASING, FIGHTING, RUNNING, ATTACKING }
public enum EnemyType { NONE, ARTIST, DESIGNER, ENGINEER, WHITE}


public enum EnemyAnimationState
{
    IDLE,
    RUNNING,
    SWORDSLASH
}