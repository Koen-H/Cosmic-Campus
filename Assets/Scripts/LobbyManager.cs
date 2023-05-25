using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] GameObject playerObj;
    Dictionary<ulong, ClientManager> clients = new Dictionary<ulong, ClientManager>();
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

    public void AddClient(ulong id, ClientManager newClient)
    {
        clients.Add(id, newClient);
        OnNewClientJoined.Invoke(newClient);
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

    public int ConnectedClientsAmount()
    {
        return clients.Count;
    }

    public void CreateCharacters()
    {

        if (spawnLocation == null) spawnLocation = this.gameObject;
        foreach (KeyValuePair<ulong, NetworkClient> client in NetworkManager.Singleton.ConnectedClients)
        {
            
            GameObject obj = Instantiate(playerObj, spawnLocation.transform.position, Quaternion.LookRotation(spawnLocation.transform.forward));
            ClientManager clientManager = client.Value.PlayerObject.GetComponent<ClientManager>();
            clientManager.playerCharacter = obj;
            //obj.GetComponent<PlayerCharacterController>().InitCharacter(client.Value.ClientId);
            NetworkObject networkObj = obj.GetComponent<NetworkObject>();
            networkObj.SpawnWithOwnership(client.Value.ClientId);
        }
    }

}
