using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.Netcode;
using UnityEngine;


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

    [SerializeField] private Weapon weapon; 
    [SerializeField] private Ability ability;

    [SerializeField] private float attackRange; // the range of the attack, adjustable in Unity's inspector
    PlayerData playerData;
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

        if (Input.GetMouseButtonDown(1))
        {
           // Ability();
        }
    }

    void Ability()
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
    }


    /// <summary>
    /// Movement
    /// </summary>
    private void Move()
    {
        if(!canMove) return;
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        if (moveHorizontal == 0 && moveVertical == 0) return;
        Vector3 direction = new Vector3(moveHorizontal, 0f, moveVertical);
        Vector3 movement = direction * moveSpeed * Time.deltaTime;
        playerObj.transform.forward = direction;
        transform.Translate(movement);
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

    /// <summary>
    /// To notify the server, that the client is attacking
    /// </summary>
    [ServerRpc]
    public void AttackServerRpc()
    {
        AttackClientRpc();
    }


    /// <summary>
    /// Tell each client this character is attacking!
    /// </summary>
    [ClientRpc]
    void AttackClientRpc()
    {
        weaponBehaviour.Attack();
    }

    /// <summary>
    /// 
    /// </summary>
    [ServerRpc]
    public void AttackStartServerRpc()
    {
        AttackStartClientRpc();
    }

    /// <summary>
    /// Begin of attack
    /// </summary>
    [ClientRpc]
    private void AttackStartClientRpc()
    {
        weaponBehaviour.AttackStart();
    }

}
