using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ReadyUpManager : NetworkBehaviour
{
    [SerializeField] List<ReadyUpUIItems> avatarPlatform;
    Dictionary<ulong, ReadyUpUIItems> clientItems = new Dictionary<ulong, ReadyUpUIItems>();

    Dictionary<ulong, bool> clientReady = new();


    private void Start()
    {
        LobbyManager.OnNewClientJoined += NewClientJoined;
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    void NewClientJoined(ClientManager newClient)
    {
        ulong newClientId = newClient.GetClientId();
        clientReady[newClientId] = false;
        clientItems.Add(newClientId, avatarPlatform[clientItems.Count]);
        UpdatePlayerCharacter(newClientId);

        //Give the client their front space spot on their own screen
        ulong localClientId = NetworkManager.Singleton.LocalClientId;
        if (!clientItems.ContainsKey(localClientId)) return;
        if (clientItems[localClientId] != avatarPlatform[0])//If we don't have the first spot
        {
            ReadyUpUIItems myCurrentItems = clientItems[localClientId];
            ReadyUpUIItems myNewItems = clientItems[0];
            clientItems[localClientId] = myNewItems;
            clientItems[0] = myCurrentItems;
        }
    }

    public void HandleRoleValueChange(SideClickerValue newValue)
    {
        ClientManager.MyClient.playerData.playerRole.Value = (PlayerRole)int.Parse(newValue.value);
        UpdatePlayerCharacter(NetworkManager.Singleton.LocalClientId);
    }


    public void UpdatePlayerCharacter(ulong outdatedClientId)
    {
        PlayerData playerData = LobbyManager.Instance.GetClient(outdatedClientId).playerData;
        PlayerRoleData playerRoleData= playerData.playerRoleData;
        if (playerRoleData == null) return;
        GameObject playerPlatform = clientItems[outdatedClientId].avatarPreview;

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

    [ServerRpc(RequireOwnership = false)]
    public void ValueChangedServerRpc(int newValue, ServerRpcParams serverRpcParams = default)
    {
        ValueChangedClientRpc(serverRpcParams.Receive.SenderClientId, newValue);
    }

    [ClientRpc]
    public void ValueChangedClientRpc(ulong receivedClientId, int newValue)
    {
        if (NetworkManager.Singleton.LocalClientId == receivedClientId) return;
        clientItems[receivedClientId].sideClickerManager.UpdateValue(newValue);
        UpdatePlayerCharacter(receivedClientId);
    }

    public void ReadyUp()
    {
        ReadyUpServerRpc(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReadyUpServerRpc(bool ready = true,ServerRpcParams serverRpcParams = default)
    {
        clientReady[serverRpcParams.Receive.SenderClientId] = ready;

        if (clientReady.ContainsValue(false)) return;
        StartGame();
    }


    private void StartGame()
    {
        NetworkManager.SceneManager.LoadScene("TestingScene",UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

}

[System.Serializable]
public class ReadyUpUIItems
{
    public GameObject avatarPreview;
    public SideClickerManager sideClickerManager;
    public TextMeshPro userNameDisplay;
}