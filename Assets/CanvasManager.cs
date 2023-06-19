using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{

    [Header("Prompts")]
    [SerializeField] TextMeshProUGUI revivePrompt;


    [SerializeField] TextMeshProUGUI engineerPrompt;


    [SerializeField] List<ArtistUIAbility> artistUIAbilities;


    [SerializeField] GameObject loadingScreen;
    [SerializeField] private GameObject gameUI;
    [SerializeField] TextMeshProUGUI loadingHint;
    private static CanvasManager _instance;
    public static CanvasManager Instance
    {
        get
        {
            if (_instance == null) Debug.LogError("CanvasManager is null");
            return _instance;
        }
    }
    [SerializeField] private List<PlayerUIItem> uiItems = new List<PlayerUIItem>();


    private void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }



    public void ToggleLoadingScreen(bool toggle)
    {
        loadingScreen.SetActive(toggle);
        if (toggle) loadingHint.text = "Please do not jump off the map";
    }

    public void LoadGameUI()
    {
        Debug.Log("Loading game ui1");
        gameUI.SetActive(true);
        Dictionary<ulong,ClientManager> clients = LobbyManager.Instance.GetClients();

        ulong myId = NetworkManager.Singleton.LocalClientId;

        uiItems[0].SetClient(clients[myId]);
        uiItems[0].LoadCorrectUI();
        uiItems[0].gameObject.SetActive(true);
        int i = 1;

        foreach(KeyValuePair<ulong,ClientManager> client in clients)
        {
            if (client.Key == myId) continue;
            uiItems[i].gameObject.SetActive(true);
            uiItems[i].SetClient(client.Value);
            uiItems[i].LoadCorrectUI();
            i++;
        }
    }

    public void ToggleRevive(bool toggle)
    {
        revivePrompt.gameObject.SetActive(toggle);
    }

    public void SetEngineerPrompt(string prompt, bool enabled = true) {
        engineerPrompt.text = prompt;
        engineerPrompt.gameObject.SetActive(enabled);
    }


    public void UpdateArtistUI(List<ArtistPaintColor> paintBucket)
    {
        for(int i = 0; i < artistUIAbilities.Count; i++)
        {
            if(paintBucket.Count > i)
            {
                artistUIAbilities[i].UpdateImage(paintBucket[i]);
            }
            else
            {
                artistUIAbilities[i].UpdateImage(ArtistPaintColor.NONE);
            }
            
        }
    }

    public void SetCooldown(float value)
    {
        uiItems[0].SetCooldown(value);
    }
}
