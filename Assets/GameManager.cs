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
    private string inputCheatCode = "";
    private const string cheatCode = "nextisland";
    private const string cheatCode2 = "lastisland";

    [SerializeField] private ListSO INWorldTeachers;

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
        foreach (ClientManager client in LobbyManager.Instance.GetClients().Values)
        {
            deadClients.Add(client.GetClientId(), false);
            client.OnClientLeft += PlayerLeft;
        }
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

    private void OnDisable()
    {
        foreach (ClientManager client in LobbyManager.Instance.GetClients().Values)
        {
            client.OnClientLeft -= PlayerLeft;
        }
    }


    public void LoadEnemyTypes(ulong clientLeft = ulong.MaxValue)
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
        foreach (ClientManager client in clients.Values)
        {
            if (clientLeft == client.GetClientId()) continue;
            allowedEnemyTypes.Add(client.playerCharacter.damageType);
        }
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
        deadClients[playerId] = deadStatus;
        CheckDeaths();
    }

    public void PlayerLeft(ClientManager clientLeft)
    {
        ulong clinetLeftID = clientLeft.GetClientId();
        deadClients.Remove(clinetLeftID);
        clientLeft.OnClientLeft -= PlayerLeft;
        LoadEnemyTypes(clinetLeftID);
        CheckDeaths();
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


    /// <summary>
    /// Happens on the server, when the boss is destroyed.
    /// </summary>
    public void OnBossDestroy()
    {
        int index = UnityEngine.Random.Range(0, INWorldTeachers.GetCount());
        SpawnInworldTeacherClientRpc(index);
    }

    [ClientRpc]
    void SpawnInworldTeacherClientRpc(int index)
    {
        GameObject teacher = INWorldTeachers.GetGameObject(index);
        Transform spawnPoint = levelGenerator.lastBossRoom.teacherSpawnPoint;
        GameObject teacherInstance =  Instantiate(teacher, spawnPoint.position, spawnPoint.rotation);
    }

    void CheckCheatCodes()
    {
        foreach (char c in Input.inputString)
        {
            if (c == '\b')  // backspace character
            {
                Debug.Log("backspace Pressed");
                if (inputCheatCode.Length != 0)
                {
                    inputCheatCode = inputCheatCode.Substring(0, inputCheatCode.Length - 1);
                }
            }
            else if ((c == '\n') || (c == '\r')) // newline or return
            {
                // maybe you want to do something when the player hits return?
                Debug.Log("Enter Pressed");
            }
            else
            {
                inputCheatCode += c;

                // Check if the cheat code was entered
                if (inputCheatCode.Contains(cheatCode))
                {
                    Debug.Log("Cheat Code Activated");
                    Room nextRoom = levelGenerator.GetCorrectPathRoom(levelGenerator.LatestEnemyLayer+1);
                    if (nextRoom != null)
                    {
                        Vector3 tpPosition = nextRoom.roomPrefab.doorEntrance.position + nextRoom.GetRoomPosition();
                        ClientManager.MyClient.playerCharacter.transform.position = tpPosition;
                    }

                    // Remove cheatCode from inputCheatCode, keeping any characters that were entered after the cheat code
                    int index = inputCheatCode.IndexOf(cheatCode);
                    inputCheatCode = inputCheatCode.Substring(index + cheatCode.Length);
                }
                if (inputCheatCode.Contains(cheatCode2))
                {
                    Debug.Log("Cheat Code Activated");
                    GoToLastRoom();

                    // Remove cheatCode from inputCheatCode, keeping any characters that were entered after the cheat code
                    int index = inputCheatCode.IndexOf(cheatCode);
                    inputCheatCode = inputCheatCode.Substring(index + cheatCode.Length);
                }
            }
        }
    }
    void GoToLastRoom()
    {
        Room nextRoom = levelGenerator.GetCorrectPathRoom(levelGenerator.numberOfRooms+1);
        if (nextRoom != null)
        {
            Vector3 tpPosition = nextRoom.roomPrefab.doorEntrance.position + nextRoom.GetRoomPosition();
            ClientManager.MyClient.playerCharacter.transform.position = tpPosition;
            levelGenerator.SpawnEnemiesInRoomServerRpc(levelGenerator.numberOfRooms+1);
        }
    }

}
