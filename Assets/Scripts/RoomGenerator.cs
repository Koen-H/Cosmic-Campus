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

    List<Room> connectedRooms = new List<Room>();


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

        List<Room> currentLine = new List<Room>();  // keep track of current line
        bool isNewLine = true;  // boolean to determine if it's the start of a new line

        // Variable to store the current color of the red line
        Color currentColor = Color.red;

        for (int i = 0; i < nonPathRooms.Count - 1; i++)
        {
            Room currentRoom = nonPathRooms[i];
            Room nextRoom = nonPathRooms[i + 1];

            if (isNewLine)
            {
                // Reset the color
                currentColor = Color.red;

                // Find the closest path room that's behind the current room
                Room closestPathRoom = generatedCorrectPath
                    .Where(room => room.transform.position.z < currentRoom.transform.position.z)
                    .OrderBy(room => (room.transform.position - currentRoom.transform.position).sqrMagnitude)
                    .FirstOrDefault();

                // Make sure the connection doesn't result in a single upwards link
                if (closestPathRoom != null && closestPathRoom.transform.position.z < currentRoom.transform.position.z)
                {
                    Debug.DrawLine(currentRoom.transform.position, closestPathRoom.transform.position, Color.yellow, 10);
                }
                isNewLine = false;
            }

            // If the next room is at the same z location, or lineLength reached the max value, or the next room is behind the current room, start a new line from the next room
            if (Mathf.Approximately(currentRoom.transform.position.z, nextRoom.transform.position.z)
                || currentLine.Count >= maxDeadEndRooms
                || currentRoom.transform.position.z > nextRoom.transform.position.z)
            {
                currentLine.Clear();

                // If there's a closer non-path room ahead, connect to it. If not, connect to the closest path room below.
                Room closerRoom = nonPathRooms
                    .Where(room => room.transform.position.z > currentRoom.transform.position.z)
                    .OrderBy(room => (room.transform.position - currentRoom.transform.position).sqrMagnitude)
                    .FirstOrDefault()
                    ?? generatedCorrectPath
                        .Where(room => room.transform.position.z < currentRoom.transform.position.z)
                        .OrderBy(room => (room.transform.position - currentRoom.transform.position).sqrMagnitude)
                        .FirstOrDefault();

                // Make sure the connection doesn't result in a single upwards link
                if (closerRoom != null && closerRoom.transform.position.z < currentRoom.transform.position.z)
                {
                    Debug.DrawLine(currentRoom.transform.position, closerRoom.transform.position, currentColor, 10);
                }
                isNewLine = true;  // Start a new line
                continue;
            }

            // Add the room to the current line
            currentLine.Add(currentRoom);

            // Draw the red line, if it doesn't result in a single upwards link
            if (currentLine.Count > 1 && currentLine[currentLine.Count - 2].transform.position.z < currentRoom.transform.position.z)
            {
                Debug.DrawLine(currentLine[currentLine.Count - 2].transform.position, currentRoom.transform.position, currentColor, 10);
            }

            // Gradually lighten the color of the line
            currentColor += new Color(0.1f, 0.1f, 0.1f);
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
