using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkObjectSpawner : MonoBehaviour
{

    [SerializeField] private bool spawnOnNetwork;

    [SerializeField] private GameObject networkObject;//Network Object

    private void Start()
    {
        if (spawnOnNetwork && NetworkManager.Singleton.IsServer)
        {
            SpawnObject();
        }
    }
    private void Update()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (Input.GetKeyDown(KeyCode.P)) SpawnObject();
        }
    }


    public void SpawnObject()
    {
        NetworkObject netObj = Instantiate(networkObject, transform.position, Quaternion.LookRotation(transform.forward)).GetComponent<NetworkObject>();
        netObj.Spawn(true);
    }
}
