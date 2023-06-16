using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Load servers found through steam
/// </summary>
public class ServersManager : MonoBehaviour
{
    [SerializeField] GameObject openSteam;
    [SerializeField] private ServerItem serverItemPrefab;
    [SerializeField] private GameObject serverList;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        if (!SteamClient.IsValid)
        {
            openSteam.SetActive(true);
            return;
        }
        if (!SteamClient.IsLoggedOn)
        {
            openSteam.SetActive(true);
            return;
        }
        //SteamGameNetworkManager.Instance.UpdateRichPresenceStatus("Fixing bugs from Thomas");
        //SteamFriends.SetRichPresence("steam_display", "Fixing bugs from Thomas");
        //Debug.Log(SteamFriends.GetRichPresence("steam_display"));
        LoadFriendServers();
        LobbyQuery lobbyQuery = new LobbyQuery();
        lobbyQuery.FilterDistanceClose();
        lobbyQuery.WithSlotsAvailable(1);
        LoadLobbies(lobbyQuery);
    }

    async void LoadLobbies(LobbyQuery query)
    {
        Lobby[] lobbies = await query.RequestAsync();
        foreach (Lobby lobby in lobbies)
        {
            ServerItem serverItem = Instantiate(serverItemPrefab, serverList.transform);
            serverItem.SetServerOwnerId(lobby.Owner.Id);
        }

    }

    async void LoadFriendServers()
    {
        IEnumerable<Friend> friends = SteamFriends.GetFriends();
        foreach (Friend friend in friends)
        {
            // Access friend properties or perform actions
            string friendName = friend.Name;
            if (friend.IsPlayingThisGame)//Friend is playing
            {
                if (friend.GameInfo.Value.Lobby.HasValue)//Friend is in a server
                {
                    Debug.Log(friend.GameInfo.Value.Lobby.Value.MemberCount);
                        Debug.Log(friend.Name);

                        ServerItem serverItem = Instantiate(serverItemPrefab, serverList.transform);
                        serverItem.SetServerOwnerId(friend.Id);

                        //await SteamMatchmaking.JoinLobbyAsync(friend.Id);
                }
            }
        }
    }
}
