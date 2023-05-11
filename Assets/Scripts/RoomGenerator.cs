using UnityEngine;
using System.Collections.Generic;

public class RoomGenerator : MonoBehaviour
{
    public int numberOfRooms;
    public List<RoomInfo> roomPrefabs;
    private List<RoomInfo> generatedRooms = new List<RoomInfo>();

    void Start()
    {
        for (int i = 0; i < numberOfRooms; i++)
        {
            GenerateRoom();
        }
    }

    void GenerateRoom()
    {
        // Select a random room from the roomPrefabs
        RoomInfo selectedRoomPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Count)];
        RoomInfo newRoom = Instantiate(selectedRoomPrefab, Vector3.zero, Quaternion.identity);

        if (generatedRooms.Count > 0)
        {
            RoomInfo lastRoom = generatedRooms[generatedRooms.Count - 1];

            Quaternion newRotation = CalculateNewRotation(lastRoom, newRoom);

            newRoom.roomDimensions.rotation = newRotation;
            newRoom.transform.rotation = newRotation;

            Vector3 newPosition = CalculateNewPosition(lastRoom, newRoom);

            newRoom.transform.position = newPosition;
        }

        generatedRooms.Add(newRoom);
    }

    Vector3 CalculateNewPosition(RoomInfo lastRoom, RoomInfo newRoom)
    {
        Vector3 newPosition = Vector3.zero;


        return newPosition;
    }

    Vector3 FindDoorNormal()
    {
        return Vector3.zero; 
    }

    Quaternion CalculateNewRotation(RoomInfo lastRoom, RoomInfo newRoom)
    {
        Quaternion newRotation = Quaternion.identity;




        return newRotation;
    }
}
