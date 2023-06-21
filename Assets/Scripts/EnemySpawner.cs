using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

    [SerializeField] bool spawnOnNetwork;

    [SerializeField] GameObject enemyPrefab;//Enemy to spawn.

    private void Update()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if(Input.GetKeyDown(KeyCode.P)) SpawnEnemy();
        }
    }

    private void Start()
    {
        if (spawnOnNetwork && NetworkManager.Singleton.IsServer)
        {
            SpawnEnemy();
        }
    }

    public void SpawnEnemy()
    {
        NetworkObject enemy = Instantiate(enemyPrefab,transform.position, Quaternion.LookRotation(transform.forward)).GetComponent<NetworkObject>();
        enemy.Spawn(true);
    }
}
