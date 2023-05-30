using UnityEngine;
using System.Collections.Generic;

public class RoomGenerator : MonoBehaviour
{
    public int numberOfRooms;
    public List<RoomInfo> roomPrefabs;
    private List<RoomInfo> generatedRooms = new List<RoomInfo>();
    public float minPosition = -50f;
    public float maxPosition = 50f;
    public float maxConnectionDistance = 20f; // Maximum distance for a room to be connected
    public float branchingProbability = 0.5f; // Probability of a room creating a new branch
    public float distanceStep = 20f; // Distance between rooms

    void Start()
    {
        Random.seed = 51; 
        // Generate a first room at the center of the area
        RoomInfo firstRoom = Instantiate(
            roomPrefabs[Random.Range(0, roomPrefabs.Count)],
            Vector3.zero, Quaternion.identity
        );
        generatedRooms.Add(firstRoom);

        for (int i = 0; i < numberOfRooms - 1; i++)
        {
            // Generate a random position for the new room
            Vector3 position = new Vector3(
                Random.Range(minPosition, maxPosition),
                0f,
                (i + 1) * distanceStep // Increasing z position to move forward
            );

            // Randomly select a room prefab
            RoomInfo roomPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Count)];

            // Instantiate the new room
            RoomInfo newRoom = Instantiate(roomPrefab, position, Quaternion.identity);

            // Add the new room to the list of generated rooms
            generatedRooms.Add(newRoom);
        }
        ConnectAllDoors();
    }

    RoomInfo GetNearestRoom(RoomInfo room)
    {
        RoomInfo nearestRoom = null;
        float nearestDistance = float.MaxValue;

        foreach (RoomInfo otherRoom in generatedRooms)
        {
            if (room != otherRoom)
            {
                float distance = Vector3.Distance(room.doorEntrance.position, otherRoom.doorExit.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestRoom = otherRoom;
                }
            }
        }

        return nearestRoom;
    }

    void ConnectAllDoors()
    {
        for (int i = 0; i < generatedRooms.Count - 1; i++)
        {
            // Connect to the next room or the room after next
            int skip = Random.value < 0.5f ? 1 : 2;
            if (i + skip < generatedRooms.Count)
            {
                DrawLineBetweenDoors(generatedRooms[i], generatedRooms[i + skip]);
            }

            // Sometimes add a connection to the previous room
            if (i > 0 && Random.value < 0.3f) // 30% chance to connect to the previous room
            {
                DrawLineBetweenDoors(generatedRooms[i], generatedRooms[i - 1]);
            }
        }
    }

    void DrawLineBetweenDoors(RoomInfo room1, RoomInfo room2)
    {
        Debug.DrawLine(room1.doorExit.position, room2.doorEntrance.position, Color.red, 100f);
    }
}
