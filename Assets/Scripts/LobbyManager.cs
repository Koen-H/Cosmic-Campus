using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] GameObject playerObj;
    private Dictionary<ulong, ClientManager> clients = new Dictionary<ulong, ClientManager>();
    public static event System.Action<ClientManager> OnNewClientJoined;
    [SerializeField] GameObject spawnLocation;


    private static LobbyManager _instance;
    public static LobbyManager Instance
    {
        get
        {
            if (_instance == null) Debug.LogError("LobbyManager is null");
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnConnectionLost;
        //NetworkManager.Singleton.OnTransportFailure += OnConnectionLost;
        //NetworkManager.Singleton.
    }



    public void AddClient(ulong id, ClientManager newClient)
    {
        clients.Add(id, newClient);
        if(OnNewClientJoined != null) OnNewClientJoined.Invoke(newClient);

        Debug.Log("client added! with id " + newClient.GetClientId());
    }
    public void RemoveClient(ulong id)
    {
        clients.Remove(id);
        Debug.Log("Client removed!");
    }

    public ClientManager GetClient(ulong id)
    {
        return clients[id];
    }

    public Dictionary<ulong,ClientManager> GetClients()
    {
        return clients;
    }

    public int ConnectedClientsAmount()
    {
        return clients.Count;
    }

    public void CreateCharactersZerozero()
    {
        CreateCharacters(Vector3.zero);
    }

    public void CreateCharacters(Vector3 spawnLoaction)
    {

        if (spawnLocation == null) spawnLocation = this.gameObject;
        foreach (KeyValuePair<ulong, NetworkClient> client in NetworkManager.Singleton.ConnectedClients)
        {
            
            GameObject obj = Instantiate(playerObj, spawnLoaction, Quaternion.LookRotation(Vector3.forward));
            ClientManager clientManager = client.Value.PlayerObject.GetComponent<ClientManager>();
            clientManager.playerCharacter = obj.GetComponent<PlayerCharacterController>();
            clientManager.playerCharacter.checkPoint = spawnLoaction;
            //obj.GetComponent<PlayerCharacterController>().InitCharacter(client.Value.ClientId);
            NetworkObject networkObj = obj.GetComponent<NetworkObject>();
            networkObj.SpawnWithOwnership(client.Value.ClientId,true);
        }

        DiscordManager.Instance.ToggleRandomUpdates(true);
    }

    public void OnConnectionLost(ulong lostClient)
    {
        Debug.Log(NetworkManager.Singleton.IsListening);
            //NetworkManager.Singleton.Shutdown();
            //UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu Scene");
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton.IsServer)
        {

        }
    }

}
