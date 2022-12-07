using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
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

        foreach (Cell c in cells)
        {
            avgPos += c.center;
        }

        avgPos = avgPos / cells.Count;

        center = avgPos;
    }

    //If one of the cells in the room is next to the passed index (including above or below), return true
    public bool HasAdjacentCell(Vector3Int index)
    {
        foreach (Cell c in cells)
        {
            if ((c.index - index).sqrMagnitude == 1)
            {
                return true;
            }
        }

        return false;
    }

    //If one of the cells in the room is next to the passed index (not above or below), return true
    public bool HasLevelAdjacentCell(Vector3Int index, out Cell adjacentCell)
    {
        adjacentCell = default;

        foreach(Cell c in cells)
        {
            if((c.index - index).sqrMagnitude == 1 && index.y == c.index.y)
            {
                adjacentCell = c;
                return true;
            }
        }

        return false;
    }

    //If one of the cells in the room matches the passed index, return true
    public bool ContainsIndex(Vector3Int index, out Cell outCell)
    {
        outCell = default;
        foreach (Cell c in cells)
        {
            if (index == c.index)
            {
                outCell = c;
                return true;
            }
        }

        return false;
    }


    //Returns the indices of the closest room to the goal index that has an open side so A star can perform
    public Vector3Int ClosestValidStartRoom(Vector3 goalIndex, Grid grid)
    {
        //Initialize closes to to far away, invalid value
        Vector3Int closest = new Vector3Int((int)grid.GridDimensions.x, (int)grid.GridDimensions.y, (int)grid.GridDimensions.z) * 3;
        for(int i = 1; i < cells.Count; i++)
        {
            if(cells[i].HasFreeLevelAdjacentCell(grid) &&                                      //If has free adjacent space to the N, S, E, or W 
               (closest - goalIndex).sqrMagnitude > (cells[i].index - goalIndex).sqrMagnitude) //and closer to goal
            {
                closest = cells[i].index;
            }
        }

        return closest;
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
        return lhs is null && rhs is null;
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
    Grid grid;

    [Header("Cell Dimensions")]
    //Dimensions of cell
    [SerializeField] float cellWidth = 2.0f;   //X
    [SerializeField] float cellHeight = 2.0f;  //Y
    [SerializeField] float cellDepth = 2.0f;   //Z

    public Vector3 CellDimensions
    {
        get { return new Vector3(cellWidth, cellHeight, cellDepth); }
    }

    [Header("Grid Dimensions")]
    //Number of cells in each direction
    [SerializeField] int cellCountX = 10;
    [SerializeField] int cellCountY = 10;
    [SerializeField] int cellCountZ = 10;

    public Vector3 GridDimensions
    {
        get { return new Vector3(cellCountX, cellCountY, cellCountZ); }
    }

    [Header("Room Parameters")]
    [SerializeField] GameObject room;
    [SerializeField] GameObject roomParent;

    [SerializeField] int numRandomRooms = 4;
    [SerializeField] Vector3 maxSize = new Vector3(3, 1, 3);
    [SerializeField] Vector3 minSize = new Vector3(1, 1, 1);

    [Header("Path Parameters")]
    [SerializeField] GameObject pathParent;
    [SerializeField] GameObject hallPrefab;
    [SerializeField] GameObject stairsPrefab;
    [SerializeField] GameObject stairSpacePrefab;
    [SerializeField] GameObject wallPrefab;
    [SerializeField] GameObject doorwayPrefab;
    [SerializeField] GameObject pillarPrefab; //We store this prefab to add the pillar to the southeast corner of rooms it is not automatically added to from a wall
    [SerializeField] GameObject archPrefab; //Add between cells of the same type where a wall was not added
    [SerializeField] GameObject archPillarPrefab;   //For rooms primarily, only usedfor north checks so extra pillars aren't added where they already exist

    [Header("Other")]
    [SerializeField] float percentEnableLights = .2f; //percentage [0, 1] of lights on walls enabled

    [Header("Gameplay")]
    [SerializeField] GameObject playerPrefab;

    [SerializeField] float extraHallwaysFactor = .5f;   //Adds (<extra> * leftover room count) number of rooms after minimum spanning tree determined

    List<Room> rooms = new();

    List<Tetrahedron> tetrahedrons = new List<Tetrahedron>();

    Tetrahedron superTetrahedron;

    //Will store adjacency list of edges in tetrahedralization excluding duplicates
    Dictionary<Vector3, List<Edge>> totalEdges = new Dictionary<Vector3, List<Edge>>();

    MinimumSpanningTree minAlgorithm = new();

    Dictionary<Room, List<Room>> roomMap = new Dictionary<Room, List<Room>>();

    [SerializeField] Vector3Int test;

    private void Start()
    {
        grid = GetComponent<Grid>();

        grid.InitGrid(new Vector3(cellWidth, cellHeight, cellDepth), new Vector3(cellCountX, cellCountY, cellCountZ));

        //Generate();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
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
        roomMap.Clear();
        foreach (Transform child in roomParent.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in pathParent.transform)
        {
            Destroy(child.gameObject);
        }
        grid.InitGrid(new Vector3(cellWidth, cellHeight, cellDepth), new Vector3(cellCountX, cellCountY, cellCountZ));
    }

    void Generate()
    {
        //Transform trans = Instantiate(room, new Vector3(0 * cellWidth, 0 * cellHeight, 0 * cellDepth), Quaternion.identity, roomParent.transform).transform;
        //trans.localScale = CellDimensions;

        //trans = Instantiate(room, new Vector3(test.x * cellWidth, test.y * cellHeight, test.z * cellDepth), Quaternion.identity, roomParent.transform).transform;
        //trans.localScale = CellDimensions;

        //Stack<AStarNode> path = AStar.Run(new Vector3Int(0, 0, 0), test, grid);

        ////path might return null if A* failed
        //if (path != null)
        //{
        //    foreach (AStarNode node in path)
        //    {
        //        grid.GetCell(node.indices).cellType = CellTypes.HALLWAY;

        //        Vector3 pos = new Vector3(node.indices.x * cellWidth, node.indices.y * cellHeight, node.indices.z * cellDepth);
        //        trans = Instantiate(hall, pos, Quaternion.identity, roomParent.transform).transform;

        //        trans.localScale = CellDimensions;
        //    }
        //}
        //else
        //{
        //    Debug.Log("null");
        //}

        double realtime = Time.realtimeSinceStartupAsDouble;
        GenerateRandomRooms();

        CreateConnectedMap();

        Vector3 start = Vector3.zero;
        IEnumerator enumerator = totalEdges.Keys.GetEnumerator(); //Get enumerator of keys
        bool success = enumerator.MoveNext();   //Move to first key
        if (success) //Check that key exists
        {
            start = (Vector3)enumerator.Current;    //Get key
        }
        else
        {
            throw new Exception("No keys in edge map.  Can not execute MST");
        }

        List<Edge> minSpanTree = minAlgorithm.DerriveMST(out List<Edge> excluded, start, totalEdges);

        AddRandomHallways(ref minSpanTree, ref excluded);

        ConvertEdgesBackToRooms(minSpanTree);

        CarveHallways();

        PlaceWalls();

        Debug.Log("Algorithm Time: " + (float)(Time.realtimeSinceStartupAsDouble - realtime) + " seconds");
    }

    void GenerateRandomRooms()
    {
        //To add noise for pseudo-randomness, we could sample perlin noise at the worldspace position of each cell (or average perlin noise in each cell from
        //a set of candidate points) and, if it is above some threshold, we make it a room.

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
            Vector3Int gridIndices = grid.GetGridIndices(randPos);

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
                            Vector3Int currentIndices = gridIndices + new Vector3Int(j, k, l);

                            //Check that placing a room here is a valid action
                            if (!grid.CanPlaceRoom(currentIndices, newRoom))
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
        foreach (Room room in rooms)
        {
            pointList.Add(room.center);
        }

        //Perform tetrahedralization
        tetrahedrons = DelaunayTriangulation.Tetrahedralize(superTetrahedron, pointList);

        //Count up each edge in tetrahedralization once and add a second with the points reversed so we have an adjacency list of an
        //undirected graph
        foreach (Tetrahedron tet in tetrahedrons)
        {
            Edge[] edges = tet.GetEdges();

            foreach (Edge e in edges)
            {
                List<Edge> list;

                //Add the edge to the dictionary if it isn't in there already
                if (totalEdges.TryGetValue(e.pointA, out list))
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
            }
        }
    }

    void AddRandomHallways(ref List<Edge> minSpanTree, ref List<Edge> excluded)
    {
        //Add random number of edges from excluded to minSpanTree
        int numHalls = (int)(extraHallwaysFactor * excluded.Count);

        for (int i = 0; i < numHalls; i++)
        {
            int hallIndex = UnityEngine.Random.Range(0, excluded.Count - 1);

            minSpanTree.Add(excluded[i]);
            excluded.RemoveAt(i);
        }
    }

    void ConvertEdgesBackToRooms(List<Edge> finalMap)
    {
        //Match rooms to edges to get room map
        foreach (Edge e in finalMap)
        {
            Room room1 = null;
            Room room2 = null;

            foreach (Room room in rooms)
            {
                if (room.center == e.pointA)
                {
                    room1 = room;
                }
                else if (room.center == e.pointB)
                {
                    room2 = room;
                }
            }

            if (room1 != null && room2 != null)
            {
                if (roomMap.TryGetValue(room1, out List<Room> roomList))
                {
                    roomList.Add(room2);
                }
                else
                {
                    List<Room> newRooms = new List<Room>();
                    newRooms.Add(room2);
                    roomMap.Add(room1, newRooms);
                }
            }
        }
    }

    void CarveHallways()
    {
        foreach (KeyValuePair<Room, List<Room>> pair in roomMap)
        {
            foreach (Room room in pair.Value)
            {
                float realtime = Time.realtimeSinceStartup;

                //find closest room cell in the goal room and make that our A* target
                //Vector3 goal = room.cells[0].center;
                //for(int i = 1; i < room.cells.Count; i++)
                //{
                //    if((goal - pair.Key.center).sqrMagnitude > (room.cells[i].center - pair.Key.center).sqrMagnitude)
                //    {
                //        goal = room.cells[i].center;
                //    }
                //}

                ////Find closest room to start from
                //Vector3 start = pair.Key.cells[0].center;
                //for (int i = 1; i < pair.Key.cells.Count; i++)
                //{
                //    if ((start - goal).sqrMagnitude > (pair.Key.cells[i].center - goal).sqrMagnitude)
                //    {
                //        start = pair.Key.cells[i].center;
                //    }
                //}

                Vector3Int startIndices = pair.Key.ClosestValidStartRoom(room.center, grid);

                if (grid.IsValidCell(startIndices))
                {
                    Stack<AStarNode> path = AStar.Run(startIndices, grid.GetGridIndices(room.center), room, grid);

                    Transform currentPathParent = new GameObject().transform;
                    currentPathParent.name = "Path";
                    currentPathParent.parent = pathParent.transform;

                    //path might return null if A* failed
                    if (path != null)
                    {
                        //Store the last Y value so we know when we've added a stairwell
                        Vector3Int lastIndices = startIndices;
                        foreach (AStarNode node in path)
                        {
                            //Get center position of unit
                            Vector3 pos = new Vector3(node.indices.x * cellWidth, node.indices.y * cellHeight, node.indices.z * cellDepth);

                            if (node.indices.y < lastIndices.y) //Stairwell down
                            {
                                //Stair cell
                                grid.GetCell(node.indices).cellType = CellTypes.STAIRS; //Mark next space as stairs

                                //Cache rotation so the space above the stairs has the same rotation and its walls appear on the correct sides
                                Quaternion stairRotation = GetStairRotation(lastIndices, node.indices);

                                Transform trans = Instantiate(stairsPrefab, pos, stairRotation, currentPathParent).transform; //Spawn stairwell
                                trans.localScale = CellDimensions;  //Scale the unit to fit the grid cell

                                //Stairspace cell
                                pos += new Vector3(0, cellHeight, 0); //Update position to be the cell above the current node
                                Vector3Int spaceIndex = new Vector3Int(node.indices.x, node.indices.y + 1, node.indices.z);

                                //Mark space above next space as stairspace so it remains empty and their is space to go down the stairs
                                grid.GetCell(spaceIndex).cellType = CellTypes.STAIRSPACE;
                                grid.GetCell(spaceIndex).faceDirection = Cell.OppositeDirection(grid.GetCell(node.indices).faceDirection);
                                trans = Instantiate(stairSpacePrefab, pos, stairRotation, currentPathParent).transform; //Spawn stairspace
                                trans.localScale = CellDimensions; //Scale the unit to fit the grid cell
                            }
                            else if (node.indices.y > lastIndices.y) //Stairwell up
                            {
                                //Stair cell
                                Vector3Int stairIndex = new Vector3Int(node.indices.x, node.indices.y - 1, node.indices.z);

                                Quaternion stairRotation = GetStairRotation(lastIndices, stairIndex);

                                grid.GetCell(stairIndex).cellType = CellTypes.STAIRS;  //Mark as stairs
                                Transform trans = Instantiate(stairsPrefab, pos - new Vector3(0, cellHeight, 0), stairRotation, currentPathParent).transform; //Spawn stair, Update position to be the cell below the current node
                                trans.localScale = CellDimensions; //Scale the unit to fit the grid cell

                                //Stairspace cell, cells up diagonally must be an empty space and the cell below them holds the actual stairs
                                grid.GetCell(node.indices).cellType = CellTypes.STAIRSPACE; //Mark next space as stairs
                                grid.GetCell(node.indices).faceDirection = Cell.OppositeDirection(grid.GetCell(stairIndex).faceDirection);

                                trans = Instantiate(stairSpacePrefab, pos, stairRotation, currentPathParent).transform; //Spawn stair space
                                trans.localScale = CellDimensions;  //Scale the unit to fit the grid cell
                            }
                            else //Hallway
                            {
                                //Mark grid cell as hallway
                                grid.GetCell(node.indices).cellType = CellTypes.HALLWAY;

                                //Spawn hallway
                                Transform trans = Instantiate(hallPrefab, pos, Quaternion.identity, currentPathParent).transform;

                                //Scale the unit to fit the grid cell
                                trans.localScale = CellDimensions;
                            }

                            //Update last y with this node's y value
                            lastIndices = node.indices;
                        }
                    }
                    else
                    {
                        Debug.LogError("A-Star Path Failed");
                    }

                    //Log the time this step took
                    Debug.Log("Path Time: " + (Time.realtimeSinceStartup - realtime));
                }
                else
                {
                    Debug.LogError("Valid starting cell could not be found");
                }
            }
        }
    }

    private void PlaceWalls()
    {
        //Do a pass of all cells and determine what types of walls to add 
        for (int x = 0; x < cellCountX; x++)
        {
            for (int y = 0; y < cellCountY; y++)
            {
                for (int z = 0; z < cellCountZ; z++)
                {
                    Vector3Int currentIndex = new Vector3Int(x, y, z);
                    //Check this node's cell type to determine what walls should be added
                    CellTypes type = grid.GetCell(currentIndex).cellType;

                    switch(type)
                    {
                        case CellTypes.ROOM:
                            //Only check north and east, the previous node will have handled the checks for south and west
                            RoomWall(currentIndex, currentIndex + AStar.constNorth);
                            RoomWall(currentIndex, currentIndex + AStar.constEast);

                            //If on grid bounds, there is no previous node to do this check, needs to be done by this node
                            if(currentIndex.z == 0)
                            {
                                RoomWall(currentIndex, currentIndex + AStar.constSouth);
                            }
                            if(currentIndex.x == 0)
                            {
                                RoomWall(currentIndex, currentIndex + AStar.constWest);
                            }
                            break;

                        case CellTypes.HALLWAY:
                            HallWall(currentIndex, currentIndex + AStar.constNorth);
                            HallWall(currentIndex, currentIndex + AStar.constEast);

                            if (currentIndex.z == 0)
                            {
                                HallWall(currentIndex, currentIndex + AStar.constSouth);
                            }
                            if (currentIndex.x == 0)
                            {
                                HallWall(currentIndex, currentIndex + AStar.constWest);
                            }
                            break;
                        case CellTypes.STAIRSPACE:
                            StairSpaceWall(currentIndex, currentIndex + AStar.constNorth);
                            StairSpaceWall(currentIndex, currentIndex + AStar.constEast);

                            if (currentIndex.z == 0)
                            {
                                StairSpaceWall(currentIndex, currentIndex + AStar.constSouth);
                            }
                            if (currentIndex.x == 0)
                            {
                                StairSpaceWall(currentIndex, currentIndex + AStar.constWest);
                            }
                            break;
                        case CellTypes.STAIRS:
                            StairWall(currentIndex, currentIndex + AStar.constNorth);
                            StairWall(currentIndex, currentIndex + AStar.constEast);

                            if (currentIndex.z == 0)
                            {
                                StairWall(currentIndex, currentIndex + AStar.constSouth);
                            }
                            if (currentIndex.x == 0)
                            {
                                StairWall(currentIndex, currentIndex + AStar.constWest);
                            }
                            break;
                        case CellTypes.NONE:
                            EmptyCellWall(currentIndex, currentIndex + AStar.constNorth);
                            EmptyCellWall(currentIndex, currentIndex + AStar.constEast);
                            break;
                    }
                }
            }
        }
    }

    private void RoomWall(Vector3Int currentIndex, Vector3Int adjacentIndex)
    {
        if(grid.IsValidCell(adjacentIndex))
        {
            Cell adjacentCell = grid.GetCell(adjacentIndex);

            GameObject spawnObject = null;

            switch(adjacentCell.cellType)
            {
                //Doorway
                case CellTypes.HALLWAY:
                    spawnObject = doorwayPrefab;
                    break;

                //Doorway if facing the room
                case CellTypes.STAIRS:
                    if (adjacentCell.faceDirection != Cell.DirectionCameFrom(adjacentIndex, currentIndex))
                    {
                        spawnObject = wallPrefab;
                    }
                    else
                    {
                        spawnObject = doorwayPrefab;
                    }
                    break;

                //Wall
                case CellTypes.NONE:
                    spawnObject = wallPrefab;
                    break;

                //Put up walls unless the stair space is facing the room
                case CellTypes.STAIRSPACE:
                    if (adjacentCell.faceDirection != Cell.DirectionCameFrom(adjacentIndex, currentIndex))
                    {
                        spawnObject = wallPrefab;
                    }
                    else
                    {
                        spawnObject = doorwayPrefab;
                    }
                    break;

                //Do nothing
                case CellTypes.ROOM:
                    //If moving north, add a pillar, if east, just the arch
                    if(Cell.DirectionCameFrom(currentIndex, adjacentIndex) == Directions.SOUTH)
                    {
                        spawnObject = archPillarPrefab;
                    }
                    else
                    {
                        spawnObject = archPrefab;
                    }
                    
                    break;
                //No walls between rooms
                default:
                    
                    break;
            }

            if(spawnObject != null)
            {
                Transform trans = Instantiate(spawnObject, grid.GetCenterByIndices(currentIndex), GetWallRotation(currentIndex, adjacentIndex), roomParent.transform).transform;
                trans.localScale = CellDimensions;

                if(UnityEngine.Random.Range(0f, 1f) < percentEnableLights)
                {
                    trans.gameObject.GetComponent<StartLight>()?.EnableLight();
                }
            }
        }
        else //If next index is not valid, it is empty and we need a wall
        {
            Transform trans = Instantiate(wallPrefab, grid.GetCenterByIndices(currentIndex), GetWallRotation(currentIndex, adjacentIndex), roomParent.transform).transform;
            trans.localScale = CellDimensions;

            if (UnityEngine.Random.Range(0f, 1f) < percentEnableLights)
            {
                trans.gameObject.GetComponent<StartLight>()?.EnableLight();
            }
        }
    }

    private void HallWall(Vector3Int currentIndex, Vector3Int adjacentIndex)
    {
        if (grid.IsValidCell(adjacentIndex))
        {
            Cell adjacentCell = grid.GetCell(adjacentIndex);

            GameObject spawnObject = null;

            switch (adjacentCell.cellType)
            {
                //Wall
                case CellTypes.NONE:
                    spawnObject = wallPrefab;
                    break;

                //Room
                case CellTypes.ROOM:
                    spawnObject = doorwayPrefab;
                    break;

                //Stairs and stairspace
                case CellTypes.STAIRS:
                    if (adjacentCell.faceDirection != Cell.DirectionCameFrom(adjacentIndex, currentIndex))
                    {
                        spawnObject = wallPrefab;
                    }
                    else
                    {
                        spawnObject = archPrefab;
                    }
                    break;

                case CellTypes.STAIRSPACE:
                    //Check the stairs below the stair space for direction, the stairs must be facing away  (therefore leading up the the hallway)
                    if (adjacentCell.faceDirection != Cell.DirectionCameFrom(adjacentIndex, currentIndex))
                    {
                        spawnObject = wallPrefab;
                    }
                    else
                    {
                        spawnObject = archPrefab;
                    }
                    break;

                //Just add an arch
                case CellTypes.HALLWAY:
                    spawnObject = archPrefab;
                    break;

                //Do nothing
                default:

                    break;
            }

            if (spawnObject != null)
            {
                Transform trans = Instantiate(spawnObject, grid.GetCenterByIndices(currentIndex), GetWallRotation(currentIndex, adjacentIndex), roomParent.transform).transform;
                trans.localScale = CellDimensions;

                if (UnityEngine.Random.Range(0f, 1f) < percentEnableLights)
                {
                    trans.gameObject.GetComponent<StartLight>()?.EnableLight();
                }
            }
        }
        else //If next index is not valid, it is empty and we need a wall
        {
            Transform trans = Instantiate(wallPrefab, grid.GetCenterByIndices(currentIndex), GetWallRotation(currentIndex, adjacentIndex), roomParent.transform).transform;
            trans.localScale = CellDimensions;

            if (UnityEngine.Random.Range(0f, 1f) < percentEnableLights)
            {
                trans.gameObject.GetComponent<StartLight>()?.EnableLight();
            }
        }
    }

    private void StairSpaceWall(Vector3Int currentIndex, Vector3Int adjacentIndex)
    {
        if (grid.IsValidCell(adjacentIndex))
        {
            Cell adjacentCell = grid.GetCell(adjacentIndex);

            GameObject spawnObject = null;

            switch (adjacentCell.cellType)
            {
                //Wall
                case CellTypes.NONE:
                case CellTypes.STAIRS:  //stair space shouldn't be able to move straight across to a stair space with our algorithm
                    spawnObject = wallPrefab;
                    break;

                //Check if staircase faces this cell and add wall if false, for rooms, add doorway if true
                case CellTypes.ROOM:
                    if (grid.GetCell(currentIndex).faceDirection != Cell.DirectionCameFrom(currentIndex, adjacentIndex))
                    {
                        spawnObject = wallPrefab;
                    }
                    else
                    {
                        spawnObject = doorwayPrefab;
                    }
                    break;


                case CellTypes.HALLWAY:
                    //Get stairs beneath the stair space to find if the stairs face away from the hallway (therefore leading up the the hallway)
                    if (grid.GetCell(currentIndex).faceDirection != Cell.DirectionCameFrom(currentIndex, adjacentIndex))
                    {
                        spawnObject = wallPrefab;
                    }
                    else
                    {
                        spawnObject = archPrefab;
                    }
                    break;

                //If moving in different directions, plug the wall
                case CellTypes.STAIRSPACE:
                    if(adjacentCell.faceDirection != grid.GetCell(currentIndex).faceDirection)
                    {
                        spawnObject = wallPrefab;
                    }
                    break;

                //Do nothing
                default:

                    break;
            }

            if (spawnObject != null)
            {
                Transform trans = Instantiate(spawnObject, grid.GetCenterByIndices(currentIndex), GetWallRotation(currentIndex, adjacentIndex), roomParent.transform).transform;
                trans.localScale = CellDimensions;

                if (UnityEngine.Random.Range(0f, 1f) < percentEnableLights)
                {
                    trans.gameObject.GetComponent<StartLight>()?.EnableLight();
                }
            }
        }
        else //If next index is not valid, it is empty and we need a wall
        {
            Transform trans = Instantiate(wallPrefab, grid.GetCenterByIndices(currentIndex), GetWallRotation(currentIndex, adjacentIndex), roomParent.transform).transform;
            trans.localScale = CellDimensions;

            if (UnityEngine.Random.Range(0f, 1f) < percentEnableLights)
            {
                trans.gameObject.GetComponent<StartLight>()?.EnableLight();
            }
        }
    }

    private void StairWall(Vector3Int currentIndex, Vector3Int adjacentIndex)
    {
        if (grid.IsValidCell(adjacentIndex))
        {
            Cell adjacentCell = grid.GetCell(adjacentIndex);

            GameObject spawnObject = null;

            switch (adjacentCell.cellType)
            {
                //Wall
                case CellTypes.NONE:
                    spawnObject = wallPrefab;
                    break;

                //Check if staircase faces this cell and add wall if false, for rooms, add doorway if true
                case CellTypes.ROOM:
                    if (grid.GetCell(currentIndex).faceDirection != Cell.DirectionCameFrom(currentIndex, adjacentIndex))
                    {
                        spawnObject = wallPrefab;
                    }
                    else
                    {
                        spawnObject = doorwayPrefab;
                    }
                    break;

                //Put wall up if not facing the hallway
                case CellTypes.HALLWAY:
                    if (grid.GetCell(currentIndex).faceDirection != Cell.DirectionCameFrom(currentIndex, adjacentIndex))
                    {
                        spawnObject = wallPrefab;
                    }
                    else
                    {
                        spawnObject = archPrefab;

                        //NEED WALL PREFAB WITH EXTA PIECE ON TOP BETWEEN CEILING AND ARCH
                        //otherwise we can see through the ceiling of the next hallway
                        Debug.LogWarning("Implement special case stair arch prefab here");
                    }
                    break;

                //If moving in different directions, plug the wall
                case CellTypes.STAIRS:
                    if(grid.GetCell(currentIndex).faceDirection != adjacentCell.faceDirection)
                    {
                        spawnObject = wallPrefab;
                    }
                    else
                    {
                        spawnObject = null;
                    }
                    break;

                //Should never need to be open next to a stair space (not accounting for straight staircases since this algorithm puts landings between all stairs)
                case CellTypes.STAIRSPACE:
                    spawnObject = wallPrefab;
                    break;

                //Do nothing
                default:

                    break;
            }

            if (spawnObject != null)
            {
                Transform trans = Instantiate(spawnObject, grid.GetCenterByIndices(currentIndex), GetWallRotation(currentIndex, adjacentIndex), roomParent.transform).transform;
                trans.localScale = CellDimensions;

                if (UnityEngine.Random.Range(0f, 1f) < percentEnableLights)
                {
                    trans.gameObject.GetComponent<StartLight>()?.EnableLight();
                }
            }
        }
        else //If next index is not valid, it is empty and we need a wall
        {
            Transform trans = Instantiate(wallPrefab, grid.GetCenterByIndices(currentIndex), GetWallRotation(currentIndex, adjacentIndex), roomParent.transform).transform;
            trans.localScale = CellDimensions;

            if (UnityEngine.Random.Range(0f, 1f) < percentEnableLights)
            {
                trans.gameObject.GetComponent<StartLight>()?.EnableLight();
            }
        }
    }

    private void EmptyCellWall(Vector3Int currentIndex, Vector3Int adjacentIndex)
    {
        if (grid.IsValidCell(adjacentIndex))
        {
            Cell adjacentCell = grid.GetCell(adjacentIndex);

            GameObject spawnObject = null;

            switch (adjacentCell.cellType)
            {
                //Wall
                case CellTypes.ROOM:
                case CellTypes.HALLWAY:
                case CellTypes.STAIRS:
                case CellTypes.STAIRSPACE:
                    spawnObject = wallPrefab;

                    //Adds back that one pillar on the southeast corner of each structure that is (supposed to be) missing since walls only come with pillars on one side
                    //This is part of the system that reduces duplication of models
                    if((!grid.IsValidCell(adjacentIndex + AStar.constSouth) || grid.IsCellEmpty(adjacentIndex + AStar.constSouth)) &&
                       (!grid.IsValidCell(adjacentIndex + AStar.constEast) || grid.IsCellEmpty(adjacentIndex + AStar.constEast)) &&
                       (!grid.IsValidCell(adjacentIndex + AStar.constSouth + AStar.constEast) || grid.IsCellEmpty(adjacentIndex + AStar.constSouth + AStar.constEast)))
                    {
                        Transform trans = Instantiate(pillarPrefab, grid.GetCenterByIndices(currentIndex), GetWallRotation(currentIndex, adjacentIndex), roomParent.transform).transform;
                        trans.localScale = CellDimensions;
                    }

                    break;

                //Do nothing
                case CellTypes.NONE:
                default:
                    break;
            }

            if (spawnObject != null)
            {
                Transform trans = Instantiate(spawnObject, grid.GetCenterByIndices(currentIndex), GetWallRotation(currentIndex, adjacentIndex), roomParent.transform).transform;
                trans.localScale = CellDimensions;

                if (UnityEngine.Random.Range(0f, 1f) < percentEnableLights)
                {
                    trans.gameObject.GetComponent<StartLight>()?.EnableLight();
                }
            }
        }
    }

    //return the yaw rotation of a staircase based on the positions of the stairs and the last cell
    private Quaternion GetStairRotation(Vector3Int lastIndices, Vector3Int currentIndices)
    {
        Vector3Int diff = currentIndices - lastIndices;

        float rot = 0f; //stairs face forward

        //Cell cell = ;

        if (diff.z < 0) //stairs face backward
        {
            rot = 180f;
            grid.GetCell(currentIndices).faceDirection = Directions.SOUTH;
        }
        else if(diff.x > 0) //stairs face right
        {
            rot = 90f;
            grid.GetCell(currentIndices).faceDirection = Directions.EAST;
        }
        else if(diff.x < 0) //stairs face left
        {
            rot = 270f;
            grid.GetCell(currentIndices).faceDirection = Directions.WEST;
        }
        else
        {
            grid.GetCell(currentIndices).faceDirection = Directions.NORTH;
        }

        //If stairs are going down, we flip the angle, i.e. when moving forward UP stairs, rotation is 0
        //but when going forward DOWN stairs we need to flip them to 180 degrees
        if (diff.y < 0)  
        {
            rot += 180;

            //Flip logged facing direction
            grid.GetCell(currentIndices).faceDirection = Cell.OppositeDirection(grid.GetCell(currentIndices).faceDirection);
        }

        return Quaternion.Euler(new Vector3(0, rot, 0));
    }

    //Return wall rotation needed to place a wall between the two indices
    private Quaternion GetWallRotation(Vector3Int lastIndices, Vector3Int currentIndices)
    {
        Vector3Int diff = currentIndices - lastIndices;

        float rot = 0f; //stairs face forward

        if (diff.z < 0) //stairs face backward
        {
            rot = 180f;
        }
        else if (diff.x > 0) //stairs face right
        {
            rot = 90f;
        }
        else if (diff.x < 0) //stairs face left
        {
            rot = 270f;
        }

        return Quaternion.Euler(new Vector3(0, rot, 0));
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
            if(roomMap.Count > 0)
            {
                foreach (KeyValuePair<Room, List<Room>> pair in roomMap)
                {
                    Gizmos.color = Color.green;
                    foreach (Room room in pair.Value)
                    {
                        Gizmos.DrawLine(pair.Key.center, room.center);
                    }
                }
            }

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
            //        Gizmos.color = Color.green;
            //        Gizmos.DrawLine(currentRoom.cells[0].center, connectedRoom.cells[0].center);
            //    }
            //}
        }
    }
}
