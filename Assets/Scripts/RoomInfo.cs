using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RoomInfo : MonoBehaviour
{
    public Transform doorEntrance;
    public Transform doorExit;

    [HideInInspector] public int roomLayer;

    public Vector3 normalEntrance;
    public Vector3 normalExit;

    public Transform studentSpawnPoint; 
    public Transform teacherSpawnPoint;

    public Vector3 GetEntrancePosition() => doorEntrance.position;
    public Vector3 GetExitPosition() =>  doorExit.position;
    public Vector3 GetStudentPosition() => studentSpawnPoint.position;
    public Vector3 GetTeacherPosition() => teacherSpawnPoint.position;

    public List<EnemySpawner> enemySpawners= new List<EnemySpawner>();

    public List<NetworkObject> potions = new List<NetworkObject>();


    private void OnTriggerEnter(Collider other)
    {
        PlayerCharacterController player = other.GetComponent<PlayerCharacterController>();
        if (player)
        {
            RoomGenerator.Instance.SpawnEnemiesInRoomServerRpc(roomLayer);
            player.checkPoint = doorEntrance.position;
        }


    }

}
