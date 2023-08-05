using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using static PlayerSO;


/// <summary>
/// This is the player Character that is used to walk and attack within the game.
/// </summary>
/// In the future, it would be really nice to create different prefabs with inherited scripts 
/// instead of dynamically adding the related script for roles as this limits the networking through the scripts that are already on the prefab.
public class PlayerCharacterController : NetworkBehaviour
{
    [Header("Player Variables")]
    public Transform centerPoint;
    [SerializeField] private GameObject playerAvatar;//The player mesh/model
    public GameObject playerObj;//With weapon.
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private ReviveAreaManager myReviveArea;
    public EffectManager effectManager;
    public PlayerSoundsManager playerSounds;//All player related sounds are on the player!
    private PlayerData playerData;
    [FormerlySerializedAs("rigidbody")]
    private Rigidbody rigid;
    private Animator animator;


    [Header("Player lockment")]
    [HideInInspector] public NetworkVariable<bool> isDead = new(false, default, NetworkVariableWritePermission.Owner);
    [HideInInspector] public bool canMove = true;
    [HideInInspector] public bool canAttack = true;
    [HideInInspector] public bool canAbility = true;
    [HideInInspector] public bool engineering = false;
    public bool inSettings = false;


    [Header("Player Health")]
    public NetworkVariable<float> maxHealth = new(50);
    public NetworkVariable<float> health = new(50, default, NetworkVariableWritePermission.Owner);
    private bool canBeDamaged = true;
    [SerializeField] private float invinsibilityDuration;

    [Header("Weapon")]
    [SerializeField] private GameObject playerWeapon;//The object
    private Weapon weaponBehaviour;//The weapon behaviour
    [HideInInspector] public NetworkVariable<Vector3> gunForward = new(default, default, NetworkVariableWritePermission.Owner);

    [Header("Damage type")]
    [Tooltip("What enemytype will take critical damage?")]
    [HideInInspector] public EnemyType damageType = EnemyType.NONE;

    [Header("Ability")]
    private Ability ability;

    [Header("Movement")]
    [SerializeField]private float accelerationTime = 0.5f;
    [SerializeField]private float maxSpeed = 10f;
    [SerializeField]private float currentSpeed = 0f;
    private bool knockedBack = false;
    ///groundcheck and gravity 
    private bool isGrounded;
    private float groundDistance = 0.2f;
    [SerializeField] private float grav;

    [Header("Cart")]
    [HideInInspector] public NetworkVariable<bool> usingCart = new(false,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);
    private float cartLoad = 0;
    [SerializeField] private float cartEnterTime = 3;
    [SerializeField] private GameObject cartObject;
    [SerializeField] private float cartSpeed;

    [Header("Misc")]
    private QuestNPC interactingNPC;
    public NetworkVariable<bool> isReviving = new(false, default, NetworkVariableWritePermission.Owner);
    [HideInInspector] public NetworkVariable<PlayerAnimationState> playerAnimationState = new(PlayerAnimationState.IDLE, default, NetworkVariableWritePermission.Owner); 
    [HideInInspector] public Vector3 checkPoint;

    [Header("Stats")]
    private int checkPointRespawns = 0;


    private void Start()
    {
        rigid = GetComponent<Rigidbody>();
        effectManager = GetComponent<EffectManager>();
        animator = GetComponentInChildren<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        InitCharacter(OwnerClientId);
        health.OnValueChanged += OnHealthChange;
        playerAnimationState.OnValueChanged += OnPlayerStateChanged;
        usingCart.OnValueChanged += ToggleCart;
        isDead.OnValueChanged += InjurePlayer;
        myReviveArea.gameObject.SetActive(false);
        LobbyManager.Instance.GetClient(OwnerClientId).playerCharacter = this;
        healthBar.SetMaxValue(maxHealth.Value);
        healthBar.UpdateBar((int)health.Value);
        if (!IsOwner) return;
        CameraManager.MyCamera.TargetPlayer();

    }

    #region initalization
    /// <summary>
    /// Creates the character visually by loading the selected data from playerdata stored in MyClient
    /// </summary>
    public void InitCharacter(ulong clientId)
    {
        playerData = LobbyManager.Instance.GetClient(clientId).GetComponent<PlayerData>();
        foreach (Transform child in playerAvatar.transform) Destroy(child.gameObject);//Destroy previous model, if exists.
        GameObject newAvatar = playerData.playerRoleData.GetAvatar(playerData.avatarId.Value);//Get the new avatar
        Instantiate(newAvatar, playerAvatar.transform);
        damageType = (EnemyType)playerData.playerRole.Value;
        InitWeapon();
    }

    public void InitWeapon()
    {
        foreach (Transform child in playerWeapon.transform) Destroy(child.gameObject);//Destroy previous model, if exists.
        WeaponData newWeapon = playerData.playerRoleData.GetWeapon(playerData.weaponId.Value);//Get the new weapon
        GetAbilityBehaviour();
        GetWeaponBehaviour(newWeapon.weaponType);
        weaponBehaviour.weaponObj = Instantiate(newWeapon.weaponPrefab, playerWeapon.transform);
        weaponBehaviour.weaponObj.transform.localPosition = newWeapon.weaponObjOffset;
        weaponBehaviour.weaponData = newWeapon;
    }

    private void GetWeaponBehaviour(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.SWORD:
                weaponBehaviour = this.gameObject.AddComponent<Sword>();
                break;

            case WeaponType.BOW:
                weaponBehaviour = this.gameObject.AddComponent<Bow>();
                break;

            case WeaponType.STAFF:
                weaponBehaviour = this.gameObject.AddComponent<Staff>();
                break;

            default:
                Debug.LogError("No weapon selected?!");
                return;
        }
    }

    private void GetAbilityBehaviour()
    {
        switch (playerData.playerRole.Value)
        {
            case PlayerRole.ENGINEER:
                ability = this.gameObject.AddComponent<EngineerAbility>();
                break;

            case PlayerRole.DESIGNER:
                ability = this.gameObject.AddComponent<DesignerAbility>();
                break;

            case PlayerRole.ARTIST:
                ability = this.gameObject.AddComponent<ArtistAbility>();
                break;

            default:
                Debug.LogError("No role selected?!");
                return;
        }
    }
    #endregion

    #region health

    [ClientRpc]
    public void HealPlayerClientRPC(float percentage)
    {
        if (!IsOwner) return;//Only the owner is allowed to write to the variable, here we prevent others
        Heal(percentage);
    }
    
    /// <summary>
    /// Heal the player based on percentage of max health.
    /// </summary>
    private void Heal(float percentage)
    {
        if (isDead.Value) return;//We don't heal when we are dead. Thats not how it works!
        float addedHealth = maxHealth.Value * (percentage / 100);
        if(health.Value + addedHealth > maxHealth.Value) health.Value  = maxHealth.Value;
        else health.Value += addedHealth;
    }

    /// <summary>
    /// For when the server has to do damage to the client
    /// </summary>
    [ClientRpc]
    public void TakeDamageClientRpc(float damage, bool inPercentage = false)
    {
        TakeDamage(damage, inPercentage);
    }

    public void TakeDamage(float damage, bool inPercentage = false)
    {
        if (!IsOwner) return;
        if (isDead.Value) return;
        if (!canBeDamaged) return;
        if (usingCart.Value) usingCart.Value = false;
        if (inPercentage) damage = maxHealth.Value * (damage / 100);
        damage = effectManager.ApplyResistanceEffect(damage);
        if (damage >= health.Value) damage = health.Value;
        if (damage > 0) health.Value -= damage;
        if (health.Value <= 0) isDead.Value = true;
    }
    void OnHealthChange(float prevHealth, float newHealth)
    {
        //healthText.text = health.Value.ToString();
        healthBar.UpdateBar((int)newHealth);
        if (prevHealth > newHealth)//Do thing where the player takes damage!
        {
            playerSounds.playerHit.Play();
            // Debug.Log("Take damage!");
        }
        else if (prevHealth < newHealth)//Do things where the player gained health!
        {
            //Debug.Log("Gained healht!");
        }
    }


    void InjurePlayer(bool prevVal, bool isCurrentlyDead)
    {
        if (isCurrentlyDead)
        {
            //Tell the gamemanager that this player is dead, if all other players are dead it's a game over!
            //Gamemanager.player died!
            weaponBehaviour.CancelAttack();
            //col.enabled= false;//Disable collider to make the enemy target a different player.
            playerObj.gameObject.SetActive(false);
            myReviveArea.gameObject.SetActive(true);
            playerSounds.playerDowned.Play();
            if (IsOwner) ClientManager.MyClient.timesDied.Value++;
        }
        else
        {
            myReviveArea.gameObject.SetActive(false);
            //col.enabled = true;//Enable collider to allow it to be targeted and attacked again.
            //Resurrected animation here!
            playerObj.gameObject.SetActive(true);
            StartCoroutine(InvinsibilityForTime(invinsibilityDuration));

        }

        if (IsServer) GameManager.Instance.PlayerDeadStatus(OwnerClientId,isCurrentlyDead);
        if (!IsOwner) return;
        if (engineering) return;// Got revived during engineering ability go back
        LockPlayer(isCurrentlyDead, true);
    }

    /// <summary>
    /// Revive the player
    /// </summary>
    public void Revive()
    {
        isDead.Value = false;
        health.Value = maxHealth.Value * 0.25f;
    }
    IEnumerator InvinsibilityForTime(float time)
    {
        canBeDamaged = false;
        yield return new WaitForSeconds(time);
        canBeDamaged = true;
    }
    #endregion

    public void LockPlayer(bool isLocked, bool deadOverride = false)
    {
        if (isDead.Value && !deadOverride) return;//If dead, you can't change this!
        canMove = !isLocked;
        canAttack = !isLocked;
        canAbility = !isLocked;
    }

    /// <summary>
    /// Handles the animations of the player. Automatically networked when the value is changed.
    /// </summary>
    void OnPlayerStateChanged(PlayerAnimationState pervAnimationState, PlayerAnimationState newAnimationState)
    {
        animator.SetBool("Bowing", false);
        animator.SetBool("Running", false);
        animator.SetBool("Staffing", false);

        switch (newAnimationState)
        {
            case PlayerAnimationState.RUNNING:
                animator.SetBool("Running", true);
                break;
            case PlayerAnimationState.SWORDSLASH:
                animator.SetTrigger("SwordSlash");
                break;
            case PlayerAnimationState.BOW:
                animator.SetBool("Bowing", true);
                break;
            case PlayerAnimationState.STAFF:
                animator.SetBool("Staffing", true);
                break;
            case PlayerAnimationState.CART:
                //TODO:: Change with cart pose
                animator.SetBool("Running", false);
                break;
            default:
                break;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        QuestNPC npc = other.gameObject.GetComponent<QuestNPC>();
        if (npc is QuestStudentNPC && npc.CurrentTarget == null)
        {
            if (interactingNPC != null && interactingNPC.saved.Value) return;
            interactingNPC = npc;
            if (!interactingNPC.isFollowing.Value && IsOwner) CanvasManager.Instance.ToggleInteract(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        QuestNPC npc = other.gameObject.GetComponent<QuestNPC>();
        if (npc)
        {
            interactingNPC = null;
            CanvasManager.Instance.ToggleInteract(false);
        }
    }

    void Update()
    {
        healthBar.transform.LookAt(Camera.main.transform, -Vector3.up);
        if (!IsOwner) return;//Things below this should only happen on the client that owns the object!
        CheckIfGrounded();
        DeathCheck();
        TryRevive();
        if (Input.GetKey(KeyCode.V)) return;//Is moving the camera!
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CanvasManager.Instance.ToggleSettingsUI(!inSettings);
        }
        if (inSettings) return;
        if (canMove && !knockedBack) Move();
        LoadCart();
        if (usingCart.Value) return;//Below this is disabled while you are in a cart!
        if (canAttack) HandleAttackInput();
        if (canAbility) HandleAbilityInput();
        if (Input.GetKeyDown(KeyCode.E)) CheckNPCInteraction();
    }

    #region updateChecks
    private void CheckIfGrounded()
    {
        // Cast a sphere downwards from the character
        RaycastHit hit;
        float radius = 0.5f; // Replace this with your sphere radius

        if (Physics.SphereCast(transform.position + Vector3.up, radius, -Vector3.up, out hit, groundDistance + 1))
        {
            // If the sphere hit something, the character is grounded
            BackgroundMusicManager.Instance.HandleGroundMusic(hit.transform.tag);
            isGrounded = true;
        }
        else
        {
            // If the sphere didn't hit anything, the character is not grounded
            isGrounded = false;
        }
    }
    
    void DeathCheck()
    {
        if (transform.position.y < -20) Respawn();
    }
    
    void TryRevive()
    {
        if (Input.GetKey(KeyCode.E) && !isDead.Value)
        {
            isReviving.Value = true;
        }
        else
        {
            isReviving.Value = false;
        }
    }
    
    /// <summary>
    /// Handles the player movement
    /// </summary>
    private void Move()
    {
        int horizontalInput = 0;
        int verticalInput = 0;

        if (Input.GetKey(KeyCode.D)) horizontalInput = 1;
        if (Input.GetKey(KeyCode.A)) horizontalInput = -1;
        if (Input.GetKey(KeyCode.W)) verticalInput = 1;
        if (Input.GetKey(KeyCode.S)) verticalInput = -1;


        Vector3 movementDirection = new Vector3(horizontalInput, 0, verticalInput).normalized;
        Quaternion rotationQuaternion = Quaternion.Euler(0, -45, 0);
        movementDirection = rotationQuaternion * movementDirection;
        // Check if there is input
        if (movementDirection != Vector3.zero)
        {
            // If there is input, accelerate the object
            float highSpeed = usingCart.Value ? cartSpeed : maxSpeed;

            currentSpeed = Mathf.Lerp(currentSpeed, highSpeed, Time.deltaTime / accelerationTime);
            playerAnimationState.Value = usingCart.Value ? PlayerAnimationState.CART : PlayerAnimationState.RUNNING;
        }
        else
        {
            // If there is no input, decelerate the object
            currentSpeed = 0;
            playerAnimationState.Value = PlayerAnimationState.IDLE;
            //currentSpeed = Mathf.Lerp(currentSpeed, 0, Time.deltaTime / decelerationTime);
        }
        // Apply the calculated speed to the Rigidbody
        Vector3 velocityVector = movementDirection * effectManager.ApplyMovementEffect(currentSpeed);
        if (!isGrounded) velocityVector += Vector3.up * grav;
        rigid.velocity = velocityVector;

        if (movementDirection.magnitude == 0) return;
        playerObj.transform.forward = movementDirection;
    }
   
    void LoadCart()
    {
        if (isDead.Value) cartLoad = 0;
        if (Input.GetKey(KeyCode.Space)) cartLoad += Time.deltaTime;
        else cartLoad = 0;
        if (cartLoad >= cartEnterTime) usingCart.Value = true;
        if (usingCart.Value && Input.GetKeyDown(KeyCode.Space)) usingCart.Value = false;

    }
    
    void Respawn()
    {
        transform.position = checkPoint;
        checkPointRespawns++;
    }
    
    void CheckNPCInteraction()
    {
        if (!interactingNPC) return;
        if (interactingNPC.saved.Value) return;
        interactingNPC.InteractServerRpc();
        CanvasManager.Instance.ToggleInteract(false);
    }

    #endregion

    void HandleAttackInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            weaponBehaviour.OnAttackInputStart();
            AttackServerRpc();//Tell the server, that we are attacking!
        }
        else if (Input.GetMouseButtonUp(0))
        {
            weaponBehaviour.OnAttackInputStop();
            AttackStopServerRpc();
        }
        if (Input.GetMouseButton(0))
        {
            weaponBehaviour.OnAttackInputHold();
        }
    }

    void HandleAbilityInput()
    {
        ability.AbilityInput();
        
    }

    #region movement

    void ToggleCart(bool old, bool toggle)
    {
        cartObject.SetActive(toggle);
        if (toggle) playerSounds.cartUse.Play();
        else playerSounds.cartUse.Stop();
        if (IsOwner && toggle) DiscordManager.Instance.UpdateStatus("Racing on rainbow road", $"Times fallen off: {checkPointRespawns}", "Haha kart goes vroem", "karting");
    }

    public void ApplyKnockback(Vector3 direction, float force, float duration)
    {
        ApplyKnockbackServerRpc(direction, force, duration);
    }

    [ServerRpc (RequireOwnership = false)]
    private void ApplyKnockbackServerRpc(Vector3 direction, float force, float duration)
    {
        ApplyKnockbackClientRpc(direction, force, duration);
    }

    [ClientRpc]
    private void ApplyKnockbackClientRpc(Vector3 direction, float force, float duration)
    {
        if (!IsOwner) return;
        rigid.velocity = Vector3.zero;
        rigid.velocity = direction * force;
        knockedBack= true;
        StartCoroutine(KnockbackReset(duration));
    }

    private IEnumerator KnockbackReset(float duration)
    {
        yield return new WaitForSeconds(duration);
        rigid.velocity = Vector3.zero;
        knockedBack= false;
    }

    #endregion



    public PlayerData GetPlayerData() => playerData;



    public void ToggleMovement(bool toggle)
    {
        if (isDead.Value) return;
        if (engineering) return;
        canMove = toggle;
        if (toggle == false) animator.SetBool("Running", false);
    }

    public void StartAbilityCooldown(float cooldownTime)
    {
        StartCoroutine(ability.Cooldown(cooldownTime));
    }

    #region Remote Procedure Calls
    /// These RPC's are purely to tell the server the client did something, and back


    [ServerRpc(RequireOwnership = false)]
    public void ActivateServerRpc(Vector3 origin, Vector3 direction, ServerRpcParams serverRpcParams = default)
    {
        weaponBehaviour.CancelAttack();
        AbilityClientRpc(origin, direction, serverRpcParams.Receive.SenderClientId);
    }
    [ClientRpc]
    void AbilityClientRpc(Vector3 origin, Vector3 direction, ulong receivedClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == receivedClientId) return;
        ability.Activate( origin, direction); 
    }

    /// <summary>
    /// To notify the server, that the client is attacking
    /// </summary>
    [ServerRpc]
    public void AttackServerRpc(ServerRpcParams serverRpcParams = default)
    {
        AttackClientRpc(serverRpcParams.Receive.SenderClientId);
    }


    /// <summary>
    /// Tell each client this character is attacking!
    /// </summary>
    [ClientRpc]
    void AttackClientRpc(ulong receivedClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == receivedClientId) return;
        weaponBehaviour.Attack();
    }

    /// <summary>
    /// 
    /// </summary>
    [ServerRpc]
    public void AttackStartServerRpc(ServerRpcParams serverRpcParams = default)
    {
        AttackStartClientRpc(serverRpcParams.Receive.SenderClientId);
    }

    /// <summary>
    /// Begin of attack
    /// </summary>
    [ClientRpc]
    private void AttackStartClientRpc(ulong receivedClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == receivedClientId) return;
        weaponBehaviour.AttackStart();
    }

    /// <summary>
    /// Tell the server we stopped the attack
    /// </summary>
    [ServerRpc]
    public void AttackStopServerRpc(ServerRpcParams serverRpcParams = default)
    {
        AttackStopClientRpc(serverRpcParams.Receive.SenderClientId);
    }

    /// <summary>
    /// Stop the attack
    /// </summary>
    [ClientRpc]
    private void AttackStopClientRpc(ulong receivedClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == receivedClientId) return;
        weaponBehaviour.OnAttackInputStop();
    }
    #endregion


}

public enum PlayerAnimationState
{
    IDLE,
    RUNNING,
    SWORDSLASH,
    BOW,
    STAFF,
    CART,

}