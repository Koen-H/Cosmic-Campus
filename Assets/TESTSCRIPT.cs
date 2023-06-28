using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TESTSCRIPT : MonoBehaviour
{
    public void ButtonClick()
    {
        SteamGameNetworkManager.Instance.UseSteam(true);
    }

    public void StartHost()
    {
        SteamGameNetworkManager.Instance.StartHost(3);
    }
    public void JoinFriend()
    {
        SteamGameNetworkManager.Instance.JoinFriend();
    }

    public void NetCode()
    {
        SteamGameNetworkManager.Instance.UseSteam(false);
    }
}
