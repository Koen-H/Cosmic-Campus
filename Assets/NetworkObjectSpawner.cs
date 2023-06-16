using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkObjectSpawner : MonoBehaviour
{

    [SerializeField] bool spawnOnNetwork;

    [SerializeField] GameObject networkObject;//Enemy to spawn.

    private void Update()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (Input.GetKeyDown(KeyCode.P)) SpawnObject();
        }
    }

    private void Start()
    {
        if (spawnOnNetwork && NetworkManager.Singleton.IsServer)
        {
            SpawnObject();
        }
    }

    public void SpawnObject()
    {
        NetworkObject enemy = Instantiate(networkObject, transform.position, Quaternion.LookRotation(transform.forward)).GetComponent<NetworkObject>();
        enemy.Spawn();
    }
}
