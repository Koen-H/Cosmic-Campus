using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TESTSCRIPT : MonoBehaviour
{
    public void ButtonClick()
    {
        SteamGameNetworkManager.Instance.UseSteam(true);
        Debug.Log(LobbyManager.Instance.GetClients().Count);
    }

    public void StartHost()
    {
        SteamGameNetworkManager.Instance.StartHost(3);
    }
    public void JoinFriend()
    {
        SteamGameNetworkManager.Instance.JoinFriend();
    }
}
