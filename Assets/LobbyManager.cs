using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] GameObject playerObj;
    void CreateCharacters()
    {

        foreach (KeyValuePair<ulong, NetworkClient> kvp in NetworkManager.Singleton.ConnectedClients)
        {
            GameObject obj = Instantiate(playerObj);
            //obj.GetComponent<NetworkObject>().ChangeOwnership(NetworkClient);
           // kvp.Value.PlayerObject

        }
    }
}
