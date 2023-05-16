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

    [SerializeField] private GameObject playerAvatar;//The player mesh/model
    public GameObject playerObj;//With weapon.
    [SerializeField] TextMeshPro healthText;

    [SerializeField] private float damage;

    [SerializeField] private Weapon weapon; 


    [SerializeField] private float attackRange; // the range of the attack, adjustable in Unity's inspector

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
        //if (Input.GetMouseButtonDown(0))
        //{
        //    weapon.Attack(); 
        //}
        if (Input.GetMouseButtonDown(0))
        {
            weapon.OnAttackInputStart();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            weapon.OnAttackInputStop();
        }
        if (Input.GetMouseButton(0))
        {
            weapon.OnAttackInputHold();
        }
    }

/*    void Attack()
    {
        // calculate raycast direction
        Vector3 rayDirection = transform.TransformDirection(Vector3.forward);

        Debug.DrawRay(transform.position + Vector3.up, rayDirection , Color.red, 0.01f);
        // initialize a variable to store the hit information
        RaycastHit hit;

        // shoot the raycast
        if (Physics.Raycast(transform.position + Vector3.up, rayDirection, out hit, attackRange))
        {
            // check if the object hit has the tag "Enemy"
            if (hit.transform.CompareTag("Enemy"))
            {
                // call DealDamage function
                DealDamage(hit.transform.gameObject);
            }
        }
    }

    void DealDamage(GameObject enemy)
    {
        enemy.transform.parent.GetComponent<Enemy>().TakeDamage(damage);
    }*/

        /// <summary>
        /// Movement
        /// </summary>
        private void Move()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0f, moveVertical) * moveSpeed * Time.deltaTime;
        transform.Translate(movement);
    }


    /// <summary>
    /// Creates the character visually by loading the selected data from playerdata stored in MyClient
    /// </summary>
    public void InitCharacter(ulong clientId)
    {
        PlayerData playerData = LobbyManager.Instance.GetClient(clientId).GetComponent<PlayerData>();
        foreach (Transform child in playerAvatar.transform) Destroy(child.gameObject);//Destroy previous model, if exists.
        Debug.Log(playerData.avatarId.Value);
        GameObject newAvatar = playerData.playerRoleData.GetAvatar(playerData.avatarId.Value);//Get the new avatar
        Instantiate(newAvatar, playerAvatar.transform);
        healthText.text = health.Value.ToString();

    }

}
