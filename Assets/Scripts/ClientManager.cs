using Steamworks;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class ClientManager : NetworkBehaviour
{
    private NetworkVariable<ulong> clientId = new NetworkVariable<ulong>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    //public NetworkVariable<string> playerName = null;
    public NetworkVariable<FixedString128Bytes> playerName = new NetworkVariable<FixedString128Bytes>("Player", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<ulong> steamId = new NetworkVariable<ulong>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> usingSteam = new NetworkVariable<bool>(false,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);
    [SerializeField] public PlayerData playerData = null;

    public PlayerCharacterController playerCharacter = null;

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
    //private void Awake()
    //{
    //    Debug.Log("hey");
    //    LobbyManager.Instance.AddClient(OwnerClientId, this);
    //    DontDestroyOnLoad(this.gameObject);
    //    if (!IsOwner) return;
    //    _myClient = this;
    //    clientId.Value = NetworkManager.Singleton.LocalClientId;
    //    Debug.Log(GetClientId());
    //    if (SteamClient.IsLoggedOn == true)
    //    {
    //        playerName.Value = SteamClient.Name;
    //        steamAccountId.Value = SteamClient.SteamId.AccountId;
    //        this.gameObject.name = $"Client ({SteamClient.Name})";
    //    }
    //}

    public override void OnNetworkSpawn()
    {
        DontDestroyOnLoad(this.gameObject);
        if (IsOwner)
        {
            _myClient = this;
            clientId.Value = NetworkManager.Singleton.LocalClientId;
            if (SteamClient.IsLoggedOn == true)
            {
                usingSteam.Value = true;
                playerName.Value = SteamClient.Name;
                steamId.Value = SteamClient.SteamId;

            }
        }
        this.gameObject.name = $"Client ({playerName.Value})";
    }
    private void Start()
    {
        LobbyManager.Instance.AddClient(OwnerClientId, this);//Do this after networkspawn so the data is synchronised

    }

    public override void OnNetworkDespawn()
    {
        LobbyManager.Instance.RemoveClient(OwnerClientId);
    }

    public ulong GetClientId()
    {
        return OwnerClientId;
    }
}
