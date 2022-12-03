using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Room : IEquatable<Room>
{
    //lowest x, y, and z valued cell in room
    //Cell parentCell;

    //cells contained by room
    //index 0 is parent cell, lowest x, y, and z value in the room
    public List<Cell> cells = new List<Cell>();

    public Vector3 center;

    //Find average position of cells to find center of room
    public void CalculateCenter()
    {
        Vector3 avgPos = Vector3.zero;

        foreach(Cell c in cells)
        {
            avgPos += c.center;
        }

        avgPos = avgPos / cells.Count;

        center = avgPos;
    }

    public bool Equals(Room other)
    {
        return this == other;
    }

    //We could go in depth here and check if each cell is equivalent, but in reality, rooms should theoretically never have the same center so we shouldn't need to worry
    public static bool operator ==(Room lhs, Room rhs)
    {
        //Check if either is null before accessing variables
        if(lhs is Room && rhs is Room)
        {
            return (lhs.center == rhs.center) && (lhs.cells.Count == rhs.cells.Count);
        }

        //If at least one parameter was null, return true if both are
        return (lhs is Room && rhs is Room);
    } 

    public static bool operator !=(Room lhs, Room rhs)
        => !(lhs == rhs);

    //public static bool operator ==(Room lhs, Edge rhs)
    //    => ((lhs.center == rhs.pointA) || (lhs.center == rhs.pointB));

    //public static bool operator !=(Room lhs, Edge rhs)
    //    => !(lhs == rhs);
}






public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] GameObject room;
    [SerializeField] GameObject roomParent;

    [SerializeField] int numRandomRooms = 4;
    [SerializeField] Vector3 maxSize = new Vector3(3, 1, 3);
    [SerializeField] Vector3 minSize = new Vector3(1, 1, 1);

    Grid grid;

    //Dimensions of cell
    [SerializeField] float cellWidth = 2.0f;   //X
    [SerializeField] float cellHeight = 2.0f;  //Y
    [SerializeField] float cellDepth = 2.0f;   //Z

    public Vector3 CellDimensions
    {
        get { return new Vector3(cellWidth, cellHeight, cellDepth); }
    }

    //Number of cells in each direction
    [SerializeField] int cellCountX = 10;
    [SerializeField] int cellCountY = 10;
    [SerializeField] int cellCountZ = 10;

    public Vector3 GridDimensions
    {
        get { return new Vector3(cellCountX, cellCountY, cellCountZ); }
    }

    [SerializeField] List<Room> rooms;

    List<Tetrahedron> tetrahedrons = new List<Tetrahedron>();

    Tetrahedron superTetrahedron;

    //Will store adjacency list of edges in tetrahedralization excluding duplicates
    Dictionary<Vector3, List<Edge>> totalEdges = new Dictionary<Vector3, List<Edge>>();

    MinimumSpanningTree minAlgorithm;   //We make this a component so we can use DrawGizmos within the class itself

    //Dictionary<Room, List<Room>> roomMap = new Dictionary<Room, List<Room>>();

    private void Start()
    {
        grid = GetComponent<Grid>();
        minAlgorithm = GetComponent<MinimumSpanningTree>();

        grid.InitGrid(new Vector3(cellWidth, cellHeight, cellDepth), new Vector3(cellCountX, cellCountY, cellCountZ));

        Generate();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Clear();
            Generate();
        }
    }

    void Clear()
    {
        //Empty all arrays and delete all current rooms
        tetrahedrons.Clear();
        rooms.Clear();
        totalEdges.Clear();
        //roomMap.Clear();
        foreach(Transform child in roomParent.transform)
        {
            Destroy(child.gameObject);
        }
        grid.InitGrid(new Vector3(cellWidth, cellHeight, cellDepth), new Vector3(cellCountX, cellCountY, cellCountZ));
    }

    void Generate()
    {
        GenerateRandomRooms();

        CreateConnectedMap();

        Vector3 start = Vector3.zero;
        IEnumerator enumerator = totalEdges.Keys.GetEnumerator(); //Get enumerator of keys
        bool success = enumerator.MoveNext();   //Move to first key
        if(success) //Check that key exists
        {
            start = (Vector3)enumerator.Current;    //Get key
        }
        else
        {
            throw new Exception("No keys in edge map.  Can not execute MST");
        }
        
        List<Edge> minSpanTree = minAlgorithm.DerriveMST(out List<Edge> excluded, start, totalEdges);

        AddRandomHallways(ref minSpanTree, excluded);

        ConvertEdgesBackToRooms(minSpanTree);

        CarveHallways();
    }

    void GenerateRandomRooms()
    {
        for (int i = 0; i < numRandomRooms; i++)
        {
            Vector3 randPos = Vector3.zero;

            //Number of room segments in each direction
            Vector3 randSize = new Vector3(
                UnityEngine.Random.Range(minSize.x, maxSize.x),
                UnityEngine.Random.Range(minSize.y, maxSize.y),
                UnityEngine.Random.Range(minSize.z, maxSize.z)
            );

            //I have this commented out because there are too many variables that go into preventing degenerate tetrahedrals.
            //If the 4 given points lie on the same plane (the plane can have any orientation) then tetrahedralization on those points fails
            //It may be simpler to just remove rooms that have a degenerate tetrahedral
            //Maybe we can go back and add 2d triangulation oriented to the plane?
            //The system below won't work because rooms can still be placed in between the 4 extremal rooms which, with only 5 rooms for example,
            //tetrahedralization would fail between the first room and the room on the other side of the fifth room

            //Ensure the first four rooms are in the corners, ensures that tetrahedralization will not be degenerate
            //if(i == 0)  //stick first room at first cell
            //{
            //    //randSize.y = 1;
            //}
            //else if(i == 1)
            //{
            //    randPos = new Vector3(CellDimensions.x * (GridDimensions.x - randSize.x), 0, 0);
            //    //randSize.y = 1;
            //}
            //else if(i == 2)
            //{
            //    randPos = new Vector3(0, CellDimensions.y * (GridDimensions.y - randSize.y), 0);
            //}
            //else if (i == 3)
            //{
            //    randPos = new Vector3(0, 0, CellDimensions.z * (GridDimensions.z - randSize.z));
            //}
            //else
            //{
                randPos = new Vector3(
                    CellDimensions.x * UnityEngine.Random.Range(0, GridDimensions.x),
                    CellDimensions.y * UnityEngine.Random.Range(0, GridDimensions.y),
                    CellDimensions.z * UnityEngine.Random.Range(0, GridDimensions.z)
                );
            //}

            Vector3 gridCenter = grid.GetCenter(randPos);
            Vector3 gridIndices = grid.GetGridIndices(randPos);

            //Make sure room exists before defining one for the map
            if (gridIndices.x < GridDimensions.x && gridIndices.x >= 0 &&
               gridIndices.y < GridDimensions.y && gridIndices.y >= 0 &&
               gridIndices.z < GridDimensions.z && gridIndices.z >= 0)
            {
                Room newRoom = new Room();

                //Loop through random size to add rooms and make the one room the size of randSize
                for (int j = 0; j < randSize.x; j++)
                {
                    for (int k = 0; k < randSize.y; k++)
                    {
                        for (int l = 0; l < randSize.z; l++)
                        {
                            //Get index of current space based on first grid index (gridIndices) plus the indices of (j, k, l)
                            Vector3 currentIndices = gridIndices + new Vector3(j, k, l);

                            //Check that placing a room here is a valid action
                            if(!grid.CanPlaceRoom(currentIndices, newRoom))
                            {
                                continue;
                            }

                            Vector3 currentCenter = grid.GetCenterByIndices(currentIndices);

                            grid.GetCell(currentIndices).cellType = CellTypes.ROOM;
                            newRoom.cells.Add(grid.GetCell(currentIndices));

                            Transform trans = Instantiate(room, currentCenter, Quaternion.identity, roomParent.transform).transform;

                            trans.localScale = CellDimensions;
                        }
                    }
                }

                newRoom.CalculateCenter();

                rooms.Add(newRoom);
            }
        }
    }

    void CreateConnectedMap()
    {

        superTetrahedron = new Tetrahedron(
            new Vector3(-cellWidth * cellCountX, -cellHeight * cellCountY, -cellDepth * cellCountZ) * 2,
            new Vector3(cellWidth * cellCountX * 4, -cellHeight, -cellDepth),
            new Vector3(-cellWidth, cellHeight * cellCountY * 4, -cellDepth),
            new Vector3(-cellWidth, -cellHeight, cellDepth * cellCountZ * 4)
        );

        //Define super tetrahedron that contains all rooms
        //tetrahedrons.Add(superTetrahedron);

        List<Vector3> pointList = new List<Vector3>();
        foreach(Room room in rooms)
        {
            pointList.Add(room.center);
        }

        //Perform tetrahedralization
        tetrahedrons = DelaunayTriangulation.Tetrahedralize(superTetrahedron, pointList);

        //Count up each edge in tetrahedralization once and add a second with the points reversed so we have an adjacency list of an
        //undirected graph
        foreach(Tetrahedron tet in tetrahedrons)
        {
            Edge[] edges = tet.GetEdges();

            foreach(Edge e in edges)
            {
                List<Edge> list;
                bool added = false, contains = false;

                //Add the edge to the dictionary if it isn't in there already
                if(totalEdges.TryGetValue(e.pointA, out list))
                {
                    if (!list.Contains(e))
                    {
                        list.Add(e);
                    }
                }
                else
                {
                    List<Edge> newList = new();
                    newList.Add(e);
                    totalEdges.Add(e.pointA, newList);
                }

                //Create a duplicate of the edge with the points reversed so we have an undirected graph
                if (totalEdges.TryGetValue(e.pointB, out list))
                {
                    if (!list.Contains(e))
                    {
                        list.Add(new Edge(e.pointB, e.pointA));
                    }
                }
                else
                {
                    List<Edge> newList = new();
                    newList.Add(new Edge(e.pointB, e.pointA));
                    totalEdges.Add(e.pointB, newList);
                }

                //if(!added && !contains && totalEdges.TryGetValue(e.pointB, out list))
                //{
                //    if (!list.Contains(e))
                //    {
                //        list.Add();

                //        if (!totalEdges.TryGetValue(e.pointA, out list))
                //        {
                //            list = new List<Edge>();
                //            list.Add(new Edge(e.pointB, e.pointA));
                //            totalEdges.Add(e.pointB, list);
                //        }
                //        else
                //        {
                //            list.Add(new Edge(e.pointB, e.pointA));
                //        }

                //        added = true;
                //    }
                //    else
                //    {
                //        contains = true;
                //    }
                //}

                //if(!added && !contains)
                //{
                //    List<Edge> newList = new();
                //    newList.Add(e);
                //    totalEdges.Add(e.pointA, newList);
                //}
            }
        }

        //int count = 0;
        //foreach (KeyValuePair<Vector3, List<Edge>> pair in totalEdges)
        //{
        //    foreach (Edge e in pair.Value)
        //    {
        //        Debug.DrawLine(e.pointA, e.pointB, Color.red, 999f);
        //        count++;
        //    }
        //}
        //Debug.Log(count);

        ////Delete edges that pass through other rooms, this happens because tetrahedralization deals with points but we deal with rooms that have sizes
        //foreach (KeyValuePair<Room, List<Room>> pair in roomMap)
        //{
        //    foreach(Room room in pair.Value)
        //    {
        //        Ray ray = new Ray(room.center, pair.Key.center - room.center);
        //        if(Physics.Raycast(ray, out RaycastHit data))
        //        {
        //Not going to worry about how to get it to understand which room objects belong to which rooms yet for raycast
        //        }
        //    }
        //}
    }

    void AddRandomHallways(ref List<Edge> minSpanTree, List<Edge> excluded)
    {
        //Add random number of edges from excluded to minSpanTree
    }

    void ConvertEdgesBackToRooms(List<Edge> finalMap)
    {
        //Match rooms to edges to get room map
        //foreach (KeyValuePair<Edge, int> pair in totalEdges)
        //{
        //    if (pair.Value > 0)
        //    {
        //        Room room1 = null;
        //        Room room2 = null;

        //        foreach (Room room in rooms)
        //        {
        //            if (room.center == pair.Key.pointA)
        //            {
        //                room1 = room;
        //            }
        //            else if (room.center == pair.Key.pointB)
        //            {
        //                room2 = room;
        //            }
        //        }

        //        if (room1 != null && room2 != null)
        //        {
        //            if (roomMap.TryGetValue(room1, out List<Room> roomList))
        //            {
        //                roomList.Add(room2);
        //            }
        //            else
        //            {
        //                List<Room> newRooms = new List<Room>();
        //                newRooms.Add(room2);
        //                roomMap.Add(room1, newRooms);
        //            }
        //        }
        //    }
        //}
    }

    void CarveHallways()
    {

    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {

            //Tetrahedron test = new Tetrahedron(Vector3.zero, Vector3.up * 4, Vector3.right, Vector3.forward);
            //test.DrawGizmos();
            //Gizmos.DrawSphere(test.circumSphere.center, test.circumSphere.radius);

            //Triangle triangle = new Triangle(rooms[0].center, rooms[1].center, rooms[2].center);
            //Circumsphere sphere = DelaunayTriangulation.FindCircumcenter(triangle);

            //Vector3 diff = (rooms[0].center - sphere.center).normalized * sphere.radius;

            //Gizmos.DrawSphere(sphere.center, sphere.radius);
            //Gizmos.DrawLine(sphere.center, sphere.center + diff);
            //Gizmos.DrawSphere(rooms[0].center, .1f);
            //Gizmos.DrawSphere(sphere.center, .1f);

            //Circumsphere sphere = DelaunayTriangulation.FindCircumcenter(triangles[0]);
            //triangles[0].DrawGizmos();
            //Gizmos.DrawSphere(sphere.center, sphere.radius);

            //foreach (Tetrahedron tet in tetrahedrons)
            //{
            //    //Room currentRoom = pair.Key;
            //    Gizmos.color = Color.red;
            //    tet.DrawGizmos();
            //}

            //foreach (KeyValuePair<Vector3, List<Edge>> pair in totalEdges)
            //{
            //    Gizmos.color = Color.red;
            //    foreach (Edge edge in pair.Value)
            //    {
            //        Gizmos.DrawLine(edge.pointA, edge.pointB);
            //    }
            //}

            //foreach (KeyValuePair<Room, List<Room>> pair in roomMap)
            //{
            //    Gizmos.color = Color.red;
            //    foreach (Room room in pair.Value)
            //    {
            //        Gizmos.DrawLine(pair.Key.center, room.center);
            //    }
            //}

            //Gizmos.DrawSphere(superTetrahedron.circumSphere.center, superTetrahedron.circumSphere.radius);
            //superTetrahedron.DrawGizmos();

            //foreach (Tetrahedron tet in tetrahedrons)
            //{
            //    //Room currentRoom = pair.Key;
            //    Gizmos.color = Color.red;
            //    tet.DrawGizmos();
            //    Gizmos.DrawSphere(tet.circumSphere.center, tet.circumSphere.radius);
            //}

            //foreach (Triangle tri in triangles)
            //{
            //    //Room currentRoom = pair.Key;
            //    Gizmos.color = Color.red;
            //    Gizmos.DrawLine(tri.pointA, tri.pointB);
            //    Gizmos.DrawLine(tri.pointA, tri.pointC);
            //    Gizmos.DrawLine(tri.pointC, tri.pointB);
            //}

            //foreach (KeyValuePair<Room, List<Room>> pair in roomMap)
            //{
            //    Room currentRoom = pair.Key;

            //    foreach (Room connectedRoom in pair.Value)
            //    {
            //        Gizmos.color = Color.red;
            //        Gizmos.DrawLine(currentRoom.cells[0].center, connectedRoom.cells[0].center);
            //    }
            //}
        }
    }
}
