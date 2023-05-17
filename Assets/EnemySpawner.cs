using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab;
    public void SpawnEnemy()
    {
        NetworkObject enemy = Instantiate(enemyPrefab,transform.position, Quaternion.identity).GetComponent<NetworkObject>();
        enemy.Spawn();
    }
}
