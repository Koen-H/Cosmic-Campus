using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
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
}
