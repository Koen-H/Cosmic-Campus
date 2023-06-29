using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.GridLayoutGroup;

public class ServerItem : MonoBehaviour
{
    SteamId serverOwnerId;
    [SerializeField] private RawImage avatarImage;
    [SerializeField] private TextMeshProUGUI playerNameText;
    Friend steamPlayer;
    Lobby lobby;

    public void SetServerOwnerId(SteamId ownerId)
    {
        serverOwnerId = ownerId;
        steamPlayer = new Friend(ownerId);
        SetServerData();
        playerNameText.text = steamPlayer.Name;
    }

    public void SetServerOwnerId(Lobby lobby)
    {
        serverOwnerId = lobby.Id;
        steamPlayer = new Friend(serverOwnerId);
        SetServerData();
        playerNameText.text = lobby.GetData("name"); ;
    }

    public async void SetServerData()
    {
        var avatarTask = SteamAvatarTest.GetAvatar(serverOwnerId);

        await Task.WhenAll(avatarTask);
        var avatar = await avatarTask;

        if (avatar != null)
        {
            Texture2D avatarTexture = SteamAvatarTest.Convert(avatar);
            avatarImage.texture = avatarTexture;
        }
        playerNameText.text = steamPlayer.Name;
    }


    public async void TryJoinServer()
    {
        Lobby ?lobbyJoined = await SteamMatchmaking.JoinLobbyAsync(serverOwnerId);
        if (lobbyJoined == null)
        {
            Debug.Log("failed to connect");
        }
        else
        {
            Debug.Log("Lobby joined value" + lobbyJoined.Value);
        }
    }
}
