using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using static PlayerSO;


/// <summary>
/// This is the player Character that is used to walk and attack within the game.
/// </summary>
public class PlayerCharacterController : NetworkBehaviour
{
    //NetworkVariables
    public NetworkVariable<float> maxHealth = new(50);
    public NetworkVariable<float> health = new(25, default, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isDead = new(false, default, NetworkVariableWritePermission.Owner);
    [HideInInspector]public NetworkVariable<Vector3> gunForward = new(default,default,NetworkVariableWritePermission.Owner);
    //LocalVariables
    //public float moveSpeed = 5f;
    public bool canMove = true;
    public bool canAttack = true;
    public bool canAbility = true;
    public bool engineering = false;

    [Tooltip("What enemytype will take critical damage?")]
    public EnemyType damageType = EnemyType.NONE;

    [SerializeField] private GameObject playerAvatar;//The player mesh/model
    public GameObject playerObj;//With weapon.
    [SerializeField] TextMeshPro healthText;
    [SerializeField] HealthBar healthBar;
    [SerializeField] ReviveAreaManager myReviveArea;
    public ReviveAreaManager otherReviveArea;

    [SerializeField] private float damage;
    [SerializeField] private GameObject playerWeapon;//The object
    private Weapon weaponBehaviour;//The weapon behaviour
    private Ability ability;
    private Collider col;

    [SerializeField]float accelerationTime = 0.5f;
    [SerializeField]float decelerationTime = 0.5f;
    [SerializeField]float maxSpeed = 10f;
    [SerializeField]float currentSpeed = 0f;

    public EffectManager effectManager;

    private List<OnMapNPC> colllectedStudents = new List<OnMapNPC>();
    private QuestNPC interactingNPC;
    private List<GameObject> collectedStudents = new List<GameObject>();

    private Animator animator;
    public Vector3 checkPoint;
    int checkPointRespawns = 0;

    public Transform centerPoint;

    protected bool canBeDamaged = true;
    [SerializeField] float invinsibilityDuration;

    [HideInInspector] public NetworkVariable<bool> usingCart = new(false,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);
    float cartLoad = 0;
    [SerializeField] private float cartEnterTime = 3;
    [SerializeField] GameObject cartObject;
    [SerializeField] float cartSpeed;



    public NetworkVariable<PlayerAnimationState> playerAnimationState = new(PlayerAnimationState.IDLE, default, NetworkVariableWritePermission.Owner); 


    [SerializeField] private float attackRange; // the range of the attack, adjustable in Unity's inspector
    PlayerData playerData;

    private Rigidbody rigidbody;
    Vector3 movementDirection;

    [SerializeField] float grav;
    private bool isGrounded;
    private float groundDistance = 0.2f;


    private bool knockedBack = false;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        effectManager = GetComponent<EffectManager>();
        animator = GetComponentInChildren<Animator>();
    }

    [ClientRpc]
    public void HealPlayerClientRPC(float percentage)
    {
        if (!IsOwner) return;
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
        LockPlayer(isCurrentlyDead, true);
    }
    IEnumerator InvinsibilityForTime(float time)
    {
        canBeDamaged = false;
        yield return new WaitForSeconds(time);
        canBeDamaged = true;
    }

    public void LockPlayer(bool isLocked, bool deadOverride = false)
    {
        if (isDead.Value && !deadOverride) return;//If dead, you can't change this!
        canMove = !isLocked;
        canAttack = !isLocked;
        canAbility = !isLocked;
    }




    /// <summary>
    /// Revive the player
    /// </summary>
    public void Revive()
    {
        isDead.Value = false;
        health.Value = maxHealth.Value * 0.25f;
    }

    /// <summary>
    /// Set the revive area that the player ented. Let the player know that he can heal his teammate by holding E!
    /// </summary>
    public void SetReviveArea(ReviveAreaManager reviveArea)
    {
        otherReviveArea = reviveArea;
        if(otherReviveArea != null)
        {
            CanvasManager.Instance.ToggleRevive(true);
        }
        else
        {
            CanvasManager.Instance.ToggleRevive(false);
        }
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
    void OnHealthChange(float prevHealth, float newHealth)
    {
        //healthText.text = health.Value.ToString();
        healthBar.UpdateBar((int)newHealth);
        if (prevHealth > newHealth)//Do thing where the player takes damage!
        {
            Debug.Log("Take damage!");
        }
        else if (prevHealth < newHealth)//Do things where the player gained health!
        {
            Debug.Log("Gained healht!");
        }
        else { Debug.LogError("Networking error?"); }
    }
    private void OnTriggerEnter(Collider other)
    {
        QuestNPC npc = other.gameObject.GetComponent<QuestNPC>();
        if (npc)
        {
            interactingNPC = npc;
            if (!interactingNPC.isFollowing.Value) CanvasManager.Instance.ToggleInteract(true);
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
        if (canMove && !knockedBack) Move();
        DeathCheck();
        LoadCart();
        if (usingCart.Value) return;//Below this is disabled while you are in a cart!
        if (canAttack) HandleAttackInput();
        if (canAbility) HandleAbilityInput();
        if (otherReviveArea != null) TryRevive();
        if (Input.GetKeyDown(KeyCode.E)) CheckNPCInteraction();
    }

    void LoadCart()
    {
        if (isDead.Value) cartLoad = 0;
        if (Input.GetKey(KeyCode.Space)) cartLoad += Time.deltaTime;
        else cartLoad = 0;
        if (cartLoad >= cartEnterTime) usingCart.Value = true;
        if (usingCart.Value && Input.GetKeyDown(KeyCode.Space)) usingCart.Value = false;

    }

    void DeathCheck()
    {
        if (transform.position.y < -20) Respawn();
    }
    void Respawn()
    {
        transform.position = checkPoint;
        checkPointRespawns++;
    }
    void CheckNPCInteraction()
    {
        if (!interactingNPC) return;

        interactingNPC.InteractServerRpc();//colllectedStudents,  //OnMapNPC student = 
        /*        if (student is StudentNPC)
                {
                    collectedStudents.Add(interactingNPC.gameObject);
                }
                if(student is TeacherNPC)
                {
                    foreach (var tempStudent in collectedStudents) { tempStudent.GetComponent<QuestNPC>().CurrentTarget = null; }
                    collectedStudents.Clear();
                }
                interactingNPC = null;
                if (student != null) colllectedStudents.Add(student);*/
    }


    void TryRevive()
    {
        if (Input.GetKey(KeyCode.E))
        {
            otherReviveArea.OnRevivingServerRpc();
        }
    }

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

    private void CheckIfGrounded()
    {
        // Cast a sphere downwards from the character
        RaycastHit hit;
        float radius = 0.5f; // Replace this with your sphere radius

        if (Physics.SphereCast(transform.position + Vector3.up, radius, -Vector3.up, out hit, groundDistance + 1))
        {
            // If the sphere hit something, the character is grounded
            isGrounded = true;
        }
        else
        {
            // If the sphere didn't hit anything, the character is not grounded
            isGrounded = false;
        }
    }


    void ToggleCart(bool old, bool toggle)
    {
        cartObject.SetActive(toggle);
        //Cart pose OR idle!
        //TODO:: Change anim
        if (IsOwner && toggle) DiscordManager.Instance.UpdateStatus("Racing on rainbow road", $"Times fallen off: {checkPointRespawns}");
    }


    /// <summary>
    /// Movement
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
            playerAnimationState.Value = usingCart.Value ?  PlayerAnimationState.CART :PlayerAnimationState.RUNNING;
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
        rigidbody.velocity = velocityVector;

        if (movementDirection.magnitude == 0) return;
        playerObj.transform.forward = movementDirection; 
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
        rigidbody.velocity = Vector3.zero;
        rigidbody.velocity = direction * force;
        knockedBack= true;
        StartCoroutine(KnockbackReset(duration));
    }

    private IEnumerator KnockbackReset(float duration)
    {
        yield return new WaitForSeconds(duration);
        rigidbody.velocity = Vector3.zero;
        knockedBack= false;
    }


    /// <summary>
    /// Creates the character visually by loading the selected data from playerdata stored in MyClient
    /// </summary>
    public void InitCharacter(ulong clientId)
    {
        playerData = LobbyManager.Instance.GetClient(clientId).GetComponent<PlayerData>();
        foreach (Transform child in playerAvatar.transform) Destroy(child.gameObject);//Destroy previous model, if exists.
        GameObject newAvatar = playerData.playerRoleData.GetAvatar(playerData.avatarId.Value);//Get the new avatar
        Instantiate(newAvatar, playerAvatar.transform);
        healthText.text = health.Value.ToString();
        damageType = (EnemyType)playerData.playerRole.Value;
        InitWeapon();
    }



    public void ToggleMovement(bool toggle)
    {
        if (isDead.Value) return;
        if (engineering) return;
        canMove = toggle;
        if (toggle == false) animator.SetBool("Running", false);
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


    [ServerRpc]
    public void DeactivateServerRpc(Vector3 clickPoint, ServerRpcParams serverRpcParams = default)
    {
        DeactivateAbilityClientRpc(clickPoint, serverRpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    void DeactivateAbilityClientRpc(Vector3 clickPoint, ulong receivedClientId)
    {
        //Commented this line, please uncomment when the deactivate is modular!
        //if (NetworkManager.Singleton.LocalClientId == receivedClientId) return;
        //DesignerAbility bruh = (DesignerAbility)ability;
        //bruh.PutDown(clickPoint);
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