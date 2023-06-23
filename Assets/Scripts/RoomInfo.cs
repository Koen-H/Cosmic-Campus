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

    public Transform doorLeft;
    public Transform doorRight;


    public Vector3 GetEntrancePosition() => doorEntrance.position;
    public Vector3 GetExitPosition() =>  doorExit.position;
    public Vector3 GetStudentPosition() => studentSpawnPoint.position;
    public Vector3 GetTeacherPosition() => teacherSpawnPoint.position;

    public List<EnemySpawner> enemySpawners= new List<EnemySpawner>();

    public List<NetworkObjectSpawner> networkObjectSpawners = new List<NetworkObjectSpawner>();

    public List<Transform> cmgtTransformSpawnpoints = new();

    private void Start()
    {
        if(roomLayer == 0 || roomLayer == 1)
        {
            RoomGenerator.Instance.SpawnEnemiesInRoomServerRpc(roomLayer);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerCharacterController player = other.GetComponent<PlayerCharacterController>();
        if (player)
        {
            RoomGenerator.Instance.SpawnEnemiesInRoomServerRpc(roomLayer + 1);
            player.checkPoint = doorEntrance.position;
        }


    }

}
