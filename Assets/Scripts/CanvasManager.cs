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
    [SerializeField] TextMeshProUGUI interactPrompt;

    Camera cam;

    [SerializeField] List<ArtistUIAbility> artistUIAbilities;

    [SerializeField] UiDirectionIndicatorManager uiDirectionIndicatorManager;
    [SerializeField] UiMoneyDroppedElement enemyDamageIndicator; 


    [SerializeField] GameObject loadingScreen;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject settingsUI;
    [SerializeField] private GameObject connectionLostUI;
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
        if(_instance != null)
        {
            Destroy(gameObject);
                return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SpawnDamageText(Vector3 worldPosition, int damge)
    {
        if(cam == null) cam = CameraManager.MyCamera.GetCamera();
        var newDamageTest = Instantiate(enemyDamageIndicator, uiDirectionIndicatorManager.transform);
        newDamageTest.SetText(damge.ToString()); 
        newDamageTest.SetPosition(worldPosition, cam);
    }



    public void DisableAll()
    {
        loadingScreen.SetActive(false);
        gameUI.SetActive(false);
        gameOverUI.SetActive(false);
        settingsUI.SetActive(false);
    }

    public void ToggleLoadingScreen(bool toggle)
    {
        loadingScreen.SetActive(toggle);
        if (toggle) loadingHint.text = "Please do not jump off the map";
    }
    public GameObject GetGameUI()
    {
        return gameUI;
    } 

    public void ToggleGameOverScreen(bool toggle)
    {
        gameOverUI.SetActive(toggle);
    }

    public void ToggleGameUI(bool toggle)
    {
        gameUI.SetActive(toggle);
    }

    public void ToggleSettingsUI(bool toggle)
    {
        settingsUI.SetActive(toggle);
        ClientManager.MyClient.playerCharacter.inSettings = toggle;
    }

    public void ToggleConnectionLostUI(bool toggle)
    {
        connectionLostUI.SetActive(toggle);
    }


    public void LoadGameUI(ulong clientLeft = ulong.MaxValue)
    {
        foreach(PlayerUIItem item in uiItems) item.DisableAll();
        gameUI.SetActive(true);
        Dictionary<ulong,ClientManager> clients = LobbyManager.Instance.GetClients();

        ulong myId = NetworkManager.Singleton.LocalClientId;

        uiItems[0].SetClient(clients[myId]);
        uiItems[0].LoadCorrectUI();
        uiItems[0].gameObject.SetActive(true);
        int i = 1;

        foreach(KeyValuePair<ulong,ClientManager> client in clients)
        {
            if (client.Key == myId || clientLeft == client.Key) continue;
            uiItems[i].gameObject.SetActive(true);
            uiItems[i].SetClient(client.Value);
            uiItems[i].LoadCorrectUI();
            uiDirectionIndicatorManager.AddToCharacterToTrack(client.Value.playerCharacter);
            i++;
            client.Value.OnClientLeft += ClientLeft;
        }
        uiDirectionIndicatorManager.InitiateDirections();
    }


    /// <summary>
    /// When a client leaves, we need to remove the related ui from the screen
    /// </summary>
    /// <param name="clientLeft"></param>
    public void ClientLeft(ClientManager clientLeft)
    {
        Dictionary<ulong, ClientManager> clients = LobbyManager.Instance.GetClients();
        ulong myId = NetworkManager.Singleton.LocalClientId;
        foreach (KeyValuePair<ulong, ClientManager> client in clients)
        {
            if (client.Key == myId) continue;
            client.Value.OnClientLeft -= ClientLeft;
        }
        LoadGameUI(clientLeft.GetClientId());

    }



    public void ToggleRevive(bool toggle)
    {
        revivePrompt.gameObject.SetActive(toggle);
    }

    public void SetEngineerPrompt(string prompt, bool enabled = true) {
        engineerPrompt.text = prompt;
        engineerPrompt.gameObject.SetActive(enabled);
    }

    public void ToggleInteract(bool toggle)
    {
        interactPrompt.gameObject.SetActive(toggle);
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
