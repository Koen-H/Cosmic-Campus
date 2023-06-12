using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomInfo : MonoBehaviour
{
    public Transform doorEntrance;
    public Transform doorExit;

    public Vector3 normalEntrance;
    public Vector3 normalExit;

    public Vector3 GetEntrancePosition() => doorEntrance.position;
    public Vector3 GetExitPosition() =>  doorExit.position;

    public List<EnemySpawner> enemySpawners= new List<EnemySpawner>();

}
