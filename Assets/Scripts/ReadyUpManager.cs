using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Xml;
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

    [SerializeField] List<GameObject> disableItemsOnClient = new List<GameObject>();
    [SerializeField] GameObject lobbyUI;

    [SerializeField] WeaponSideClickerManager weaponSideClickerManager;

    bool weaponsSelected;

    [SerializeField]public  List<ReadyOption> optionsTaken = new();

    [ServerRpc(RequireOwnership = false)]
    public void TakeOptionServerRpc(ReadyOption option, bool take)
    {
        TakeOptionclientRpc(option, take);
    }

    [ClientRpc]
    public void TakeOptionclientRpc(ReadyOption option, bool take)
    {
        if (take) optionsTaken.Add(option);
        else optionsTaken.Remove(option);
        clientItems[NetworkManager.LocalClientId].sideClickerManager.CheckTaken();
        weaponSideClickerManager.CheckTaken();
    }


    private void Awake()
    {
        clientItems = new Dictionary<ulong, ReadyUpUIItems>();
    }


    private void Start()
    {
        LobbyManager.OnNewClientJoined += NewClientJoined;
        SteamMatchmaking.OnLobbyEntered += LoadLobby;
    }


    public void StartNetcodeHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void StartNetcodeClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    void LoadLobby(Steamworks.Data.Lobby lobby)
    {
        foreach(GameObject obj in disableItemsOnClient) obj.SetActive(false);
        lobbyUI.SetActive(true);
    }

    void NewClientJoined(ClientManager newClient)
    {
        ulong newClientId = newClient.GetClientId();
        clientReady[newClientId] = false;
        clientItems.Add(newClientId, avatarPlatform[clientItems.Count]);
        clientItems[newClientId].sideClickerManager.gameObject.SetActive(true);
        //Give the client their front space spot on their own screen
        ulong localClientId = NetworkManager.Singleton.LocalClientId;
        if (clientItems.ContainsKey(localClientId))
        {
            if (clientItems[localClientId] != avatarPlatform[0])//If we don't have the first spot
            {
                ulong firstSpotId = FindKeyByValue(avatarPlatform[0]);
                ReadyUpUIItems myCurrentItems = clientItems[localClientId];
                ReadyUpUIItems myNewItems = clientItems[firstSpotId];
                clientItems[localClientId] = myNewItems;
                clientItems[firstSpotId] = myCurrentItems;
                UpdatePlayerCharacter(firstSpotId);
            }
        }
        UpdatePlayerCharacter(newClientId);
        CheckReady();
        newClient.OnClientLeft += ClientLeft;
    }


    void ClientLeft(ClientManager clientLeft)
    {
        ulong clientLeftId = clientLeft.GetClientId();
        if (clientLeftId == NetworkManager.Singleton.LocalClientId) return;
        clientReady.Remove(clientLeftId);
        clientItems[clientLeftId].sideClickerManager.gameObject.SetActive(false);
        UpdatePlayerCharacter(clientLeftId,true);
        clientItems.Remove(clientLeftId);

        CheckReady();
        clientLeft.OnClientLeft -= ClientLeft;//TODO Unsubscibe when scene changes
    }


    private ulong FindKeyByValue(ReadyUpUIItems value)
    {
        foreach (var pair in clientItems)
        {
            if (pair.Value == value)
            {
                return pair.Key;
            }
        }
        Debug.Log("NO KEY");
        return 0; // Key not found
    }


    public void HandleRoleValueChange(SideClickerValue newValue)
    {
        ClientManager.MyClient.playerData.playerRole.Value = (PlayerRole)int.Parse(newValue.value);
        UpdatePlayerCharacter(NetworkManager.Singleton.LocalClientId, false, int.Parse(newValue.value));
    }


    public void UpdatePlayerCharacter(ulong outdatedClientId, bool left = false, int newValue = 100)
    {
        if (left)//Player left, clean up the spot
        {
            GameObject platform = clientItems[outdatedClientId].avatarPreview;
            foreach (Transform child in platform.transform) Destroy(child.gameObject);
            return;
        }

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
                try {
                    playerRoleData = LobbyManager.Instance.GetClient(outdatedClientId).playerData.playerRoleData;}
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
        UpdatePlayerCharacter(clientId,false, newRoleInt);
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
        UpdatePlayerCharacter(receivedClientId, false, int.Parse(sideClickerManager.GetValue()));
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
        charNextButton.gameObject.SetActive(true);
        weaponNextButton.gameObject.SetActive(true);
        int isReadies = 0;
        foreach (KeyValuePair<ulong, bool> entry in clientReady)
        {
            if (entry.Value)
            {
                isReadies++;
            }
        }
        if (isReadies == clientReady.Count)
        {
            if (!weaponsSelected)
            {
                if(IsServer)charNextButton.interactable = true;
                charNextButtonText.text = "Everyone ready!";
            }
            else
            {
                if (IsServer) weaponNextButton.interactable = true;
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
                weaponNextButtonText.text = $"{isReadies}/{clientReady.Count} ready";
            }
        }
        DiscordManager.Instance.UpdateStatus("Waiting for friends", $"{isReadies}/{clientReady.Count} ready");

    }

    void UnreadyAll()
    {
        foreach (KeyValuePair<ulong, ReadyUpUIItems> entry in clientItems)
        {
            entry.Value.platform.GetComponent<ReadyUpPlatform>().ChangeColor(false);
        }
        if (!IsServer) return;
        List<ulong> keys = new List<ulong>(clientReady.Keys);
        foreach (ulong key in keys)
        {
            clientReady[key] = false;
        }
    }

    [ClientRpc]
    void ReadyUpClientRpc(bool ready, ulong receivedClientId)
    {
        clientReady[receivedClientId] = ready;
        CheckReady();
        clientItems[receivedClientId].platform.GetComponent<ReadyUpPlatform>().ChangeColor(ready);
    }

    public void SelectWeapon(int weaponInt)
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;
        LobbyManager.Instance.GetClient(clientId).playerData.weaponId.Value = weaponInt;
    }

    [ClientRpc]
    public void EnableLoadingScreenClientRpc()
    {
        CanvasManager.Instance.ToggleLoadingScreen(true);

    }

    public void StartGame()
    {
        if(SteamGameNetworkManager.Instance.CurrentLobby != null)SteamGameNetworkManager.Instance.CurrentLobby.Value.SetJoinable(false);
        EnableLoadingScreenClientRpc();
        NetworkManager.SceneManager.LoadScene("Level 1",UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private void OnDisable()
    {
        Dictionary<ulong, ClientManager> clients = LobbyManager.Instance.GetClients();
        foreach (ClientManager client in clients.Values) client.OnClientLeft -= ClientLeft;
    }



    [ClientRpc]
    public void LoadWeaponUIClientRpc()
    {
        weaponsSelected = true;
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
public enum ReadyOption { ARTIST, DESIGNER, ENGINEER, BOW, SWORD, STAFF }
