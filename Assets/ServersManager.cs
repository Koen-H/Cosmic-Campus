using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Load servers found through steam
/// </summary>
public class ServersManager : MonoBehaviour
{

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
        if(SteamClient.IsLoggedOn) LoadFriendServers();
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

                        Debug.Log(friend.Name);

                        ServerItem serverItem = Instantiate(serverItemPrefab, serverList.transform);
                        serverItem.SetServerOwnerId(friend.Id);

                        //await SteamMatchmaking.JoinLobbyAsync(friend.Id);
                }
            }
        }
    }
}
