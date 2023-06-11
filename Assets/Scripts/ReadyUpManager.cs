using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ReadyUpManager : NetworkBehaviour
{
    [SerializeField] List<ReadyUpUIItems> avatarPlatform;
    Dictionary<ulong, ReadyUpUIItems> clientItems = new Dictionary<ulong, ReadyUpUIItems>();

    Dictionary<ulong, bool> clientReady = new();

    [SerializeField] GameObject charUI;
    [SerializeField] Button charNextButton;
    [SerializeField] TextMeshProUGUI charNextButtonText;

    [SerializeField] GameObject weaponUI;
    [SerializeField] Button weaponNextButton;
    [SerializeField] TextMeshProUGUI weaponNextButtonText;


    bool weaponsSelected;


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
        clientItems[newClientId].sideClickerManager.gameObject.SetActive(true);
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
        CheckReady();
    }

    public void HandleRoleValueChange(SideClickerValue newValue)
    {
        ClientManager.MyClient.playerData.playerRole.Value = (PlayerRole)int.Parse(newValue.value);
        UpdatePlayerCharacter(NetworkManager.Singleton.LocalClientId, int.Parse(newValue.value));
    }


    public void UpdatePlayerCharacter(ulong outdatedClientId, int newValue = 100)
    {
        //PlayerData playerData = LobbyManager.Instance.GetClient(outdatedClientId).playerData;
        PlayerRoleData playerRoleData;
        switch (newValue)
        {
            case 1: playerRoleData = GameData.Instance.artistData; 
                break;
            case 2:
                playerRoleData = GameData.Instance.designerData;
                break;
            case 3:
                playerRoleData = GameData.Instance.engineerData;
                break;
            default:
                Debug.Log("no value!?");
                try {playerRoleData = LobbyManager.Instance.GetClient(outdatedClientId).playerData.playerRoleData;}
                catch { return; }
                break;
        }

        if(playerRoleData == null) return;
        GameObject playerPlatform = clientItems[outdatedClientId].avatarPreview;

        GameObject newAvatar = playerRoleData.GetAvatar(0);
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
        UpdatePlayerCharacter(clientId, newRoleInt);
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
        SideClickerManager sideClickerManager = clientItems[receivedClientId].sideClickerManager;
        sideClickerManager.UpdateValue(newValue);
        UpdatePlayerCharacter(receivedClientId, int.Parse(sideClickerManager.GetValue()));
    }

    public void ReadyUp()
    {
        ReadyUpServerRpc(true);
    }


    public void ReadyDown()
    {
        ReadyUpServerRpc(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReadyUpServerRpc(bool ready = true,ServerRpcParams serverRpcParams = default)
    {
        clientReady[serverRpcParams.Receive.SenderClientId] = ready;
        ReadyUpClientRpc(ready,serverRpcParams.Receive.SenderClientId);
        CheckReady();

        //StartGame();
    }

    void CheckReady()
    {
        if (IsServer) charNextButton.gameObject.SetActive(true);

        int isReadies = 0;
        foreach (KeyValuePair<ulong, bool> entry in clientReady)
        {
            if (entry.Value)
            {
                isReadies++;
            }
        }

        //
        if (isReadies == clientReady.Count)
        {
            if (!weaponsSelected)
            {
                charNextButton.interactable = true;
                charNextButtonText.text = "Everyone ready!";
            }
            else
            {
                weaponNextButton.interactable = false;
                weaponNextButtonText.text = "Everyone ready!";
            }
        }
        else
        {
            if (!weaponsSelected)
            {
                charNextButton.interactable = false;
                charNextButtonText.text = $"{isReadies}/{clientReady.Count} ready";
            }
            else
            {
                weaponNextButton.interactable = false;
                charNextButtonText.text = $"{isReadies}/{clientReady.Count} ready";
            }
        }
    }

    void UnreadyAll()
    {
        foreach (KeyValuePair<ulong, ReadyUpUIItems> entry in clientItems)
        {
            entry.Value.platform.SetActive(false);
        }
    }

    [ClientRpc]
    void ReadyUpClientRpc(bool ready, ulong receivedClientId)
    {
        clientItems[receivedClientId].platform.SetActive(ready);
    }

    public void WeaponsSelected()
    {

    }

    //public void StartGame()
    //{
    //    NetworkManager.SceneManager.LoadScene("TestingScene",UnityEngine.SceneManagement.LoadSceneMode.Single);
    //}



    [ClientRpc]
    public void LoadWeaponUIClientRpc()
    {
        charUI.SetActive(false);
        weaponUI.SetActive(true);
        UnreadyAll();
    }

}

[System.Serializable]
public class ReadyUpUIItems
{
    public GameObject avatarPreview;
    public GameObject platform;
    public SideClickerManager sideClickerManager;
    public TextMeshPro userNameDisplay;
}