using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

    [SerializeField] bool spawnOnNetwork;

    [SerializeField] GameObject enemyPrefab;//Enemy to spawn.
    private string inputCheatCode = "";
    private const string cheatCode = "killenemies";

    GameObject spawnedEnemy;


    private void Update()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if(Input.GetKeyDown(KeyCode.P)) SpawnEnemy();
        }

        CheckCheatCodes();
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
        spawnedEnemy = enemy.gameObject;
    }
    void CheckCheatCodes()
    {
        foreach (char c in Input.inputString)
        {
            if (c == '\b')  // backspace character
            {
                Debug.Log("backspace Pressed");
                if (inputCheatCode.Length != 0)
                {
                    inputCheatCode = inputCheatCode.Substring(0, inputCheatCode.Length - 1);
                }
            }
            else if ((c == '\n') || (c == '\r')) // newline or return
            {
                // maybe you want to do something when the player hits return?
                Debug.Log("Enter Pressed");
            }
            else
            {
                inputCheatCode += c;

                // Check if the cheat code was entered
                if (inputCheatCode.Contains(cheatCode))
                {
                    if (spawnedEnemy == null) return;
                    Destroy(spawnedEnemy.gameObject);
                    // Remove cheatCode from inputCheatCode, keeping any characters that were entered after the cheat code
                    int index = inputCheatCode.IndexOf(cheatCode);
                    inputCheatCode = inputCheatCode.Substring(index + cheatCode.Length);
                }
            }
        }
    }
}
