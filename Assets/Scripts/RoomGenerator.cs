using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using Random = UnityEngine.Random;

public class RoomGenerator : MonoBehaviour
{
    public int numberOfRooms;
    public List<RoomInfo> roomPrefabs;
    //private List<RoomInfo> generatedRooms = new List<RoomInfo>();

    [SerializeField] private float forwardOffset; 
    [SerializeField] private float rightOffset;

    [SerializeField] private float randomXOffset;

    [SerializeField] private int numberOfBranches;
    [SerializeField] private int maxDepthOfBranch;

    [SerializeField] float ittiriations; 

    private List<RoomsLayer> roomLayers = new List<RoomsLayer>();


    private void Start()
    {
        GeneratePyramid(); 
    }
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.R)) ResetRooms();
        if (Input.GetKeyUp(KeyCode.KeypadEnter)) VisualiseRooms();
        if (Input.GetKeyUp(KeyCode.Space))
        {
            AddLayer();
            VisualiseRooms();
        }
        if (Input.GetKeyUp(KeyCode.Return))
        {
            for (int i = 0; i < ittiriations; i++)
            {
                int rand = Random.Range(0, roomLayers[roomLayers.Count - 1].roomPositions.Count);
                Room from = roomLayers[roomLayers.Count - 1].roomPositions[rand];
                Room to = roomLayers[0].roomPositions[0];
                List<Room> correctPath = FindPath(from, to);
                VisualiseCorrectPath(correctPath);
                VisualiseBranches(correctPath, numberOfBranches, maxDepthOfBranch);
            }
        }
    }
    private void VisualiseBranches(List<Room> path, int numberOfBranches, int depthOfBranches)
    {
        int totalRooms = path.Count;
        if (numberOfBranches > totalRooms || depthOfBranches > totalRooms)
        {
            Debug.LogWarning("Number of branches or Max Depth of branches is more than number of Rooms!");
            return;
        }
        List<Room> branchingPoints = new List<Room>();
        int lowerBound = totalRooms-2;
        for (int i = numberOfBranches-1; i >= 0; i--)
        {
            int upperBound = (int)((totalRooms-1) / numberOfBranches) * (i);
            int rand = Random.Range(lowerBound, upperBound);
            Room point = path[rand];
            branchingPoints.Add(point);
            Debug.Log(lowerBound + " to " + upperBound + " rnad : " + rand + " // total rooms = " + totalRooms);
            lowerBound = upperBound-1;
        }
        for (int i = 0; i < branchingPoints.Count; i++)
        {
            Room point = branchingPoints[i];
            Debug.DrawLine(point.GetRoomPosition(), point.GetRoomPosition() + Vector3.right, Color.white, 2f);
        }
    }
    private void VisualiseCorrectPath(List<Room> path)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            Debug.DrawLine(path[i].GetRoomPosition(), path[i + 1].GetRoomPosition(), Color.green, 2f);
        }
    }
    private void ResetRooms()
    {
        roomLayers.Clear(); 
    }
    private List<Room> FindPath(Room from, Room to)
    {
        List<Room> path = new List<Room>();
        path.Add(from);
        for (int i = roomLayers.Count -2; i >= 0; i--)
        {
            Room closest = new Room(Vector3.zero,Vector3.zero);
            float closestFloat = float.MaxValue;

/*            Vector3 closestTO = Vector3.zero;
            float closestFloatTO = float.MaxValue;*/
            foreach (var room in roomLayers[i].roomPositions)
            {
                float diff = Mathf.Abs(room.GetRoomPosition().x - from.GetRoomPosition().x);
                //float diffTO = Mathf.Abs()
                if (diff < closestFloat)
                {
                    closest = room;
                    closestFloat = diff;
                }
            }
            path.Add(closest); 
        }
        return path;
    }
    void AddLayer()
    {
        if (roomLayers.Count == 0) AddInitialRoom();
        Multiply(roomLayers[roomLayers.Count-1]);
    }
    private void VisualiseRooms()
    {
        foreach (var layer in roomLayers)
        {
            foreach (var room in layer.roomPositions) Debug.DrawLine(room.GetRoomPosition(), room.GetRoomPosition() + Vector3.forward, Color.cyan, 2);
        }
    }

    void GeneratePyramid()
    {
        Multiply(AddInitialRoom());
    }
    private RoomsLayer AddInitialRoom()
    {
        RoomsLayer initialLayer = new RoomsLayer();
        initialLayer.roomPositions = new List<Room>() { new Room(Vector3.zero, Vector3.zero)};
        roomLayers.Add(initialLayer);
        return initialLayer;
    }

    void Multiply(RoomsLayer layer)
    {
        RoomsLayer newLayer = new RoomsLayer();
        for (int i = 0; i < layer.roomPositions.Count; i++)
        {
            Room room = layer.roomPositions[i];
            float rand1 = Random.Range(-randomXOffset, randomXOffset);
            float rand2 = Random.Range(-randomXOffset, randomXOffset);
            Vector3 variation1 = new Vector3(rand1, 0, 0);
            Vector3 variation2 = new Vector3(rand2, 0, 0);
            Room firstBranch = new Room(variation1, room.origin + new Vector3(rightOffset, 0, forwardOffset));
            Room secondBranch = new Room(variation2, room.origin + new Vector3(-rightOffset, 0, forwardOffset));
            if(i == 0) newLayer.roomPositions.Add(firstBranch);
            newLayer.roomPositions.Add(secondBranch);
        }
        roomLayers.Add(newLayer); 
    }
}
public class RoomsLayer {
    public List<Room> roomPositions = new List<Room>();
    public RoomsLayer()
    {
    }
}
public class Room {
    public Vector3 variation;
    public Vector3 origin; 

    public Room(Vector3 Variation, Vector3 Origin)
    {
        variation = Variation;
        origin = Origin;
    }
    public Vector3 GetRoomPosition() { return (origin + variation); }
}


