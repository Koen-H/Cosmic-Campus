using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using Random = System.Random;
using UnityEngine.AI;
using Unity.Netcode;

public class RoomGenerator : MonoBehaviour
{
    [SerializeField] private int seed;
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

    private List<List<RoomsLayer>> generation = new List<List<RoomsLayer>>();

    Random systemRand = new Random();


    private void Start()
    {

    }

    List<Vector3> SplinePath(Door from, Door to)
    {
        //Debug.Log("From : " + from.normal);
        //Debug.Log("To : " + to.normal);
        int resolution = splineResolution; // Higher numbers make the curve smoother
        Vector3 fromDirection = (from.normal+Vector3.forward).normalized * splineSharpness;
        Vector3 toDirection = (to.normal-Vector3.forward).normalized * splineSharpness;
        List<Vector3> pathPoints = new List<Vector3>();

        Vector3 p0 = from.position; // Starting point
        Vector3 p1 = from.position + fromDirection; // First control point
        Vector3 p2 = to.position + toDirection; // Second control point
        Vector3 p3 = to.position; // Ending point

        // Generate the curve
        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;
            Vector3 point = uuu * p0 + 3 * uu * t * p1 + 3 * u * tt * p2 + ttt * p3;
            pathPoints.Add(point);

            // Draw the curve
            if (i < resolution)
            {
                float t2 = (i + 1) / (float)resolution;
                float u2 = 1 - t2;
                float tt2 = t2 * t2;
                float uu2 = u2 * u2;
                float uuu2 = uu2 * u2;
                float ttt2 = tt2 * t2;
                Vector3 point2 = uuu2 * p0 + 3 * uu2 * t2 * p1 + 3 * u2 * tt2 * p2 + ttt2 * p3;
                //Debug.DrawLine(point, point2, Color.blue, drawingDelay);
            }

        }

        return pathPoints;
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) ResetRooms();
       // if (Input.GetKeyDown(KeyCode.S)) SplineDemo();
    }
    void BakeNavMesh(List<NavMeshSurface> surfaces)
    {
        for (int i = 0; i < surfaces.Count; i++)
        {
            if (surfaces[i].gameObject.layer != 6) continue;
            surfaces[i].BuildNavMesh();
        }
        // set to ture 
    }
    void SplineDemo()
    {
        Door from = new Door(Vector3.zero, Vector3.forward);
        Door to = new Door((Vector3.right + Vector3.forward) * 5, Vector3.right);
        List<Vector3> splinePath = SplinePath(from, to);
        Curve newCurve = Instantiate(curveMesh, this.transform);
        newCurve.points = splinePath;
        newCurve.gameObject.layer = 6;
        newCurve.Apply();
    }
    private void InitializeBranches(List<Room> path, int numberOfBranches, int depthOfBranches, out List<NavMeshSurface> navMeshSurfaces, out List<EnemyNPC> allEnemies, out List<Room> outQuestNPC)
    {
        navMeshSurfaces = new List<NavMeshSurface>();
        allEnemies = new List<EnemyNPC>();
        outQuestNPC = new List<Room>();
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
            int randNumber; 
            if (lowerBound > upperBound) randNumber = systemRand.Next(upperBound, lowerBound);
            else randNumber = systemRand.Next(lowerBound, upperBound);
            Room point = path[randNumber];
            branchingPoints.Add(point);
            lowerBound = upperBound-1;
        }
        for (int i = 0; i < branchingPoints.Count; i++)
        {
            Room point = branchingPoints[i];
            Debug.DrawLine(point.GetRoomPosition(), point.GetRoomPosition() + Vector3.right, Color.white, drawingDelay);
        }
        GenerateBranches(branchingPoints, path, out navMeshSurfaces, out allEnemies, out outQuestNPC);
    }



    private void GenerateBranches(List<Room> branchingPoints, List<Room> correctPath, out List<NavMeshSurface> navMeshSurfaces, out List<EnemyNPC> allEnemies, out List<Room> outQuestNPC)
    {
        List<Room> allBranches = new List<Room>();
        navMeshSurfaces = new List<NavMeshSurface>();
        allEnemies = new List<EnemyNPC>();
        outQuestNPC = new List<Room>();
        branchingPoints.Reverse();

        int teacherRatio = teacherStudentRatio;
        bool drawTeacher = true;  // Start by drawing a teacher
        OnMapNPC currentTeacher = null;  // This will hold the current teacher for which we are collecting students
        List<OnMapNPC> allTeachers = new List<OnMapNPC>();

        for (int i = branchingPoints.Count - 1; i >= 0; i--)
        {
            Room branchingPoint = branchingPoints[i];
            int rand = maxDepthOfBranch;
            List<Room> branchedPath = BranchOff(branchingPoint, rand, correctPath, allBranches);
            if (branchedPath == null) Debug.LogError("branched path Was NULL");
            List<Room> tempOut; 
            OnMapNPC newNPC = InitializeNPCs(branchingPoint, branchedPath, drawTeacher, out tempOut); // Draw a student every time
            foreach (var item in tempOut) { outQuestNPC.Add(item); }
            if (drawTeacher)
            {
                currentTeacher = newNPC;
                allTeachers.Add(currentTeacher);
            }
            else
            {
                currentTeacher.dependency.Add(newNPC);
                newNPC.dependency.Add(currentTeacher);
            }

            // Decrease teacher ratio or reset it and prepare to draw a new teacher
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

        foreach (var teacher in allTeachers)
        {
            foreach (var student in teacher.dependency)
            {
                Debug.DrawLine(student.position, teacher.position, new Color(255, 137, 0), drawingDelay * 1.5f);
            }
        }
    }
    private OnMapNPC InitializeNPCs(Room branchingPoint, List<Room> branchedPath, bool drawTeacher, out List<Room> outQuestNPC)
    {
        Room studentRoom = branchedPath[branchedPath.Count - 1];
        Room teacherRoom;
        outQuestNPC = new List<Room>();

        if (!branchedPath.Contains(branchingPoint.roomA)) teacherRoom = branchingPoint.roomA;
        else teacherRoom = branchingPoint.roomB;

        // Draw a student every time
        OnMapNPC student = new StudentNPC(studentRoom.GetRoomPosition());
        studentRoom.roomNpc = student;
        if (studentRoom.roomNpc != null)
        {
            VisualiseRoomNPC(studentRoom);
            outQuestNPC.Add(studentRoom);
           // if (!Input.GetKey(KeyCode.Space)) questStudent = SpawnRoomNPC(studentRoom);
        }

        if (drawTeacher)
        {
            // Draw a teacher and add the student to its dependencies
            OnMapNPC teacher = new TeacherNPC(teacherRoom.GetRoomPosition());
            teacherRoom.roomNpc = teacher;
            student.dependency.Add(teacher);
            teacher.dependency.Add(student);
            if (teacherRoom.roomNpc != null)
            {
                VisualiseRoomNPC(teacherRoom);
                outQuestNPC.Add(teacherRoom);
                // if (!Input.GetKey(KeyCode.Space)) questTeacher = SpawnRoomNPC(teacherRoom);
            }
            return teacher;
        }

        return student;
    }

    private List<Room> BranchOff(Room from, int maxDepthOfBranch, List<Room> correctPath, List<Room> generatedBranches)
    {
        //Debug.Log("Ittiration attempt");
        if (maxDepthOfBranch == 0)
        {
            //Debug.LogWarning("Generation finished, depth = " + maxDepthOfBranch);
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
        int randInt = systemRand.Next(0, 2);
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
    private void VisualisePath(List<Vector3> path, Color color)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            Debug.DrawLine(path[i], path[i + 1], color, drawingDelay);
        }
    }
    private void GeneratePath(List<Room> path, Color color, out List<NavMeshSurface> navMeshSurfaces, out List<EnemyNPC> allEnemies, bool reverse = false)
    {
        if (reverse) path.Reverse();
        navMeshSurfaces = new List<NavMeshSurface>();
        allEnemies = new List<EnemyNPC>();
        for (int i = 0; i < path.Count - 1; i++)
        {
            Debug.DrawLine(path[i].GetRoomPosition(), path[i + 1].GetRoomPosition(), color, drawingDelay);
            List<Vector3> splinePath = SplinePath(path[i].exit, path[i + 1].entrance);
            VisualisePath(splinePath, Color.blue);
            if(!Input.GetKey(KeyCode.Space))GenerateGeometry(path, navMeshSurfaces, allEnemies, i, splinePath);
        }
        if (!Input.GetKey(KeyCode.Space)) Instantiate(path[path.Count - 1].roomPrefab, path[path.Count - 1].GetRoomPosition(), Quaternion.identity, this.transform).gameObject.layer = 6;// generates Last room
        //if (!Input.GetKey(KeyCode.Space)) SpawnEnemies(allEnemies);
    }

    private void SpawnEnemies(List<EnemyNPC> allEnemies)
    {
        foreach (var enemy in allEnemies)
        {
            DrawTriangle(enemy.position, rightOffset / 10, forwardOffset / 20, Color.red, false, 10);
            SpawnEnemy(enemy.position, 0.7f);
        }
    }

    private void GenerateGeometry(List<Room> path, List<NavMeshSurface> navMeshSurfaces, List<EnemyNPC> allEnemies, int i, List<Vector3> splinePath)
    {
        splinePath.Add(splinePath[splinePath.Count - 1] + (splinePath[splinePath.Count - 1] - splinePath[splinePath.Count - 2]));
        Curve newCurve = Instantiate(curveMesh, this.transform);
        newCurve.points = splinePath;
        newCurve.Apply();
        newCurve.gameObject.layer = 6;
        newCurve.gameObject.AddComponent<MeshCollider>();
        navMeshSurfaces.Add(newCurve.gameObject.AddComponent<NavMeshSurface>());
        int randomCount = systemRand.Next(0, maxEnemiesOnPath + 1);
        List<EnemyNPC> newEnemies = InitiateEnemyOnPath(splinePath, randomCount);
        foreach (var enemy in newEnemies) allEnemies.Add(enemy);
        Instantiate(path[i].roomPrefab, path[i].GetRoomPosition(), Quaternion.identity, this.transform).gameObject.layer = 6;
    }

    public void SpawnEnemy(Vector3 position, float hightOffset)
    {
        NetworkObject enemy = Instantiate(enemyPrefab, position, Quaternion.LookRotation(transform.forward)).GetComponent<NetworkObject>();
        enemy.Spawn();
        spawnedEnemies.Add(enemy.gameObject);
    }
    private List<EnemyNPC> InitiateEnemyOnPath(List<Vector3> path, int count)
    {
        List<EnemyNPC> enemies = new List<EnemyNPC>();
        List<int> rands = new List<int>();
        for (int i = 0; i < count; i++)
        {
            int randInt = systemRand.Next((int)(path.Count * 0.2f), (int)((path.Count - 1) * 0.8f));
            if(rands.Contains(randInt))continue;
            enemies.Add(new EnemyNPC(path[randInt]));
            rands.Add(randInt);
        }
        return enemies;
    }
    void VisualiseRoomNPC(Room room, float scale = 0.2f)
    {
        float width = forwardOffset * scale;
        float height = rightOffset * scale;
        if (room.roomNpc is StudentNPC) DrawTriangle(room.GetRoomPosition(), width, height, new Color(255, 137, 0)); 
        if (room.roomNpc is TeacherNPC) DrawTriangle(room.GetRoomPosition(), width, height, new Color(131, 53, 184),false);
    }
    void  SpawnRoomNPC(Room room, float hightOffset = 2)
    {
        if (room.roomNpc is StudentNPC)
        {
            QuestStudentNPC student = Instantiate(studentPrefab, room.GetRoomPosition() + Vector3.up * hightOffset, Quaternion.identity, this.transform);
            student.self = room.roomNpc;
        }
        if (room.roomNpc is TeacherNPC)
        {
            QuestTeacherNPC teacher = Instantiate(teacherPrefab, room.GetRoomPosition() + Vector3.up * hightOffset, Quaternion.identity, this.transform);
            teacher.self = room.roomNpc;
        }
    }

    void DrawTriangle(Vector3 start, float width, float height, Color color, bool downwards = true, int iterations = 5)
    {
        if (iterations <= 0)
            return;
        Vector3 p1 = start;
        Vector3 p2 = new Vector3(start.x + width / 2, 0, (downwards ? start.z - height : start.z + height));
        Vector3 p3 = new Vector3(start.x + width, start.y, start.z);
        Debug.DrawLine(p1, p2, color, drawingDelay * 1.5f);
        Debug.DrawLine(p2, p3, color, drawingDelay * 1.5f);
        Debug.DrawLine(p3, p1, color, drawingDelay * 1.5f);
        float newWidth = width * 0.8f;
        float newHeight = height * 0.8f;
        DrawTriangle(p1, newWidth, newHeight, color, downwards, iterations - 1);
    }

    private void ResetRooms()
    {
        systemRand = new Random(seed);
        for (int i = transform.childCount-1; i >= 0 ; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            Destroy(spawnedEnemies[i]);
        }
        spawnedEnemies.Clear();
        generation.Clear();

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
    void AddLayer(List<RoomsLayer> roomLayers)
    {
        Multiply(roomLayers[roomLayers.Count-1],roomLayers);
    }
    private void VisualiseRooms(List<RoomsLayer> roomLayers)
    {
        foreach (var layer in roomLayers)
        {
            foreach (var room in layer.roomPositions) Debug.DrawLine(room.GetRoomPosition(), room.GetRoomPosition() + Vector3.forward, Color.cyan, drawingDelay/2);
        }
    }

    Room GeneratePyramid(Vector3 origin, List<RoomsLayer> roomLayers)
    {
        List<EnemyNPC> mainPathEnemies = new List<EnemyNPC>();
        List<EnemyNPC> branchedPathEnemies = new List<EnemyNPC>();
        List<Room> outQuestNPC = new List<Room>();
        Multiply(AddInitialRoom(origin,roomLayers),roomLayers);
        for (int i = 0; i < numberOfRooms; i++)
        {
            AddLayer(roomLayers);
        }
        VisualiseRooms(roomLayers);
        int rand = systemRand.Next(0, roomLayers[roomLayers.Count - 1].roomPositions.Count);
        Room from = roomLayers[roomLayers.Count - 1].roomPositions[rand];
        Room to = roomLayers[0].roomPositions[0];
        List<Room> correctPath = FindPath(from, to,roomLayers);
        List<NavMeshSurface> navMeshSurfaces; 
        GeneratePath(correctPath, Color.green, out navMeshSurfaces, out mainPathEnemies, true);
        List<NavMeshSurface> newNavMeshSurfaces; 
        InitializeBranches(correctPath, numberOfBranches, maxDepthOfBranch, out newNavMeshSurfaces, out branchedPathEnemies, out outQuestNPC);
        foreach (var surface in newNavMeshSurfaces) navMeshSurfaces.Add(surface);
        if(Input.GetKey(KeyCode.LeftShift))BakeNavMesh(navMeshSurfaces);
        SpawnEnemies(mainPathEnemies);
        SpawnEnemies(branchedPathEnemies);
        foreach (var questNPC in outQuestNPC) SpawnRoomNPC(questNPC);
        return from;
    }
    private RoomsLayer AddInitialRoom(Vector3 origin, List<RoomsLayer> roomLayers)
    {
        RoomsLayer initialLayer = new RoomsLayer(0);
        int randInt = systemRand.Next(0, roomPrefabs.Count);
        RoomInfo roomInfo = roomPrefabs[randInt];
        initialLayer.roomPositions = new List<Room>() { new Room(Vector3.zero, origin, 0,new Door(roomInfo.GetEntrancePosition()+ origin, roomInfo.normalEntrance), new Door(roomInfo.GetExitPosition()+ origin, roomInfo.normalExit),roomInfo) };
        roomLayers.Add(initialLayer);
        //Debug.Log("SHuthirsfu" + initialLayer.roomPositions[0].origin);
        return initialLayer;
    }

    void Multiply(RoomsLayer layer, List<RoomsLayer> roomLayers)
    {
        RoomsLayer newLayer = new RoomsLayer(layer.layerIndex +1);
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
            Room firstBranch = new Room(variation1, room.origin + new Vector3(rightOffset, 0, forwardOffset), newLayer.layerIndex,null,null, roomInfo);
            Room secondBranch = new Room(variation2, room.origin + new Vector3(-rightOffset, 0, forwardOffset), newLayer.layerIndex,null,null, roomInfo);
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





