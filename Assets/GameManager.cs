using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public RoomGenerator levelGenerator;


    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null) Debug.LogError("GameManager is null");
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    public void InitalizeLevel()
    {
        //levelGenerator.SetSeed();
        levelGenerator.GenerateMapClientRpc(0);
        StartCoroutine(GenerateNextFrame());
        
    }
    IEnumerator GenerateNextFrame()
    {
        yield return new WaitForFixedUpdate();
        LobbyManager.Instance.CreateCharacters(levelGenerator.initialSpawnLocation);
        ToggleLoadingScreenClientRpc(false);
    }

    [ClientRpc]
    public void ToggleLoadingScreenClientRpc(bool toggle)
    {
        CanvasManager.Instance.ToggleLoadingScreen(toggle);
    }


}
