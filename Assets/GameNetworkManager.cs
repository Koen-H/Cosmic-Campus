using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class GameNetworkManager : MonoBehaviour
{

    public static GameNetworkManager Instance { get; private set; } = null;
    public Lobby? CurrentLobby { get; private set; } = null;

    private FacepunchTransport transport = null;
    
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject); 
    }

    private void Start()
    {
        transport = GetComponent<FacepunchTransport>();

        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
        SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
    }

    public void OnApplicationQuit() => Disconnect();

    private void OnDestroy()
    {
        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite -= OnLobbyInvite;
        SteamMatchmaking.OnLobbyGameCreated -= OnLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;

        if(NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;

    }

    public async void StartHost(int maxMembers = 10)
    {
        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        NetworkManager.Singleton.StartHost();

        CurrentLobby = await SteamMatchmaking.CreateLobbyAsync(maxMembers);
    }

    public void Disconnect()
    {
        CurrentLobby?.Leave();
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.Shutdown();
    }

    public void StartClient(SteamId steamId)
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;

        transport.targetSteamId = steamId;

        if (NetworkManager.Singleton.StartClient()) Debug.Log("Client has joined", this);
    }


    #region Network Callbacks
    private void OnServerStarted()
    {
        Debug.Log($"Server started!");
    }
    private void OnClientConnectedCallback(ulong clientID)
    {
        Debug.Log($"Client connected, clientId={clientID}");
    }
    private void OnClientDisconnectCallback(ulong clientID)
    {
        Debug.Log($"Client disconnected, clientId={clientID}");
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
    } 
    #endregion

    #region Steam Callbacks
    private void OnLobbyCreated(Result result, Lobby lobby)
    {
        if(result != Result.OK)
        {
            Debug.LogError($"Lobby couldn't be createad, {result}", this);
            return;
        }
        lobby.SetFriendsOnly();
        lobby.SetData("name", "Awesome lobby name");
        lobby.SetJoinable(true);

        Debug.Log("Lobby has been created!",this);
    }
    private void OnLobbyEntered(Lobby lobby)
    {
        if (NetworkManager.Singleton.IsHost) return;
        StartClient(lobby.Id);
    }
    private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
    {
        throw new System.NotImplementedException();
    }
    private void OnLobbyMemberLeave(Lobby lobby, Friend friend)
    {
        throw new System.NotImplementedException();
    }
    private void OnLobbyInvite(Friend friend, Lobby lobby)
    {
        Debug.Log($"Invite received from { friend.Name }", this);
    }
    private void OnLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId steamId)
    {
        throw new System.NotImplementedException();
    }
    private void OnGameLobbyJoinRequested(Lobby lobby, SteamId steamId)
    {
        StartClient(steamId);
    } 
    #endregion
}
