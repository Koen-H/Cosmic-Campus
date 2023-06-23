using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
//using Inworld;
//using Inworld.Sample;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public RoomGenerator levelGenerator;
    //[SerializeField] InworldController inworldController;
    //[SerializeField] InworldPlayer InworldPlayer;
    [SerializeField] GameObject npc; 
    Dictionary<ulong, bool> deadClients;
    [SerializeField] private bool useFairPlay = true;
    [SerializeField] private bool useWhiteEnemies = true;
    private List<EnemyType> allowedEnemyTypes;

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
        StartCoroutine(GenerateNextFrame());
        
    }
    IEnumerator GenerateNextFrame()
    {
        levelGenerator.SetSeed();
        levelGenerator.GenerateMapClientRpc(levelGenerator.GetSeed());
        deadClients = new();
        foreach (ulong playerId in LobbyManager.Instance.GetClients().Keys) deadClients.Add(playerId, false);
        yield return new WaitForFixedUpdate();
        LobbyManager.Instance.CreateCharacters(levelGenerator.initialSpawnLocation);
        LoadEnemyTypes();
        ToggleLoadingScreenClientRpc(false);
        LoadGameUIClientRpc();

        //inworldController.m_InworldPlayer = ClientManager.MyClient.playerCharacter.gameObject;
        //var playerController = Instantiate(InworldPlayer, ClientManager.MyClient.playerCharacter.transform);
        //Instantiate(npc, inworldController.transform);
        //playerController.m_GlobalChatCanvas.transform.SetParent(CanvasManager.Instance.GetGameUI().transform,false);

    }


    public void LoadEnemyTypes()
    {
        allowedEnemyTypes = new();
        if (useWhiteEnemies) allowedEnemyTypes.Add(EnemyType.WHITE);
        if (!useFairPlay)
        {
            allowedEnemyTypes.Add(EnemyType.ARTIST);
            allowedEnemyTypes.Add(EnemyType.DESIGNER);
            allowedEnemyTypes.Add(EnemyType.ENGINEER);
            return;
        }

        Dictionary<ulong, ClientManager> clients = LobbyManager.Instance.GetClients();
        foreach (ClientManager client in clients.Values) allowedEnemyTypes.Add(client.playerCharacter.damageType);
    }


    public EnemyType GetEnemyType(EnemyType oldType = EnemyType.NONE)
    {
        EnemyType newType = allowedEnemyTypes[UnityEngine.Random.Range(0, allowedEnemyTypes.Count)];

        while(newType == oldType) newType = GetEnemyType(oldType);

        return newType;
    }

    [ClientRpc]
    public void ToggleLoadingScreenClientRpc(bool toggle)
    {
        CanvasManager.Instance.ToggleLoadingScreen(toggle);
    }

    [ClientRpc]
    public void LoadGameUIClientRpc()
    {
        CanvasManager.Instance.LoadGameUI();
    }


    public void PlayerDeadStatus(ulong playerId, bool deadStatus)
    {
        Debug.Log(playerId);
        Debug.Log(deadClients.Count);
        deadClients[playerId] = deadStatus;
        CheckDeaths();
    }

    public void PlayerLeft(ulong playerId)
    {
        deadClients.Remove(playerId);
    }

    void CheckDeaths()
    {
        int deadPlayers = 0;
        foreach (KeyValuePair<ulong, bool> entry in deadClients)
        {
            if (entry.Value)
            {
                deadPlayers++;
            }
        }
        if (deadPlayers == deadClients.Count) OnAllPlayersDied();
    }

    void OnAllPlayersDied()
    {
        Debug.Log("All players died");
        StartCoroutine(AfterDead());
    }

    IEnumerator AfterDead()
    {
        CanvasManager.Instance.ToggleGameUI(false);
        CanvasManager.Instance.ToggleGameOverScreen(true);
        yield return new WaitForSeconds(5);//Wait for some bit
        CanvasManager.Instance.ToggleGameOverScreen(false);
        CanvasManager.Instance.ToggleLoadingScreen(true);
        if(IsServer)NetworkManager.SceneManager.LoadScene("Level 1",LoadSceneMode.Single);
        yield return null;
    }

}
