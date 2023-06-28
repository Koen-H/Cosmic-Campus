using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class SteamGameNetworkManager : MonoBehaviour
{


    public static SteamGameNetworkManager Instance { get; private set; } = null;
    public Lobby? CurrentLobby { get; private set; } = null;

    private FacepunchTransport transport = null;
    private FacepunchTransport facePunchTransport;
    private UnityTransport unityTransport;
    

    #region unity

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        facePunchTransport = this.GetComponent<FacepunchTransport>();
        unityTransport = this.GetComponent<UnityTransport>();
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
    public void OnApplicationQuit() => Disconnect();

    #endregion

    /// <summary>
    /// To enable the steam (facepunch) transport
    /// </summary>
    /// <param name="toggle">Wheter to use it or not</param>
    public void UseSteam(bool toggle = false)   
    {
        facePunchTransport.enabled = toggle;//The steam transport
        unityTransport.enabled = !toggle;//The unity transport
        NetworkTransport selectedTransport = toggle ? facePunchTransport : unityTransport;
        NetworkManager.Singleton.NetworkConfig.NetworkTransport = selectedTransport;
        this.enabled = toggle;
    }


    /// <summary>
    /// Open the friendlist overlay in steam.
    /// </summary>
    public void OpenFriends()
    {
        SteamFriends.OpenGameInviteOverlay(CurrentLobby.Value.Id);
    }


    public async void StartHost(int maxMembers = 3)
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

    /// <summary>
    /// Tries to join a friend that's currently playing
    /// </summary>
    public async void JoinFriend()
    {
        IEnumerable<Friend> friends = SteamFriends.GetFriends();

        foreach (Friend friend in friends)
        {
            // Access friend properties or perform actions
            string friendName = friend.Name;
            if (friend.IsPlayingThisGame)
            {
                Debug.Log($"{friendName} is playing this game, trying to join!");
                //await friend.GameInfo.Value.Lobby.Value.Join();
                //transport.targetSteamId = friend.Id;
                await SteamMatchmaking.JoinLobbyAsync(friend.Id);
                if (CurrentLobby != null) break;
            }
        }
        //LobbyQuery query = new LobbyQuery();
        //query.FilterDistanceClose();
        //Lobby[] lobbies = await query.RequestAsync();
        //foreach(Lobby lobby in lobbies)
        //{
        //    Debug.Log(lobby.Id);
        //    if (lobby.IsOwnedBy(SteamClient.SteamId))
        //    {
        //        await lobby.Join();

        //    }
        //    //Debug.Log(lobby.GetData("key"));

        //}

        //string PlayerName = SteamClient.Name;
        //SteamId PlayerSteamId = SteamClient.SteamId;
        //string playerSteamIdString = PlayerSteamId.ToString();
        //SteamMatchmaking.LobbyList.FilterDistanceClose();
        //SteamFriends.SetRichPresence("gamestatus", "winning");
        //Debug.Log("Should be set");
        //await SteamMatchmaking.JoinLobbyAsync();
    }

    public void StartClient(SteamId steamId)
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;

        transport.targetSteamId = steamId;

        if (NetworkManager.Singleton.StartClient()) Debug.Log("Succesfully joined", this);
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
        //lobby.SetFriendsOnly();
        lobby.SetPublic();
        lobby.SetData("name", "Awesome lobby name");
        lobby.SetJoinable(true);
        Debug.Log($"Lobby {lobby.Id} has been created with name {lobby.GetData("name")}!", this);
    }
    private void OnLobbyEntered(Lobby lobby)
    {
        if (NetworkManager.Singleton.IsHost) return;
        Debug.Log($"entered lobby with id {lobby.Id}");
        StartClient(lobby.Id);
    }
    private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
    {
        Debug.Log($"{friend.Name} joined the lobby");
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
        Debug.Log("Lobby game created!");
    }
    private void OnGameLobbyJoinRequested(Lobby lobby, SteamId steamId)
    {
        StartClient(steamId);
    }
    #endregion

    /// <summary>
    /// Set the rich pressence on steam
    /// </summary>
    /// <param name="value"></param>
    public void UpdateRichPresenceStatus(string value)
    {
        
        Debug.Log(SteamFriends.SetRichPresence("steam_display ", value));
    }
}
