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
    NetworkVariable<float> maxHealth = new(20);
    NetworkVariable<float> health = new(20);
    NetworkVariable<bool> isDead = new(false);
    [HideInInspector]public NetworkVariable<Vector3> gunForward = new(default,default,NetworkVariableWritePermission.Owner);
    //LocalVariables
    public float moveSpeed = 5f;
    public bool canMove = true;
    public bool canAttack = true;
    public bool canAbility = true;

    [SerializeField] private GameObject playerAvatar;//The player mesh/model
    public GameObject playerObj;//With weapon.
    [SerializeField] TextMeshPro healthText;
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

    [SerializeField] EffectManager effectManager;

    private List<OnMapNPC> colllectedStudents = new List<OnMapNPC>();
    private QuestNPC interactingNPC;
    private List<GameObject> collectedStudents = new List<GameObject>(); 

    [SerializeField] private float attackRange; // the range of the attack, adjustable in Unity's inspector
    PlayerData playerData;

    private Rigidbody rigidbody;
    Vector3 movementDirection;

    [SerializeField] float grav; 
    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        effectManager = GetComponent<EffectManager>();
    }

    /// <summary>
    /// Heal the player based on percentage of max health.
    /// </summary>
    public void Heal(float percentage)
    {
        float addedHealth = maxHealth.Value * (percentage / 100);
        if(health.Value + addedHealth > maxHealth.Value) health.Value  = maxHealth.Value;
        else health.Value += addedHealth;
    }

    public void TakeDamage(float damage)
    {
        if (isDead.Value) return;
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
            
        }
        else
        {
            myReviveArea.gameObject.SetActive(false);
            //col.enabled = true;//Enable collider to allow it to be targeted and attacked again.
            //Resurrected animation here!
            playerObj.gameObject.SetActive(true);
        }

        if (!IsOwner) return;
        LockPlayer(isCurrentlyDead);
    }

    public void LockPlayer(bool isLocked)
    {
        canMove = !isLocked;
        canAttack = !isLocked;
        canAbility = !isLocked;
    }

    /// <summary>
    /// Revive the player
    /// </summary>
    [ServerRpc(RequireOwnership=false)]
    public void ReviveServerRpc()
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
            //TODO: show ui option for revive
        }
        else
        {
            //TODO: Hide that option for revive
        }
    }



    public override void OnNetworkSpawn()
    {
        InitCharacter(OwnerClientId);
        health.OnValueChanged += OnHealthChange;
        isDead.OnValueChanged += InjurePlayer;
        myReviveArea.gameObject.SetActive(false);
        if (!IsOwner) return;
        ClientManager.MyClient.playerCharacter = this;
        CameraManager.MyCamera.TargetPlayer();
    }

    void OnHealthChange(float prevHealth, float newHealth)
    {
        healthText.text = health.Value.ToString();
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
        if (npc) interactingNPC = npc;
    }
    private void OnTriggerExit(Collider other)
    {
        QuestNPC npc = other.gameObject.GetComponent<QuestNPC>();
        if (npc) interactingNPC = null;
    }

    void Update()
    {
        if (!IsOwner) return;//Things below this should only happen on the client that owns the object!
        if(canMove) Move();
        if (canAttack) HandleAttackInput();
        if (canAbility) HandleAbilityInput();
        if (otherReviveArea != null) TryRevive();
        if (Input.GetKeyDown(KeyCode.E)) CheckNPCInteraction();
        
    }
    void CheckNPCInteraction()
    {
        if (!interactingNPC) return;

        OnMapNPC student = interactingNPC.Interact(colllectedStudents, this.transform);
        if(student is StudentNPC)
        {
            collectedStudents.Add(interactingNPC.gameObject);
        }
        if(student is TeacherNPC)
        {
            foreach (var tempStudent in collectedStudents) { tempStudent.GetComponent<QuestNPC>().CurrentTarget = null; }
            collectedStudents.Clear();
        }
        interactingNPC = null;
        if (student != null) colllectedStudents.Add(student);
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
            currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed, Time.deltaTime / accelerationTime);
        }
        else
        {
            // If there is no input, decelerate the object
            currentSpeed = 0;
            //currentSpeed = Mathf.Lerp(currentSpeed, 0, Time.deltaTime / decelerationTime);
        }
        // Apply the calculated speed to the Rigidbody
        rigidbody.velocity = movementDirection * effectManager.ApplyMovementEffect(currentSpeed);

        if (movementDirection.magnitude == 0) return;
        playerObj.transform.forward = movementDirection; 
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

        InitWeapon();
    }

    public void ToggleMovement(bool toggle)
    {
        canMove = toggle;
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
        DesignerAbility bruh = (DesignerAbility)ability;
        bruh.PutDown(clickPoint);
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
