using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab;//Enemy to spawn.

    //private void Start()
    //{
    //    if (NetworkManager.Singleton.IsServer) SpawnEnemy();
    //    Destroy(this.gameObject);
    //}

    public void SpawnEnemy()
    {
        NetworkObject enemy = Instantiate(enemyPrefab,transform.position, Quaternion.LookRotation(transform.forward)).GetComponent<NetworkObject>();
        enemy.Spawn();
    }
}
