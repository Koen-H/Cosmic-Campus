using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServerItem : MonoBehaviour
{
    SteamId serverOwnerId;
    [SerializeField] private RawImage avatarImage;
    [SerializeField] private TextMeshProUGUI playerNameText;
    Friend steamPlayer;

    public void SetServerOwnerId(SteamId ownerId)
    {
        serverOwnerId = ownerId;
        steamPlayer = new Friend(ownerId);
        SetServerData();
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
        await SteamMatchmaking.JoinLobbyAsync(serverOwnerId);
        Debug.Log(await SteamMatchmaking.JoinLobbyAsync(serverOwnerId));
    }
}
