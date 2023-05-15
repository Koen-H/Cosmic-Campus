using Steamworks;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class ClientManager : NetworkBehaviour
{
    [SerializeField] private NetworkVariable<ulong> clientId = new NetworkVariable<ulong>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    //public NetworkVariable<string> playerName = null;
    public NetworkVariable<uint> steamAccountId = new NetworkVariable<uint>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] public PlayerData playerData = null;

    public GameObject playerCharacter = null;

    //Get the clientManager, that belongs to you, the client.
    private static ClientManager _myClient;
    public static ClientManager MyClient
    {
        get
        {
            if (_myClient == null) Debug.LogError("MyClient is null");
            return _myClient;
        }
    }

    public override void OnNetworkSpawn()
    {
        LobbyManager.Instance.AddClient(OwnerClientId,this);

        if (!IsOwner) return;
        _myClient = this;
        clientId.Value = NetworkManager.Singleton.LocalClientId;
        if (SteamClient.IsLoggedOn == true)
        {
        //    playerName.Value = SteamClient.Name;
            steamAccountId.Value = SteamClient.SteamId.AccountId;
            this.gameObject.name = $"Client ({SteamClient.Name})";
        }
    }

    public override void OnNetworkDespawn()
    {
        LobbyManager.Instance.RemoveClient(OwnerClientId);
    }
}
