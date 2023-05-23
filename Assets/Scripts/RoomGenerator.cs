using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class RoomGenerator : MonoBehaviour
{
    public float xOffsetVariation;
    public float falseRoomProbability;
    public int numberOfRooms;
    public int maxDeadEndRooms;
    [SerializeField] float extraOffset;

    public List<Room> roomPrefabs = new List<Room>();
    public List<Room> rooms = new List<Room>();
    public List<Room> generatedCorrectPath = new List<Room>();
    public List<Room> correctPath = new List<Room>();


    private float averageRoomSize;

    void Start()
    {
        CalculateAverageRoomSize();

        if (correctPath.Count == 0)
        {
            Room initialRoom = InstantiateRandomRoom();
            rooms.Add(initialRoom);
            correctPath.Add(initialRoom);
        }
        StartCoroutine(GenerateRoomsCoroutine());
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.KeypadEnter))
        {
            CheckOverlap();
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            RemoveAllRooms();
            StopAllCoroutines();
            StartCoroutine(GenerateRoomsCoroutine());
        }
    }

    void CalculateAverageRoomSize()
    {
        float totalSize = 0;
        foreach (var roomPrefab in roomPrefabs)
        {
            totalSize += roomPrefab.roomDepth;
        }
        averageRoomSize = totalSize / roomPrefabs.Count;
    }
    Room InstantiateRandomRoom()
    {
        int randomRoom = Random.Range(0, roomPrefabs.Count);
        return Instantiate(roomPrefabs[randomRoom]);
    }

    IEnumerator GenerateRoomsCoroutine()
    {
        while (generatedCorrectPath.Count <= numberOfRooms)
        {
            GenerateRoom(correctPath[0]);
            yield return new WaitForSeconds(0.1f); // Adjust this delay as needed
            if (generatedCorrectPath.Count > numberOfRooms)
            {
                CheckOverlap();
                ColorizeCorrectPath();
                DrawLinesBetweenRooms();
                DrawRedLinesBetweenNonPathRooms();
                yield break;
            }
        }
    }
    void DrawLinesBetweenRooms()
    {
        for (int i = 0; i < generatedCorrectPath.Count - 1; i++)
        {
            Debug.DrawLine(generatedCorrectPath[i].transform.position, generatedCorrectPath[i + 1].transform.position, Color.green, 10f);
        }
    }
    void DrawRedLinesBetweenNonPathRooms()
    {
        // Store the rooms that are not in the generatedCorrectPath list
        List<Room> nonPathRooms = new List<Room>(rooms.Except(generatedCorrectPath));

        // Sort the nonPathRooms list based on z position
        nonPathRooms = nonPathRooms.OrderBy(room => room.transform.position.z).ToList();

        int lineLength = 0;  // keep track of current line length

        for (int i = 0; i < nonPathRooms.Count - 1; i++)
        {
            Room currentRoom = nonPathRooms[i];
            Room nextRoom = nonPathRooms[i + 1];

            // If the next room is at the same z location, or lineLength reached the max value, reset lineLength and skip drawing a line
            if (Mathf.Approximately(currentRoom.transform.position.z, nextRoom.transform.position.z) || lineLength >= maxDeadEndRooms)
            {
                lineLength = 0;
                continue;
            }

            Debug.DrawLine(currentRoom.transform.position, nextRoom.transform.position, Color.red, 10);
            lineLength++;
        }
    }


    bool CheckOverlap()
    {
        foreach (var room in rooms)
        {
            if (IsOverlapping(room))
            {
                if (!generatedCorrectPath.Contains(room))
                {
                    rooms.Remove(room);
                    Destroy(room.gameObject);
                    return true;
                }
            }
        }
        return false;
    }
    void RemoveAllRooms()
    {
        for (int i = rooms.Count - 1; i >= 0; i--)
        {
            Destroy(rooms[i].gameObject);
        }
        rooms.Clear();
        generatedCorrectPath.Clear();
        correctPath.Clear();

        int randomRoom = Random.Range(0, roomPrefabs.Count);
        Room initialRoom = Instantiate(roomPrefabs[randomRoom]);
        rooms.Add(initialRoom);
        correctPath.Add(initialRoom);
    }

    void GenerateRooms()
    {
        if(correctPath.Count == 0)
        {
            int randomRoom = Random.Range(0, roomPrefabs.Count);
            Room initialRoom = Instantiate(roomPrefabs[randomRoom]);
            rooms.Add(initialRoom);
            correctPath.Add(initialRoom);
        }
        GenerateRoom(correctPath[0]);
        GenerateRooms();
    }

    void GenerateRoom(Room from, bool rightPath = true)
    {
        if (CheckOverlap()) return;
        Room newRoom = InstantiateRandomRoom();
        float xRand = Random.Range(-xOffsetVariation, xOffsetVariation);
        newRoom.transform.position = new Vector3(from.transform.position.x + xRand * from.roomDepth, from.transform.position.y, from.transform.position.z + from.roomDepth * extraOffset);




        if (rightPath) {
            correctPath.Add(newRoom);
            correctPath.Remove(from);
            generatedCorrectPath.Add(newRoom);
        }

        rooms.Add(newRoom);
        float rand = Random.Range(0.0f, 1.0f);
        if(rand < falseRoomProbability)
        {
            GenerateRoom(newRoom, false); 
        }
    }

    void ColorizeCorrectPath()
    {
        foreach (var room in generatedCorrectPath)
        {
            room.SetColor(Color.green);

        }
    }

    bool IsOverlapping(Room newRoom)
    {
        // Calculate half extents (since Unity wants the "half size" for the box, not full size)
        Vector3 halfExtents = Vector3.Scale(newRoom.boxCollider.size, newRoom.transform.lossyScale) / 2f;



        // Get all colliders that overlap with the new room.
        Collider[] overlappingColliders = Physics.OverlapBox(newRoom.transform.position, halfExtents, newRoom.transform.rotation);

        // Check if any of the overlapping colliders belong to the rooms.
        foreach (Collider collider in overlappingColliders)
        {
            Room overlappedRoom = collider.GetComponent<Room>();
            // If the collider belongs to an existing room (and it's not the collider of the new room itself), return true.
            if (collider != newRoom.roomCollider && overlappedRoom != null && rooms.Contains(overlappedRoom))
            {
                return true; // Found an overlap with another room.
            }
        }

        return false; // No overlaps found.
    }
}
