using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] GameObject playerObj;
    Dictionary<ulong, ClientManager> clients = new Dictionary<ulong, ClientManager>();

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
        Debug.Log("client added!");
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

    public void CreateCharacters()
    {

        foreach (KeyValuePair<ulong, NetworkClient> client in NetworkManager.Singleton.ConnectedClients)
        {
            
            GameObject obj = Instantiate(playerObj);
            ClientManager clientManager = client.Value.PlayerObject.GetComponent<ClientManager>();
            clientManager.playerCharacter = obj;
            //obj.GetComponent<PlayerCharacterController>().InitCharacter(client.Value.ClientId);
            NetworkObject networkObj = obj.GetComponent<NetworkObject>();
            networkObj.SpawnWithOwnership(client.Value.ClientId);
        }
    }

}
