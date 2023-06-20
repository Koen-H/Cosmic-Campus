using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using Inworld;
using Inworld.Sample;

public class GameManager : NetworkBehaviour
{
    public RoomGenerator levelGenerator;
    [SerializeField] InworldController inworldController;
    [SerializeField] InworldPlayer InworldPlayer;
    [SerializeField] GameObject npc; 


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
    private void Start()
    {

    }

    public void InitalizeLevel()
    {
        StartCoroutine(GenerateNextFrame());
        
    }
    IEnumerator GenerateNextFrame()
    {
        levelGenerator.SetSeed();
        levelGenerator.GenerateMapClientRpc(levelGenerator.GetSeed());
        yield return new WaitForFixedUpdate();
        LobbyManager.Instance.CreateCharacters(levelGenerator.initialSpawnLocation);
        ToggleLoadingScreenClientRpc(false);
        LoadGameUIClientRpc();

        Debug.Log("SHit: " + ClientManager.MyClient.playerCharacter.gameObject);
        Debug.Log("Fuck: " + inworldController);
        inworldController.m_InworldPlayer = ClientManager.MyClient.playerCharacter.gameObject;
        var playerController = Instantiate(InworldPlayer, ClientManager.MyClient.playerCharacter.transform);
        Instantiate(npc, inworldController.transform);
        playerController.m_GlobalChatCanvas.transform.SetParent(CanvasManager.Instance.GetGameUI().transform,false);

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


}
