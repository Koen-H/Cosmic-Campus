using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using Random = System.Random;
using UnityEngine.AI;
using Unity.Netcode;

public class RoomGenerator : NetworkBehaviour
{
    [SerializeField] private int seed;
    [SerializeField] private bool randomSeed;

    public int numberOfRooms;
    public List<RoomInfo> roomPrefabs;

    [SerializeField] RoomInfo firstRoom;
    [SerializeField] RoomInfo lastRoom;
    [HideInInspector] public RoomInfo lastBossRoom;

    [SerializeField] private float forwardOffset;
    [SerializeField] private float rightOffset;

    [SerializeField] private float randomXOffset;

    [SerializeField] private int numberOfBranches;
    [SerializeField] private int maxDepthOfBranch;

    [SerializeField] float randomError;

    [SerializeField] float splineSharpness;
    [SerializeField] int splineResolution;

    [SerializeField] private Curve curveMesh;

    [SerializeField] private int teacherStudentRatio;

    [SerializeField] private int maxEnemiesOnPath;

    [SerializeField] GameObject enemyPrefab;

    [SerializeField] private QuestTeacherNPC teacherPrefab;
    [SerializeField] private QuestStudentNPC studentPrefab;

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    private List<GameObject> spawnedNpcs = new List<GameObject>();
    private List<Room> correctPath = new List<Room>();

    private List<List<RoomsLayer>> generation = new List<List<RoomsLayer>>();

    Random systemRand = new Random();

    List<Animator> doorKeys = new List<Animator>();

    private List<RoomInfo> lateRoomEnemiesToSpawn = new List<RoomInfo>();
    private int latestEnemyLayer = -1;
    public int LatestEnemyLayer
    {
        get
        {
            return latestEnemyLayer;
        }
    }

    public Vector3 initialSpawnLocation;

    [SerializeField] private ListSO cmgtPrefabs;

    private static RoomGenerator instance;
    public static RoomGenerator Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("RoomGenerator Instance is null");
                return instance;
            }
            else
            {
                return instance;
            }
        }
    }
    private void Awake()
    {
        instance = this;
    }
    #region Map Initialization
    private void GeneratePath(List<Room> path, Color color, out List<NavMeshSurface> navMeshSurfaces, out List<EnemyNPC> allEnemies, bool reverse = false)
    {
        if (reverse) path.Reverse();
        navMeshSurfaces = new List<NavMeshSurface>();
        allEnemies = new List<EnemyNPC>();
        for (int i = 0; i < path.Count - 1; i++)
        {
            if (i == 0) initialSpawnLocation = path[i].entrance.position + path[i].GetRoomPosition() - path[i].entrance.normal;

            Room otherRoom;
            List<Vector3> splinePath;
            if (!reverse)
            {
                otherRoom = path[i].roomA == path[i + 1] ? path[i].roomB : path[i].roomA;

                Vector3 leftDoor;
                Vector3 rightDoor;
                if (otherRoom.GetRoomPosition().x < path[i + 1].GetRoomPosition().x)
                {
                    rightDoor = otherRoom.roomPrefab.doorRight.position + otherRoom.GetRoomPosition();
                    leftDoor = path[i + 1].roomPrefab.doorLeft.position + path[i + 1].GetRoomPosition();
                    splinePath = SplinePath(rightDoor, rightDoor + Vector3.right, leftDoor, leftDoor - Vector3.right);
                }
                else
                {
                    rightDoor = path[i + 1].roomPrefab.doorRight.position + path[i + 1].GetRoomPosition();
                    leftDoor = otherRoom.roomPrefab.doorLeft.position + otherRoom.GetRoomPosition();
                    splinePath = SplinePath(leftDoor, leftDoor - Vector3.right, rightDoor, rightDoor + Vector3.right);
                }

            }
            else splinePath = SplinePath(path[i].exit, path[i + 1].entrance);
            GenerateGeometry(path, navMeshSurfaces, allEnemies, i, splinePath, reverse);
        }
        RoomInfo room = Instantiate(path[path.Count - 1].roomPrefab, path[path.Count - 1].GetRoomPosition(), Quaternion.identity, this.transform);//generates last room
        room.gameObject.layer = 6;
        if (reverse) lastBossRoom = room;
        LoadCmgtPrefabs(room);
        ApplyNavMeshModifierToChildren(room.transform);
        room.roomLayer = path[path.Count - 1].layerNumber;
        lateRoomEnemiesToSpawn.Add(room);
    }
    private void GenerateBranches(List<Room> branchingPoints, List<Room> path, out List<NavMeshSurface> navMeshSurfaces, out List<EnemyNPC> allEnemies, out List<Room> outQuestNPC)
    {
        List<Room> allBranches = new List<Room>();
        navMeshSurfaces = new List<NavMeshSurface>();
        allEnemies = new List<EnemyNPC>();
        outQuestNPC = new List<Room>();
        branchingPoints.Reverse();

        int teacherRatio = teacherStudentRatio;
        bool drawTeacher = true;  
        OnMapNPC currentTeacher = null;  
        List<OnMapNPC> allTeachers = new List<OnMapNPC>();

        for (int i = branchingPoints.Count - 1; i >= 0; i--)
        {
            Room branchingPoint = branchingPoints[i];
            int rand = maxDepthOfBranch;
            List<Room> branchedPath = BranchOff(branchingPoint, rand, correctPath, allBranches);
            if (branchedPath == null) Debug.LogError("branched path Was NULL");
            List<Room> tempOut;
            OnMapNPC newNPC = InitializeNPCs(branchingPoint, branchedPath, drawTeacher, out tempOut);
            foreach (var item in tempOut) { outQuestNPC.Add(item); }
            if (drawTeacher)
            {
                currentTeacher = newNPC;
                allTeachers.Add(currentTeacher);
            }
            else
            {
                currentTeacher.requiredStudents++;
                newNPC.requiredStudents++;
                currentTeacher.dependency.Add(newNPC);
                newNPC.dependency.Add(currentTeacher);
            }
            if (--teacherRatio <= 0)
            {
                teacherRatio = teacherStudentRatio;
                drawTeacher = true;
            }
            else
            {
                drawTeacher = false;
            }

            GeneratePath(branchedPath, Color.red, out navMeshSurfaces, out allEnemies);
            foreach (var branch in branchedPath) allBranches.Add(branch);
        }
    }
    private void ResetRooms()
    {
        systemRand = new Random(seed);
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            Destroy(spawnedEnemies[i]);
        }
        spawnedEnemies.Clear();
        generation.Clear();
        correctPath.Clear();

        StartCoroutine(GenerateNextFrame());
    }
    IEnumerator GenerateNextFrame()
    {
        yield return new WaitForFixedUpdate();
        GeneratePyramids();
    }
    private void GeneratePyramids()
    {
        Vector3 origin = Vector3.zero;
        List<RoomsLayer> roomLayer = new List<RoomsLayer>();
        Vector3 temp = GeneratePyramid(origin, roomLayer).GetRoomPosition();
        origin = temp;
    }
    Room GeneratePyramid(Vector3 origin, List<RoomsLayer> roomLayers)
    {
        List<EnemyNPC> mainPathEnemies = new List<EnemyNPC>();
        List<EnemyNPC> branchedPathEnemies = new List<EnemyNPC>();
        List<Room> outQuestNPC = new List<Room>();
        Multiply(AddInitialRoom(origin, roomLayers), roomLayers);
        for (int i = 0; i < numberOfRooms; i++)
        {
            AddLayer(roomLayers);
        }
        int rand = systemRand.Next(0, roomLayers[roomLayers.Count - 1].roomPositions.Count);
        Room from = roomLayers[roomLayers.Count - 1].roomPositions[rand];
        if (lastRoom != null)
        {
            from.roomPrefab = lastRoom;
            from.entrance = new Door(from.GetRoomPosition() + from.roomPrefab.GetEntrancePosition(), from.roomPrefab.normalEntrance);
            from.exit = new Door(from.GetRoomPosition() + from.roomPrefab.GetExitPosition(), from.roomPrefab.normalExit);
        }

        Room to = roomLayers[0].roomPositions[0];
        correctPath = FindPath(from, to, roomLayers);
        List<NavMeshSurface> navMeshSurfaces;
        GeneratePath(correctPath, Color.green, out navMeshSurfaces, out mainPathEnemies, true);
        List<NavMeshSurface> newNavMeshSurfaces;
        InitializeBranches(correctPath, numberOfBranches, maxDepthOfBranch, out newNavMeshSurfaces, out branchedPathEnemies, out outQuestNPC);
        foreach (var surface in newNavMeshSurfaces) navMeshSurfaces.Add(surface);
        BakeNavMesh(navMeshSurfaces);
        if (IsServer)
        {
            SpawnEnemies(mainPathEnemies);
            SpawnEnemies(branchedPathEnemies);

        }
        foreach (var questNPC in outQuestNPC) SpawnRoomNPC(questNPC);
        return from;
    }
    void Multiply(RoomsLayer layer, List<RoomsLayer> roomLayers)
    {
        RoomsLayer newLayer = new RoomsLayer(layer.layerIndex + 1);
        for (int i = 0; i < layer.roomPositions.Count; i++)
        {
            Room room = layer.roomPositions[i];
            double randDouble1 = systemRand.NextDouble();
            float rand1 = (float)(randDouble1 * 2 * randomXOffset - randomXOffset);

            double randDouble2 = systemRand.NextDouble();
            float rand2 = (float)(randDouble2 * 2 * randomXOffset - randomXOffset);
            Vector3 variation1 = new Vector3(rand1, 0, 0);
            Vector3 variation2 = new Vector3(rand2, 0, 0);
            int randInt = systemRand.Next(0, roomPrefabs.Count);
            RoomInfo roomInfo = roomPrefabs[randInt];
            Room firstBranch = new Room(variation1, room.origin + new Vector3(rightOffset, 0, forwardOffset), newLayer.layerIndex, null, null, roomInfo);
            Room secondBranch = new Room(variation2, room.origin + new Vector3(-rightOffset, 0, forwardOffset), newLayer.layerIndex, null, null, roomInfo);
            firstBranch.entrance = new Door(firstBranch.GetRoomPosition() + roomInfo.GetEntrancePosition(), roomInfo.normalEntrance);
            firstBranch.exit = new Door(firstBranch.GetRoomPosition() + roomInfo.GetExitPosition(), roomInfo.normalExit);
            secondBranch.entrance = new Door(secondBranch.GetRoomPosition() + roomInfo.GetEntrancePosition(), roomInfo.normalEntrance);
            secondBranch.exit = new Door(secondBranch.GetRoomPosition() + roomInfo.GetExitPosition(), roomInfo.normalExit);
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
    private RoomsLayer AddInitialRoom(Vector3 origin, List<RoomsLayer> roomLayers)
    {
        RoomsLayer initialLayer = new RoomsLayer(0);
        int randInt = systemRand.Next(0, roomPrefabs.Count);
        RoomInfo roomInfo = roomPrefabs[randInt];
        if (firstRoom != null) roomInfo = firstRoom;
        initialLayer.roomPositions = new List<Room>() { new Room(Vector3.zero, origin, 0, new Door(roomInfo.GetEntrancePosition() + origin, roomInfo.normalEntrance), new Door(roomInfo.GetExitPosition() + origin, roomInfo.normalExit), roomInfo) };
        roomLayers.Add(initialLayer);
        return initialLayer;
    }
    void AddLayer(List<RoomsLayer> roomLayers)
    {
        Multiply(roomLayers[roomLayers.Count - 1], roomLayers);
    }
    private List<Room> FindPath(Room from, Room to, List<RoomsLayer> roomLayers)
    {
        List<Room> path = new List<Room>();
        Room temp = from;
        path.Add(temp);

        for (int i = roomLayers.Count - 2; i >= 1; i--)
        {
            RoomsLayer roomsLayer = roomLayers[i];
            Room roomA = null;
            Room roomB = null;

            foreach (var room in roomsLayer.roomPositions)
            {
                if (temp == room.roomA) roomA = room;
                if (temp == room.roomB) roomB = room;
            }
            int randInt = systemRand.Next(0, 2);

            if (roomA == null) roomA = roomB;
            if (roomB == null) roomB = roomA;

            if (randInt == 0) temp = roomA;
            else temp = roomB;

            path.Add(temp);
        }
        path.Add(roomLayers[0].roomPositions[0]);
        return path;
    }
    private void InitializeBranches(List<Room> path, int numberOfBranches, int depthOfBranches, out List<NavMeshSurface> navMeshSurfaces, out List<EnemyNPC> allEnemies, out List<Room> outQuestNPC)
    {
        navMeshSurfaces = new List<NavMeshSurface>();
        allEnemies = new List<EnemyNPC>();
        outQuestNPC = new List<Room>();
        int totalRooms = path.Count;
        if (numberOfBranches * 2 >= totalRooms)
        {
            Debug.LogWarning("numberOfBranches is too high, please decrease it or increase number of rooms");
            return;
        }
        List<Room> branchingPoints = new List<Room>();
        int lowerBound = totalRooms - 2;
        for (int i = numberOfBranches - 1; i >= 0; i--)
        {
            int upperBound = (int)((totalRooms - 1) / numberOfBranches) * (i);
            int randNumber;
            if (lowerBound > upperBound) randNumber = systemRand.Next(upperBound, lowerBound);
            else randNumber = systemRand.Next(lowerBound, upperBound);
            Room point = path[randNumber];
            branchingPoints.Add(point);
            lowerBound = upperBound - 1;
        }
        for (int i = 0; i < branchingPoints.Count; i++)
        {
            Room point = branchingPoints[i];
        }
        GenerateBranches(branchingPoints, path, out navMeshSurfaces, out allEnemies, out outQuestNPC);
    }
    private List<Room> BranchOff(Room from, int maxDepthOfBranch, List<Room> path, List<Room> generatedBranches)
    {
        if (maxDepthOfBranch == 0)
        {
            return null;
        }
        if (correctPath == null) Debug.LogError("correctPath was NULL");
        if (from == null) Debug.LogError("from was NULL");
        if (from.layerNumber > correctPath.Count - 2)
        {
            Debug.LogWarning("FROM LAYER WAS MORE THAN CORRECT PATH DOT COUNT");
            return null;
        }
        List<Room> branchedPath = new List<Room>();
        branchedPath.Add(from);

        Room tempA;
        Room tempB;
        int randInt = systemRand.Next(0, 2);
        if (randInt == 0)
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
        else if (tempB != null)
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
    private OnMapNPC InitializeNPCs(Room branchingPoint, List<Room> branchedPath, bool drawTeacher, out List<Room> outQuestNPC)
    {
        Room studentRoom = branchedPath[branchedPath.Count - 1];
        Room teacherRoom;
        outQuestNPC = new List<Room>();

        if (!branchedPath.Contains(branchingPoint.roomA)) teacherRoom = branchingPoint.roomA;
        else teacherRoom = branchingPoint.roomB;

        OnMapNPC student = new StudentNPC(studentRoom.GetRoomPosition());
        studentRoom.roomNpc = student;
        if (studentRoom.roomNpc != null)
        {
            outQuestNPC.Add(studentRoom);
        }

        if (drawTeacher)
        {
            OnMapNPC teacher = new TeacherNPC(teacherRoom.GetRoomPosition());
            teacherRoom.roomNpc = teacher;
            teacher.requiredStudents++;
            if (teacherRoom.roomNpc != null)
            {
                outQuestNPC.Add(teacherRoom);
            }
            return teacher;
        }

        return student;
    }
    #endregion

    #region Generating Geometry
    private void GenerateGeometry(List<Room> path, List<NavMeshSurface> navMeshSurfaces, List<EnemyNPC> allEnemies, int i, List<Vector3> splinePath, bool branchedPath = true)
    {
        splinePath.Add(splinePath[splinePath.Count - 1] + (splinePath[splinePath.Count - 1] - splinePath[splinePath.Count - 2]));
        Curve newCurve = Instantiate(curveMesh, this.transform);
        newCurve.points = splinePath;
        newCurve.Apply();
        newCurve.gameObject.layer = 6;
        newCurve.gameObject.tag = "RainbowRoad";
        newCurve.gameObject.AddComponent<MeshCollider>();
        navMeshSurfaces.Add(newCurve.gameObject.AddComponent<NavMeshSurface>());
        if (!branchedPath && i == 0) return;
        int randomCount = systemRand.Next(0, maxEnemiesOnPath + 1);
        List<EnemyNPC> newEnemies = InitiateEnemyOnPath(splinePath, randomCount);
        foreach (var enemy in newEnemies) allEnemies.Add(enemy);
        RoomInfo room = Instantiate(path[i].roomPrefab, path[i].GetRoomPosition(), Quaternion.identity, this.transform);
        ApplyNavMeshModifierToChildren(room.transform);
        room.roomLayer = path[i].layerNumber;
        lateRoomEnemiesToSpawn.Add(room);
    }
    List<Vector3> SplinePath(Door from, Door to)
    {
        Vector3 fromDirection = (from.normal + Vector3.forward).normalized * splineSharpness;
        Vector3 toDirection = (to.normal - Vector3.forward).normalized * splineSharpness;


        Vector3 p1 = from.position; 
        Vector3 d1 = from.position + fromDirection; 
        Vector3 p2 = to.position; 
        Vector3 d2 = to.position + toDirection; 

        return SplinePath(p1, d1, p2, d2);
    }
    private List<Vector3> SplinePath(Vector3 p1, Vector3 d1, Vector3 p2, Vector3 d2)
    {
        List<Vector3> pathPoints = new List<Vector3>();
        int resolution = splineResolution;
        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;
            Vector3 point = uuu * p1 + 3 * uu * t * d1 + 3 * u * tt * d2 + ttt * p2;
            pathPoints.Add(point);

            if (i < resolution)
            {
                float t2 = (i + 1) / (float)resolution;
                float u2 = 1 - t2;
                float tt2 = t2 * t2;
                float uu2 = u2 * u2;
                float uuu2 = uu2 * u2;
                float ttt2 = tt2 * t2;
                Vector3 point2 = uuu2 * p1 + 3 * uu2 * t2 * d1 + 3 * u2 * tt2 * d2 + ttt2 * p2;
            }

        }

        return pathPoints;
    }
    private void SpawnEnemies(List<EnemyNPC> allEnemies)
    {
        foreach (var enemy in allEnemies)
        {
            SpawnEnemy(enemy.position, 0.7f);
        }
    }
    private List<EnemyNPC> InitiateEnemyOnPath(List<Vector3> path, int count)
    {
        List<EnemyNPC> enemies = new List<EnemyNPC>();
        List<int> rands = new List<int>();
        for (int i = 0; i < count; i++)
        {
            int randInt = systemRand.Next((int)(path.Count * 0.2f), (int)((path.Count - 1) * 0.8f));
            if (rands.Contains(randInt)) continue;
            enemies.Add(new EnemyNPC(path[randInt]));
            rands.Add(randInt);
        }
        return enemies;
    }
    public void SpawnEnemy(Vector3 position, float hightOffset)
    {
        NetworkObject enemy = Instantiate(enemyPrefab, position, Quaternion.LookRotation(transform.forward)).GetComponent<NetworkObject>();
        enemy.Spawn(true);
        spawnedEnemies.Add(enemy.gameObject);
    }
    void SpawnRoomNPC(Room room, float hightOffset = 2)
    {
        if (room.roomNpc is StudentNPC)
        {
            if (IsServer)
            {
                QuestStudentNPC student = Instantiate(studentPrefab, room.GetRoomPosition() + room.roomPrefab.GetStudentPosition(), Quaternion.identity, this.transform);
                student.self = room.roomNpc;
                student.GetComponent<NetworkObject>().Spawn(true);
            }
        }
        if (room.roomNpc is TeacherNPC)
        {
            GameObject newDoor = Instantiate(teacherPrefab.door, this.transform.parent);
            newDoor.transform.position = room.exit.position;
            newDoor.transform.rotation = Quaternion.LookRotation(-room.exit.normal, Vector3.up);

            int doorId = doorKeys.Count;

            doorKeys.Add(newDoor.GetComponent<Animator>());

            if (IsServer)
            {
                QuestNPC teacher = Instantiate(teacherPrefab, room.GetRoomPosition() + room.roomPrefab.GetTeacherPosition(), Quaternion.identity, this.transform);
                teacher.self = room.roomNpc;
                teacher.doorNormal = room.exit.normal;
                teacher.doorPosition = room.exit.position;
                teacher.GetComponent<NetworkObject>().Spawn(true);
                teacher.doorId = doorId;

            }

        }
    }
    void LoadCmgtPrefabs(RoomInfo room)
    {
        int max = cmgtPrefabs.GetCount();
        foreach (Transform spawnSpot in room.cmgtTransformSpawnpoints)
        {
            int rand = systemRand.Next(max);
            GameObject instance = Instantiate(cmgtPrefabs.GetGameObject(rand), spawnSpot.position, spawnSpot.rotation);
            instance.transform.localScale = spawnSpot.lossyScale;
        }
    }
    #endregion

    public Room GetCorrectPathRoom(int layerIndex)
    {
        if (layerIndex > numberOfRooms + 1) return null;
        foreach (Room room in correctPath)
        {
            if (room.layerNumber == layerIndex)
            {
                return room;
            }
        }
        return null;
    }
    public int GetSeed()
    {
        return seed;
    }
    public void SetSeed()
    {
        seed = randomSeed ? new Random().Next() : seed;
    }


    void BakeNavMesh(List<NavMeshSurface> surfaces)
    {
        for (int i = 0; i < surfaces.Count; i++)
        {
            surfaces[i].BuildNavMesh();
            if (surfaces[i].gameObject.layer == 6)
            {
            }
        }
    }
    void ApplyNavMeshModifierToChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag("Interactable")) continue;
            child.gameObject.layer = 6;
            if (child.childCount > 0)
            {
                ApplyNavMeshModifierToChildren(child);
            }
        }
    }


    public void OpenDoor(int code)
    {
        doorKeys[code].SetTrigger("Animate");
    }


    [ClientRpc]
    public void GenerateMapClientRpc(int serverSeed)
    {
        seed = serverSeed;
        ResetRooms();
    }
    [ServerRpc(RequireOwnership = false)]
    public void SpawnEnemiesInRoomServerRpc(int layer)
    {
        if (layer > latestEnemyLayer)
        {
            if (layer > numberOfRooms + 1) return;
            latestEnemyLayer = layer;
            if (IsServer)
            {
                foreach (RoomInfo room in lateRoomEnemiesToSpawn)
                {
                    if (room.roomLayer == latestEnemyLayer)
                    {
                        foreach (var spawner in room.enemySpawners) spawner.SpawnEnemy();
                        foreach (var networkObject in room.networkObjectSpawners) networkObject.SpawnObject();//TODO:: Replace list with potion.cs instead of networkobject
                    }
                }
            }
        }
    }
}
public class RoomsLayer
{
    public int layerIndex;
    public List<Room> roomPositions = new List<Room>();
    public RoomsLayer(int LayerIndex)
    {
        layerIndex = LayerIndex;
    }
}
public class Room
{
    public Vector3 variation { private set; get; }
    public Vector3 origin { private set; get; }
    public bool multiplePaths = false;
    public OnMapNPC roomNpc = null;

    public Room roomA;
    public Room roomB;

    public Door entrance;
    public Door exit;

    public RoomInfo roomPrefab;

    public int layerNumber;

    public Room(Vector3 Variation, Vector3 Origin, int LayerNumber, Door Entrance, Door Exit, RoomInfo RoomPrefab)
    {
        variation = Variation;
        origin = Origin;
        layerNumber = LayerNumber;
        entrance = Entrance;
        exit = Exit;
        roomPrefab = RoomPrefab;
    }
    public Vector3 GetRoomPosition() { return (origin + variation); }
}

public class Door
{
    public Vector3 position;
    public Vector3 normal;
    public Door(Vector3 Position, Vector3 Normal)
    {
        position = Position;
        normal = Normal;
    }
}

public class OnMapNPC
{
    public Vector3 position;
    public List<OnMapNPC> dependency = new List<OnMapNPC>();
    public int requiredStudents;

    public OnMapNPC(Vector3 position)
    {
        this.position = position;
    }
}
public class TeacherNPC : OnMapNPC
{
    public TeacherNPC(Vector3 position) : base(position) { }
}
public class StudentNPC : OnMapNPC
{
    public StudentNPC(Vector3 position) : base(position) { }
}

public class EnemyNPC : OnMapNPC
{
    public EnemyNPC(Vector3 position) : base(position) { }
}