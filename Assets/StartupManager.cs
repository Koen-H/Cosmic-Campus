using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Netcode;

public class StartupManager : MonoBehaviour
{
    [SerializeField] private GameObject openSteam;

    [SerializeField] private uint steamAppId;
    private void Awake()
    {
        try
        {
            SteamClient.Init(steamAppId, false);
            if(SteamClient.IsValid) SetupSteam();
            else
            {
                SteamFail();
            }
            //Debug.Log(SteamClient.IsValid);
        }
        catch
        {
            Debug.LogError("ERROR");
            SteamFail();
        }
    }

    private void SteamFail()
    {
        Debug.Log("Steam failed!");
    }

    private void SetupSteam()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("SteamMenu Scene");
    }
}
