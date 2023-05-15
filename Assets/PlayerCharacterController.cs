using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class PlayerCharacterController : NetworkBehaviour
{
    public float moveSpeed = 5f;
    [SerializeField] private GameObject playerAvatar;
    void Update()
    {
        if (!IsOwner)
            return;
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
    }

    public override void OnNetworkSpawn()
    {
        InitCharacter(OwnerClientId);
    }
}
