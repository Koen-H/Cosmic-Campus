using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ReadyUpManager : MonoBehaviour
{
    [SerializeField] List<GameObject> avatarPlatform;
    Dictionary<ulong,GameObject> clientAvatars= new Dictionary<ulong, GameObject>();

    private void Start()
    {
        LobbyManager.OnNewClientJoined += NewClientJoined;
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    void NewClientJoined(ClientManager newClient)
    {
        if(newClient.GetClientId() == NetworkManager.Singleton.LocalClientId)
        {
            clientAvatars.Add(newClient.GetClientId(), avatarPlatform[0]);
        }
        else
        {
            clientAvatars.Add(newClient.GetClientId(), avatarPlatform[LobbyManager.Instance.ConnectedClientsAmount()]);
        }
        UpdatePlayerCharacter(newClient.GetClientId());
    }

    void UpdatePlayerCharacter(ulong outdatedClientId)
    {
        PlayerData playerData = LobbyManager.Instance.GetClient(outdatedClientId).playerData;
        PlayerRoleData playerRoleData= playerData.playerRoleData;
        if (playerRoleData == null) return;
        GameObject playerPlatform = clientAvatars[outdatedClientId];


        GameObject newAvatar = playerRoleData.GetAvatar(playerData.avatarId.Value);
        foreach (Transform child in playerPlatform.transform) Destroy(child.gameObject);
        Instantiate(newAvatar, playerPlatform.transform);
        //GameObject newWeapon = playerRoleData.GetWeapon(playerData.weaponId.Value).weaponPrefab;
        //foreach (Transform child in weaponShowcase.transform) Destroy(child.gameObject);
        //Instantiate(newWeapon, weaponShowcase.transform);
    }

    public void AssignRole(int newRoleInt)
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;
        LobbyManager.Instance.GetClient(clientId).playerData.playerRole.Value = (PlayerRole)newRoleInt;
        UpdatePlayerCharacter(clientId);
    }
}
