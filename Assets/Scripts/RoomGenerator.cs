using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using Random = UnityEngine.Random;

public class RoomGenerator : MonoBehaviour
{
    [SerializeField] private float drawingDelay;
    [SerializeField] private int numberOfPyramids;
    public int numberOfRooms;
    public List<RoomInfo> roomPrefabs;
    //private List<RoomInfo> generatedRooms = new List<RoomInfo>();

    [SerializeField] private float forwardOffset; 
    [SerializeField] private float rightOffset;

    [SerializeField] private float randomXOffset;

    [SerializeField] private int numberOfBranches;
    [SerializeField] private int maxDepthOfBranch;

    [SerializeField] float ittiriations;

    [SerializeField] float randomError; 

    private List<List<RoomsLayer>> generation = new List<List<RoomsLayer>>();


    private void Start()
    {

    }
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.R)) ResetRooms();
    }
    private void InitializeBranches(List<Room> path, int numberOfBranches, int depthOfBranches)
    {
        int totalRooms = path.Count;
        if (numberOfBranches*2 >= totalRooms)
        {
            Debug.LogWarning("numberOfBranches is too high, please decrease it or increase number of rooms");
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
            lowerBound = upperBound-1;
        }
        for (int i = 0; i < branchingPoints.Count; i++)
        {
            Room point = branchingPoints[i];
            Debug.DrawLine(point.GetRoomPosition(), point.GetRoomPosition() + Vector3.right, Color.white, drawingDelay);
        }
        GenerateBranches(branchingPoints, path);
    }

    private void GenerateBranches(List<Room> branchingPoints, List<Room> correctPath)
    {
        List<Room> allBranches = new List<Room>(); 
        for (int i = branchingPoints.Count -1; i >= 0 ; i--)
        {
            int rand = maxDepthOfBranch;
            List<Room> branchedPath = BranchOff(branchingPoints[i], rand, correctPath, allBranches);
            if (branchedPath == null) Debug.LogError("branched path Was NULL");
            VisualisePath(branchedPath, Color.red);
            foreach (var branch in branchedPath) allBranches.Add(branch);
        }
    }

    private List<Room> BranchOff(Room from, int maxDepthOfBranch, List<Room> correctPath, List<Room> generatedBranches)
    {
        Debug.Log("Ittiration attempt");
        if (maxDepthOfBranch == 0)
        {
            Debug.LogWarning("Generation finished, depth = " + maxDepthOfBranch);
            return null;
        }
            if (correctPath == null) Debug.LogError("correctPath was NULL");
        if (from == null) Debug.LogError("from was NULL");
        if (from.layerNumber > correctPath.Count - 2){
            Debug.LogWarning("FROM LAYER WAS MORE THAN CORRECT PATH DOT COUNT");
            return null;
        }
        List<Room> branchedPath = new List<Room>();
        branchedPath.Add(from);

        Room tempA;
        Room tempB;
        int randInt = Random.Range(0, 2);
        if(randInt == 0)
        {
            tempA = from.roomA;
            tempB = from.roomB; 
        }
        else
        {
            tempA = from.roomB;
            tempB = from.roomA;
        }

        if (!correctPath.Contains(tempA) && tempA != null && !generatedBranches.Contains(tempA))
        {
            branchedPath.Add(tempA);
            List<Room> newRooms = BranchOff(tempA, maxDepthOfBranch - 1, correctPath, generatedBranches);
            if (newRooms != null)
            {
                foreach (var newRoom in newRooms)
                {
                    if (branchedPath.Contains(newRoom)) continue;
                    branchedPath.Add(newRoom);
                }
            }
        }
        else if(tempB != null)
        {
            branchedPath.Add(tempB);
            List<Room> newRooms = BranchOff(tempB, maxDepthOfBranch - 1, correctPath, generatedBranches);
            if (newRooms != null)
            {
                foreach (var newRoom in newRooms)
                {
                    if (branchedPath.Contains(newRoom)) continue;
                    branchedPath.Add(newRoom);
                }
            }
        }
        else Debug.Log("Room A and B was NULL " + tempA + " : " + tempB);
        return branchedPath;
    }

    private void VisualisePath(List<Room> path, Color color)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            Debug.DrawLine(path[i].GetRoomPosition(), path[i + 1].GetRoomPosition(), color, drawingDelay);
        }
    }
    private void ResetRooms()
    {
        generation.Clear();
        GeneratePyramids();
    }

    private void GeneratePyramids()
    {
        Vector3 origin = Vector3.zero;
        for (int i = 0; i < numberOfPyramids; i++)
        {
            List<RoomsLayer> roomLayer = new List<RoomsLayer>(); 
            Vector3 temp = GeneratePyramid(origin, roomLayer).GetRoomPosition();
            origin = temp;
        }
    }

    private List<Room> FindPath(Room from, Room to, List<RoomsLayer> roomLayers)
    {
        List<Room> path = new List<Room>();
        path.Add(from);
        for (int i = roomLayers.Count -2; i >= 0; i--)
        {
            Room closest = new Room(Vector3.zero,Vector3.zero, 0);
            float closestFloat = float.MaxValue;
            foreach (var room in roomLayers[i].roomPositions)
            {
                float rand = Random.Range(-randomError, randomError);
                float diff = Mathf.Abs(room.GetRoomPosition().x + rand - from.GetRoomPosition().x);
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
    void AddLayer(List<RoomsLayer> roomLayers)
    {
        Multiply(roomLayers[roomLayers.Count-1],roomLayers);
    }
    private void VisualiseRooms(List<RoomsLayer> roomLayers)
    {
        foreach (var layer in roomLayers)
        {
            foreach (var room in layer.roomPositions) Debug.DrawLine(room.GetRoomPosition(), room.GetRoomPosition() + Vector3.forward, Color.cyan, drawingDelay);
        }
    }

    Room GeneratePyramid(Vector3 origin, List<RoomsLayer> roomLayers)
    {
        Multiply(AddInitialRoom(origin,roomLayers),roomLayers);
        for (int i = 0; i < numberOfRooms; i++)
        {
            AddLayer(roomLayers);
        }
        VisualiseRooms(roomLayers);
        int rand = Random.Range(0, roomLayers[roomLayers.Count - 1].roomPositions.Count);
        Room from = roomLayers[roomLayers.Count - 1].roomPositions[rand];
        Room to = roomLayers[0].roomPositions[0];
        List<Room> correctPath = FindPath(from, to,roomLayers);
        VisualisePath(correctPath, Color.green);
        InitializeBranches(correctPath, numberOfBranches, maxDepthOfBranch);
        return from;
    }
    private RoomsLayer AddInitialRoom(Vector3 origin, List<RoomsLayer> roomLayers)
    {
        RoomsLayer initialLayer = new RoomsLayer(0);
        initialLayer.roomPositions = new List<Room>() { new Room(Vector3.zero, origin, 0)};
        roomLayers.Add(initialLayer);
        Debug.Log("SHuthirsfu" + initialLayer.roomPositions[0].origin);
        return initialLayer;
    }

    void Multiply(RoomsLayer layer, List<RoomsLayer> roomLayers)
    {
        RoomsLayer newLayer = new RoomsLayer(layer.layerIndex +1);
        for (int i = 0; i < layer.roomPositions.Count; i++)
        {
            Room room = layer.roomPositions[i];
            float rand1 = Random.Range(-randomXOffset, randomXOffset);
            float rand2 = Random.Range(-randomXOffset, randomXOffset);
            Vector3 variation1 = new Vector3(rand1, 0, 0);
            Vector3 variation2 = new Vector3(rand2, 0, 0);
            Room firstBranch = new Room(variation1, room.origin + new Vector3(rightOffset, 0, forwardOffset), newLayer.layerIndex);
            Room secondBranch = new Room(variation2, room.origin + new Vector3(-rightOffset, 0, forwardOffset), newLayer.layerIndex);
            if (i == 0)
            {
                newLayer.roomPositions.Add(firstBranch);
                room.roomA = firstBranch;
            }
            else room.roomA = layer.roomPositions[i - 1].roomB;
            newLayer.roomPositions.Add(secondBranch);  
            room.roomB = secondBranch;
        }
        roomLayers.Add(newLayer); 
    }
}
public class RoomsLayer {
    public int layerIndex;
    public List<Room> roomPositions = new List<Room>();
    public RoomsLayer(int LayerIndex)
    {
        layerIndex = LayerIndex;
    }
}
public class Room {
    public Vector3 variation { private set; get; }
    public Vector3 origin { private set; get; }
    public bool multiplePaths = false;

    public Room roomA;
    public Room roomB;

    public int layerNumber; 

    public Room(Vector3 Variation, Vector3 Origin, int LayerNumber)
    {
        variation = Variation;
        origin = Origin;
        layerNumber = LayerNumber;
    }
    public Vector3 GetRoomPosition() { return (origin + variation); }
}


