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

    [SerializeField] private GameObject remoteEngineerPrefab;

    [SerializeField] private List<NetworkObject> designerObjects;


    [Header("Artist decals")]
    [SerializeField] private GameObject whiteDecal;
    [SerializeField] private GameObject blueDecal;
    [SerializeField] private GameObject yellowDecal;
    [SerializeField] private GameObject orangeDecal;
    [SerializeField] private GameObject redDecal;
    [SerializeField] private GameObject greenDecal;
    [SerializeField] private GameObject purpleDecal;
    [SerializeField] private GameObject pinkDecal;

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

    [ServerRpc(RequireOwnership = false)]
    public void SpawnArtistDecalPrefabServerRpc(Vector3 instanceLocation, ArtistPaintColor color, ServerRpcParams serverRpcParams = default)
    {
        GameObject instance = null;

        switch (color)
        {
            case ArtistPaintColor.WHITE:
                instance = Instantiate(whiteDecal, instanceLocation, Quaternion.identity);
                break;
            case ArtistPaintColor.BLUE:
                instance = Instantiate(blueDecal, instanceLocation, Quaternion.identity);
                break;
            case ArtistPaintColor.YELLOW:
                instance = Instantiate(yellowDecal, instanceLocation, Quaternion.identity);
                break;
            case ArtistPaintColor.ORANGE:
                instance = Instantiate(orangeDecal, instanceLocation, Quaternion.identity);
                break;
            case ArtistPaintColor.RED:
                instance = Instantiate(redDecal, instanceLocation, Quaternion.identity);
                break;
            case ArtistPaintColor.GREEN:
                instance = Instantiate(greenDecal, instanceLocation, Quaternion.identity);
                break;
            case ArtistPaintColor.PURPLE:
                instance = Instantiate(purpleDecal, instanceLocation, Quaternion.identity);
                break;
            case ArtistPaintColor.PINK:
                instance = Instantiate(pinkDecal, instanceLocation, Quaternion.identity);
                break;
        }
        if (instance == null) return;
        instance.GetComponent<NetworkObject>().SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnDesignerObjectServerRpc(Vector3 instanceLocation, ServerRpcParams serverRpcParams = default)
    {
        int randomIndex = Random.Range(0, designerObjects.Count);

        Vector3 spawnSpot = new Vector3(instanceLocation.x, instanceLocation.y + 5, instanceLocation.z);
        NetworkObject instance = Instantiate(designerObjects[randomIndex], spawnSpot, Quaternion.identity);
        instance.GetComponent<ObjectSlamManager>().playerController = LobbyManager.Instance.GetClient(serverRpcParams.Receive.SenderClientId).playerCharacter;
        instance.Spawn();
    } 

}
