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
    NetworkVariable<float> health = new(10);
    //LocalVariables
    public float moveSpeed = 5f;
    public bool canMove = true;

    [SerializeField] private GameObject playerAvatar;//The player mesh/model
    public GameObject playerObj;//With weapon.
    [SerializeField] TextMeshPro healthText;

    [SerializeField] private float damage;
    [SerializeField] private GameObject playerWeapon;//The object
    [SerializeField] private Weapon weaponBehaviour;//The weapon behaviour
    private Ability ability;

    private bool isGrounded;


    [SerializeField]float accelerationTime = 0.5f;
    [SerializeField]float decelerationTime = 0.5f;
    [SerializeField]float maxSpeed = 10f;
    [SerializeField]float currentSpeed = 0f;


    [SerializeField] private float attackRange; // the range of the attack, adjustable in Unity's inspector
    PlayerData playerData;

    private Rigidbody rigidbody;
    Vector3 movementDirection;

    [SerializeField] float grav; 
    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }
    public void TakeDamage(float damage)
    {
        if (damage > 0) health.Value -= damage;
        if (health.Value <= 0) Debug.Log("Player Died");
    }


    public override void OnNetworkSpawn()
    {
        InitCharacter(OwnerClientId);
        health.OnValueChanged += OnHealthChange;
        if (!IsOwner) return;

        Camera.main.GetComponent<CameraFollow>().target = this.transform;
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


    void Update()
    {
        if (!IsOwner) return;//Things below this should only happen on the client that owns the object!

        Move();
        if (Input.GetMouseButtonDown(0))
        {
            weaponBehaviour.OnAttackInputStart();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            weaponBehaviour.OnAttackInputStop();
        }
        if (Input.GetMouseButton(0))
        {
            weaponBehaviour.OnAttackInputHold();
        }







    }
    private void FixedUpdate()
    {
/*        float groundCheckDist = 0.5f;
        Vector3 rayDirection = Vector3.down * groundCheckDist;
        Debug.DrawRay(transform.position, rayDirection, Color.blue, 0.01f);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, rayDirection, out hit, rayDirection.magnitude))
        {
            isGrounded = true;
        }
        else isGrounded = false; */
    }

    /*    void Ability()
        {
            // calculate raycast direction
            Vector3 rayDirection = transform.TransformDirection(Vector3.forward);

            Debug.DrawRay(transform.position + Vector3.up, rayDirection, Color.green, 0.01f);
            // initialize a variable to store the hit information
            RaycastHit hit;

            // shoot the raycast
            if (Physics.Raycast(transform.position + Vector3.up, rayDirection, out hit, attackRange))
            {
                ability.Activate(hit.collider.gameObject);
            }
        }*/

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
        Debug.Log($"MOve {horizontalInput} , {verticalInput}");
        // Check if there is input
        if (movementDirection != Vector3.zero)
        {
            // If there is input, accelerate the object
            currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed, Time.deltaTime / accelerationTime);
            Debug.Log("moving");
        }
        else
        {
            Debug.Log("else");
            // If there is no input, decelerate the object
            currentSpeed = 0;
            //currentSpeed = Mathf.Lerp(currentSpeed, 0, Time.deltaTime / decelerationTime);
        }

        // Apply the calculated speed to the Rigidbody
        rigidbody.velocity = movementDirection * currentSpeed;




        /*        // Capture input from WASD keys
                float horizontalInput = Input.GetAxis("Horizontal");
                float verticalInput = Input.GetAxis("Vertical");


                // Calculate the movement direction based on input
                movementDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;

                // Move the player based on the movement direction
                if (movementDirection != Vector3.zero)
                {
                    // Calculate the desired position based on movement direction
                    Vector3 desiredPosition = transform.position + movementDirection * moveSpeed * Time.deltaTime;

                    // Use NavMesh.SamplePosition to find the closest valid position on the NavMesh
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(desiredPosition, out hit, 1, NavMesh.AllAreas))
                    {
                        // Move the player to the valid NavMesh position
                        transform.position = hit.position;
                    }
                }*/






        /*        if (!canMove) return;
                float moveHorizontal = Input.GetAxis("Horizontal");
                float moveVertical = Input.GetAxis("Vertical");


                Vector3 direction = new Vector3(moveHorizontal, 0, moveVertical);
                Vector3 movement = direction.normalized * moveSpeed * Time.deltaTime;
                playerObj.transform.forward = new Vector3(direction.x, 0, direction.z);
                //rigidbody.AddForce(movement, ForceMode.);
                rigidbody.velocity = movement + Vector3.down * grav;*/
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

}
