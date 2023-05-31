using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// For when a client needs to spawn a network prefab. 
/// You can ask it from the serverspawner as the clients can't spawn network objects.
/// </summary>
public class ServerSpawner : NetworkBehaviour
{

    [SerializeField] GameObject remoteEngineerPrefab;


    private static ServerSpawner _instance;
    public static ServerSpawner Instance
    {
        get
        {
            if (_instance == null) Debug.LogError("ServerSpawner is null");
            return _instance;
        }
    }

    public override void OnNetworkSpawn()
    {
        if(_instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnRemoteEngineerPrefabServerRpc(Vector3 instanceLocation, ServerRpcParams serverRpcParams = default)
    {
        GameObject instance = Instantiate(remoteEngineerPrefab,instanceLocation, Quaternion.identity);
        instance.GetComponent<NetworkObject>().SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);
    }
}
