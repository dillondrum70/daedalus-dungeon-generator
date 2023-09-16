/*
        Copyright (c) 2022 - 2023 Dillon Drummond

        Permission is hereby granted, free of charge, to any person obtaining
        a copy of this software and associated documentation files (the
        "Software"), to deal in the Software without restriction, including
        without limitation the rights to use, copy, modify, merge, publish,
        distribute, sublicense, and/or sell copies of the Software, and to
        permit persons to whom the Software is furnished to do so, subject to
        the following conditions:

        The above copyright notice and this permission notice shall be
        included in all copies or substantial portions of the Software.

        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
        EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
        MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
        NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
        LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
        OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
        WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

/*
        Daedalus Dungeon Generator: 3D Dungeon Generator Tool
	    By Dillon W. Drummond

	    DungeonGenerator.cs

	    ********************************************
	    ***      Class that runs generation      ***
	    ********************************************
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// DungeonGenerator Component:
/// Main component of the dungeon generator.  This is where the generator starts and
/// ties together all the other components of this package.
/// Entry Point: Generate()
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Grid))]
public class DungeonGenerator : MonoBehaviour
{
    [Flags]
    public enum DebugMode
    {
        MIN_SPAN_TREE = 1,
        ROOM_MAP = 2,
        TETRAHEDRALIZATION = 4
    }

    //Stores all cells in dungeon.
    Grid grid;
    [SerializeField] Transform dungeonParent;

[Header("Debug Controls")]
    [Tooltip("Display debug logs for algorithm time.")]
    [SerializeField] bool displayAlgorithmTime = true;
    [Tooltip("Sets what is drawn for debug in OnDrawGizmos().")]
    [SerializeField] DebugMode debugMode;

    [Header("Cell Dimensions")]
    [Tooltip("Dimensions of a singular cell in Unity units.\n" +
        "Width, Height, and Depth respectively in X, Y, and Z components.")]
    public Vector3 cellDimensions = new Vector3(5, 5, 5);

[Header("Grid Dimensions")]
    [Tooltip("Dimensions of the entire grid in terms of cells.\n" +
        "Width, Height, and Depth respectively in X, Y, and Z components.")]
    public Vector3 gridDimensions = new Vector3(20, 10, 20);

[Header("Room Parameters")]
    [Tooltip("Prefab of room cell.")]
    [SerializeField] GameObject room;
    [Tooltip("Transform under which all rooms are parented")]
    [SerializeField] Transform roomParent;

    [Tooltip("Number of rooms to randomly place within the dungeon space.")]
    [SerializeField] int numRandomRooms = 10;
    [Tooltip("Maximum size of room in cells.  Room size is randomized between [minSize, maxSize].")]
    [SerializeField] Vector3 maxSize = new Vector3(4, 1, 4);
    [Tooltip("Minimum size of room in cells.   Room size is randomized between [minSize, maxSize].")]
    [SerializeField] Vector3 minSize = new Vector3(2, 1, 2);

[Header("Path Parameters")]
    [Tooltip("Transform parent for path prefabs.")]
    [SerializeField] Transform pathParent;
    [SerializeField] GameObject hallPrefab;
    [SerializeField] GameObject stairsPrefab;
    [SerializeField] GameObject stairSpacePrefab;
    [SerializeField] GameObject wallPrefab;
    [SerializeField] GameObject doorwayPrefab;
    [SerializeField] GameObject pillarPrefab; //We store this prefab to add the pillar to the southeast corner of rooms it is not automatically added to from a wall
    [SerializeField] GameObject archPrefab; //Add between cells of the same type where a wall was not added

[Header("Other")]
    [Tooltip("Percentage chance [0, 1] for lights being placed on a new wall.")]
    [Range(0, 1)]
    [SerializeField] float percentEnableLights = .1f;

    [Tooltip("Adds (<this variable> * number of hallways excluded from MST) hallways to final graph after minimum spanning tree determined.\n" +
        "Adds some freedom of choice for player so there is more than one path between each room.")]
    [SerializeField] float extraHallwaysFactor = .5f;

    //All rooms in dungeon
    List<Room> rooms = new();

    //Tetrahedrons used in Delaunay Tetrahedralization
    List<Tetrahedron> tetrahedrons = new List<Tetrahedron>();

    //The tetrahedron that all dungeons reside within that we use to add new points (rooms) to the tetrahedralization
    Tetrahedron superTetrahedron;

    //Will store adjacency list of edges in tetrahedralization excluding duplicates
    Dictionary<Vector3, List<Edge>> totalEdges = new Dictionary<Vector3, List<Edge>>();

    //Stores minimum spanning tree of total edges
    List<Edge> minSpanTree = new();

    //Map of all rooms and the rooms they are connected to.
    //i.e. 1 : {2, 4} means room 1 is conencted via hallway to room 2 and room 4
    //This is an adjacency list of a directed graph (directed so we don't try to make duplicate hallways.
    Dictionary<Room, List<Room>> roomMap = new Dictionary<Room, List<Room>>();

    public UnityEvent onDungeonClear;
    public UnityEvent onDungeonGenerate;

    public List<Room> GetRooms() { return rooms; }

    private void Start()
    {
        Setup();
    }

    /// <summary>
    /// When destroying the DungeonGenerator, destroy the dungeon as well
    /// </summary>
    private void OnDestroy()
    {
        Clear();
    }

    void Setup()
    {
        //Grid component should be on the same object as the DungeonGenerator component
        if(grid == null)
        {
            grid = GetComponent<Grid>();
        }

        /*
         * If can not find parents, create new ones
         */
        if (dungeonParent == null)
        {
            GameObject dObject = GameObject.Find("Dungeon");
            if (dObject != null)
            {
                dungeonParent = dObject.transform;
            }
            else
            {
                dungeonParent = (new GameObject("Dungeon")).transform;
            }
        }

        if (roomParent == null)
        {
            GameObject rObject = GameObject.Find("Rooms");
            if (rObject != null)
            {
                roomParent = rObject.transform;
            }
            else
            {
                roomParent = (new GameObject("Rooms")).transform;
                roomParent.parent = dungeonParent;
            }
        }

        if (pathParent == null)
        {
            GameObject pObject = GameObject.Find("Paths");
            if (pObject != null)
            {
                pathParent = pObject.transform;
            }
            else
            {
                pathParent = (new GameObject("Paths")).transform;
                pathParent.parent = dungeonParent;
            }
        }
    }

    /// <summary>
    /// Clears all lists, dictionaries, and other members of DungeonGenerator except component references.
    /// Reinitializes grid component.
    /// </summary>
    public void Clear()
    {
        //Run event to call external functions
        onDungeonClear.Invoke();

        //Deal with class members
        tetrahedrons.Clear();
        rooms.Clear();
        totalEdges.Clear();
        minSpanTree.Clear();
        roomMap.Clear();

        //Setup();

        if (roomParent != null)
        {
            for (int i = roomParent.childCount - 1; i >= 0; i--)
            {
                AlwaysDestroy(roomParent.GetChild(i).gameObject);
            }
        }

        if (pathParent != null)
        {
            for (int i = pathParent.childCount - 1; i >= 0; i--)
            {
                AlwaysDestroy(pathParent.GetChild(i).gameObject);
            }
        }

        //Initialize fresh, empty grid
        grid.InitGrid(cellDimensions, gridDimensions);
    }

    void AlwaysDestroy(UnityEngine.Object obj)
    {
#if UNITY_EDITOR
        if (Application.IsPlaying(gameObject))
        {
            Destroy(obj);
        }
        else
        {
            DestroyImmediate(obj);
        }
#else
        Destroy(child.gameObject);
#endif
    }

    /// <summary>
    /// Call GenerateDungeon coroutine
    /// </summary>
    public void Generate()
    {
        Setup();

        StartCoroutine(GenerateDungeon());
    }

    /// <summary>
    /// Generates dungeon
    /// </summary>
    public IEnumerator GenerateDungeon()
    {
        double realtime = Time.realtimeSinceStartupAsDouble; //Store initial time before algorithm starts

        //Clear old dungeon info
        Clear();

        //Randomly place rooms
        GenerateRandomRooms();

        //Delaunay Tetrahedralization to create streamlined map
        CreateConnectedMap(ref totalEdges, ref rooms);

        //Check that the dictionary of all edges in the delaunay tetrahedralization actually has edges
        Vector3 start = Vector3.zero;
        IEnumerator enumerator = totalEdges.Keys.GetEnumerator(); //Get enumerator of keys
        bool success = enumerator.MoveNext();   //Move to first key
        if (success) //Check that key exists
        {
            start = (Vector3)enumerator.Current;    //Get key
        }
        else
        {
            Debug.LogError("No keys in edge map from tetrahedralization.  Can not execute MST.\n" +
                "Try checking that rooms are actually getting created and that tetrahedralization is functioning.");
        }

        //Create MST
        minSpanTree = MinimumSpanningTree.DerriveMST(out List<Edge> excluded, start, totalEdges);

        List<Edge> expandedTree = new();
        //Add hallways randomly from the list of hallways not in the minimum spanning tree
        AddRandomHallways(minSpanTree, ref excluded, out expandedTree);

        //Turn the list of edges into an adjacency list of rooms
        ConvertEdgesBackToRooms(ref expandedTree, ref rooms, out roomMap);

        //Create hallways between rooms using A*
        //This is the heaviest function computationally.  If you experience performance issues, this is likely the culprit.
        CarveHallways(ref roomMap);

        //Place walls in between rooms and hallways (keeps hallways and rooms from having 
        PlaceWalls();

        //Use event to call external functions after dungeon is generated
        onDungeonGenerate.Invoke();

        //Display total time for algorithm
        if(displayAlgorithmTime)
        {
            //On an Intel 9th Gen i9, this algorithm takes ~1-5 seconds to run for reference.
            Debug.Log("Algorithm Time: " + (float)(Time.realtimeSinceStartupAsDouble - realtime) + " seconds");
        }

        yield return null;
    }

    /// <summary>
    /// Generate random rooms of random sizes on the grid.
    /// </summary>
    void GenerateRandomRooms()
    {
        //TODO: add noise for pseudo-randomness, we could sample perlin noise at the worldspace position of each cell (or average perlin noise in each cell from
        //a set of candidate points) and, if it is above some threshold, we make it a room.

        //Loop through for each room
        for (int i = 0; i < numRandomRooms; i++)
        {
            Vector3 randPos = Vector3.zero;

            //Number of room segments in each direction
            Vector3 randSize = new Vector3(
                UnityEngine.Random.Range(minSize.x, maxSize.x),
                UnityEngine.Random.Range(minSize.y, maxSize.y),
                UnityEngine.Random.Range(minSize.z, maxSize.z)
            );

            //TODO: Create an elegant solution for degenerate tetrahedrals and triangles of rooms
            //I have this commented out because there are too many variables that go into preventing degenerate tetrahedrals.
            //If the 4 given points lie on the same plane (the plane can have any orientation) then tetrahedralization on those points fails
            //It may be simpler to just remove rooms that have a degenerate tetrahedral
            //The likely solution is to go back into the git history and add back the 2d triangulation oriented to the plane
            //in these cases, but that doesn't fix degenerate triangles (all rooms in a straight line).
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
                cellDimensions.x * UnityEngine.Random.Range(0, gridDimensions.x),
                cellDimensions.y * UnityEngine.Random.Range(0, gridDimensions.y),
                cellDimensions.z * UnityEngine.Random.Range(0, gridDimensions.z)
            );
            //}

            Vector3 gridCenter = grid.GetCenter(randPos);
            Vector3Int gridIndices = grid.GetGridIndices(randPos);

            //Make sure cell exists in the grid before defining one for the map
            if (gridIndices.x < gridDimensions.x && gridIndices.x >= 0 &&
               gridIndices.y < gridDimensions.y && gridIndices.y >= 0 &&
               gridIndices.z < gridDimensions.z && gridIndices.z >= 0)
            {
                Room newRoom = new Room();

                //Loop through random size to add cells and make one room the size of randSize in cells
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

                            //Get cell center
                            Vector3 currentCenter = grid.GetCenterByIndices(currentIndices);

                            //Set cell type to ROOM and add to room
                            grid.GetCell(currentIndices).cellType = CellTypes.ROOM;
                            newRoom.cells.Add(grid.GetCell(currentIndices));

                            //Create room prefab in the cell and set it's dimensions
                            Transform trans = Instantiate(room, currentCenter, Quaternion.identity, roomParent).transform;

                            trans.localScale = cellDimensions;
                        }
                    }
                }

                //Perform first time calculation of room center which is stored in the class
                newRoom.CalculateCenter();

                //Add to rooms
                rooms.Add(newRoom);
            }
        }
    }

    /// <summary>
    /// Performs Delaunay Tetrahedralization on the list of rooms
    /// </summary>
    void CreateConnectedMap(ref Dictionary<Vector3, List<Edge>> edgeMap, ref List<Room> roomList)
    {
        //Define a super tetrahedron that is guaranteed to encapsulate the entire grid
        superTetrahedron = new Tetrahedron(
            new Vector3(-cellDimensions.x * gridDimensions.x, -cellDimensions.y * gridDimensions.y, -cellDimensions.z * gridDimensions.z) * 2,
            new Vector3(cellDimensions.x * gridDimensions.x * 4, -cellDimensions.y, -cellDimensions.z),
            new Vector3(-cellDimensions.x, cellDimensions.y * gridDimensions.y * 4, -cellDimensions.z),
            new Vector3(-cellDimensions.x, -cellDimensions.y, cellDimensions.z * gridDimensions.z * 4)
        );

        //Define super tetrahedron that contains all rooms
        //tetrahedrons.Add(superTetrahedron);

        //Create list of points from the rooms
        List<Vector3> pointList = new List<Vector3>();
        foreach (Room room in roomList)
        {
            pointList.Add(room.center);
        }

        //Perform tetrahedralization
        tetrahedrons = DelaunayTetrahedralization.Tetrahedralize(superTetrahedron, pointList);

        //Count up each edge in tetrahedralization once and add a second with the points reversed so we have an adjacency list of an
        //undirected graph.  This means looping through every edge in every tetrahedron which has a lot of duplicates.
        //TODO: Optimize this
        foreach (Tetrahedron tet in tetrahedrons)
        {
            Edge[] edges = tet.GetEdges();

            foreach (Edge e in edges)
            {
                List<Edge> list;

                //Add the edge to the dictionary if it isn't in there already
                if (edgeMap.TryGetValue(e.pointA, out list))
                {
                    if (!list.Contains(e))
                    {
                        list.Add(e);
                    }
                    //Else do nothing.  Already has edge
                }
                else
                {
                    //If totalEdges does not have a value at e.pointA, initialize the value and add the new KeyValuePair
                    List<Edge> newList = new();
                    newList.Add(e);
                    edgeMap.Add(e.pointA, newList);
                }

                //Create a duplicate of the edge with the points reversed so we have an undirected graph
                if (edgeMap.TryGetValue(e.pointB, out list))
                {
                    if (!list.Contains(e))
                    {
                        list.Add(new Edge(e.pointB, e.pointA));
                    }
                    //Else do nothing, already has edge
                }
                else
                {
                    //If totalEdges does nto have an entry for e.pointB, create a new dictionary entry
                    List<Edge> newList = new();
                    newList.Add(new Edge(e.pointB, e.pointA));
                    edgeMap.Add(e.pointB, newList);
                }
            }
        }
    }

    /// <summary>
    /// Add back random hallways after the Minimum Spanning Tree is derived from tetrahedralization
    /// </summary>
    /// <param name="mst">The minimum spanning tree to add edges back to</param>
    /// <param name="excluded">List of edges not in minSpanTree</param>
    /// <param name="expandedTree">Out parameter of minSpanTree with added hallways</param>
    void AddRandomHallways(List<Edge> mst, ref List<Edge> excluded, out List<Edge> expandedTree)
    {
        //Add random number of edges from excluded to minSpanTree
        int numHalls = (int)(extraHallwaysFactor * excluded.Count);
        expandedTree = new(mst);

        for (int i = 0; i < numHalls; i++)
        {
            int hallIndex = UnityEngine.Random.Range(0, excluded.Count - 1);

            //Add edge to minSpanTree and remove it from excluded, this ensures duplicate edges are not added since
            //the graph exclided + minSpanTree should not have any duplicates
            expandedTree.Add(excluded[i]);
            excluded.RemoveAt(i);
        }
    }

    /// <summary>
    /// Turns the list of edges into an adjacency list (dictionary) of rooms.
    /// TODO: Optimize this
    /// </summary>
    /// <param name="finalMap">Finalized edge list that needs to be turned into a room adjacency list</param>
    /// <param name="roomList">List of rooms</param>
    /// <param name="adjacencyList">Resulting list from converting edges back into rooms</param>
    void ConvertEdgesBackToRooms(ref List<Edge> finalMap, ref List<Room> roomList, out Dictionary<Room, List<Room>> adjacencyList)
    {
        adjacencyList = new();

        //Match rooms to edges to get room map
        foreach (Edge e in finalMap)
        {
            Room room1 = null;
            Room room2 = null;

            //Go through each room and check if it matched pointA or pointB
            foreach (Room room in roomList)
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
            
            if (room1 != null && room2 != null) //Sanity check, rooms should exist
            {
                //Add connected room to the value list under room1 key
                if (adjacencyList.TryGetValue(room1, out List<Room> adjacentRooms)) 
                {
                    adjacentRooms.Add(room2);
                }
                else
                {
                    //Adds new dictionary entry for room1
                    List<Room> newRooms = new List<Room>();
                    newRooms.Add(room2);
                    adjacencyList.Add(room1, newRooms);
                }
            }
        }
    }

    /// <summary>
    /// Creates hallways and staircases between rooms, spawns in objects.
    /// TODO: Optimize this
    /// </summary>
    void CarveHallways(ref Dictionary<Room, List<Room>> adjacencyList)
    {
        foreach (KeyValuePair<Room, List<Room>> pair in adjacencyList)
        {
            foreach (Room room in pair.Value)
            {
                float realtime = Time.realtimeSinceStartup;
                
                //Cell closest to the goal room
                Vector3Int startIndices = pair.Key.ClosestValidStartCell(grid.GetGridIndices(room.center), grid);

                if (grid.IsValidCell(startIndices)) //Sanity check
                {
                    //Run A* algorithm
                    Stack<AStarNode> path = AStar.Run(startIndices, grid.GetGridIndices(room.center), room, grid);

                    //Set up currentPathParent
                    Transform currentPathParent = new GameObject().transform;
                    currentPathParent.name = "Path";
                    currentPathParent.parent = pathParent;

                    //path might return null if A* failed
                    if (path == null)
                    {
                        Debug.LogError("A-Star Path Failed");
                        //Log the time this step took
                        if (displayAlgorithmTime)
                        {
                            Debug.Log("Path Time: " + (Time.realtimeSinceStartup - realtime));
                        }
                        continue;
                    }

                    //Store the last Y value so we know when we've added a stairwell
                    Vector3Int lastIndices = startIndices;
                    foreach (AStarNode node in path)
                    {
                        //Get center position of unit
                        Vector3 pos = new Vector3(node.indices.x * cellDimensions.x, node.indices.y * cellDimensions.y, node.indices.z * cellDimensions.z);

                        if (node.indices.y < lastIndices.y) //Stairwell down
                        {
                            //Stair cell
                            grid.GetCell(node.indices).cellType = CellTypes.STAIRS; //Mark next space as stairs

                            //Cache rotation so the space above the stairs has the same rotation and its walls appear on the correct sides
                            Quaternion stairRotation = GetStairRotation(lastIndices, node.indices);

                            Transform trans = Instantiate(stairsPrefab, pos, stairRotation, currentPathParent).transform; //Spawn stairwell
                            trans.localScale = cellDimensions;  //Scale the unit to fit the grid cell

                            //Stairspace cell
                            pos += new Vector3(0, cellDimensions.y, 0); //Update position to be the cell above the current node
                            Vector3Int spaceIndex = new Vector3Int(node.indices.x, node.indices.y + 1, node.indices.z);

                            //Mark space above next space as stairspace so it remains empty and their is space to go down the stairs
                            grid.GetCell(spaceIndex).cellType = CellTypes.STAIRSPACE;
                            grid.GetCell(spaceIndex).faceDirection = Cell.OppositeDirection(grid.GetCell(node.indices).faceDirection);
                            trans = Instantiate(stairSpacePrefab, pos, stairRotation, currentPathParent).transform; //Spawn stairspace
                            trans.localScale = cellDimensions; //Scale the unit to fit the grid cell
                        }
                        else if (node.indices.y > lastIndices.y) //Stairwell up
                        {
                            //Stair cell
                            Vector3Int stairIndex = new Vector3Int(node.indices.x, node.indices.y - 1, node.indices.z);

                            Quaternion stairRotation = GetStairRotation(lastIndices, stairIndex);

                            grid.GetCell(stairIndex).cellType = CellTypes.STAIRS;  //Mark as stairs
                            Transform trans = Instantiate(stairsPrefab, pos - new Vector3(0, cellDimensions.y, 0), stairRotation, currentPathParent).transform; //Spawn stair, Update position to be the cell below the current node
                            trans.localScale = cellDimensions; //Scale the unit to fit the grid cell

                            //Stairspace cell, cells up diagonally must be an empty space and the cell below them holds the actual stairs
                            grid.GetCell(node.indices).cellType = CellTypes.STAIRSPACE; //Mark next space as stairs
                            grid.GetCell(node.indices).faceDirection = Cell.OppositeDirection(grid.GetCell(stairIndex).faceDirection);

                            trans = Instantiate(stairSpacePrefab, pos, stairRotation, currentPathParent).transform; //Spawn stair space
                            trans.localScale = cellDimensions;  //Scale the unit to fit the grid cell
                        }
                        else //Hallway
                        {
                            //Mark grid cell as hallway
                            grid.GetCell(node.indices).cellType = CellTypes.HALLWAY;

                            //Spawn hallway
                            Transform trans = Instantiate(hallPrefab, pos, Quaternion.identity, currentPathParent).transform;

                            //Scale the unit to fit the grid cell
                            trans.localScale = cellDimensions;
                        }

                        //Update last y with this node's y value
                        lastIndices = node.indices;
                    }
                }
                else
                {
                    Debug.LogError("Valid starting cell could not be found");
                }

                //Log the time this step took
                if (displayAlgorithmTime)
                {
                    Debug.Log("Path Time: " + (Time.realtimeSinceStartup - realtime));
                }
            }
        }
    }

    /// <summary>
    /// Place walls on rooms and hallways.  Separating this step prevents hallways from blocking off other paths and 
    /// creates some emergent behavior between adjacent hallways which creates some open areas, double staircases, etc.
    /// </summary>
    private void PlaceWalls()
    {
        //Do a pass of all cells and determine what types of walls to add 
        for (int x = 0; x < gridDimensions.x; x++)
        {
            for (int y = 0; y < gridDimensions.y; y++)
            {
                for (int z = 0; z < gridDimensions.z; z++)
                {
                    Vector3Int currentIndex = new Vector3Int(x, y, z);
                    //Check this node's cell type to determine what walls should be added
                    CellTypes type = grid.GetCell(currentIndex).cellType;

                    PlacePillar(currentIndex);

                    switch(type)
                    {
                        case CellTypes.ROOM:
                            //Only check north and east, the previous node will have handled the checks for south and west
                            RoomWall(currentIndex, currentIndex + AStar.constNorth);
                            RoomWall(currentIndex, currentIndex + AStar.constEast);

                            //If on grid bounds, there is no previous node to do this check, needs to be done by this node
                            if(currentIndex.z == 0) //Southern edge of grid
                            {
                                RoomWall(currentIndex, currentIndex + AStar.constSouth);
                            }
                            if(currentIndex.x == 0) //Eastern edge of grid
                            {
                                RoomWall(currentIndex, currentIndex + AStar.constWest);
                            }
                            //PlacePillar(currentIndex);
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
                            //PlacePillar(currentIndex);
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
                            //PlacePillar(currentIndex);
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
                            //PlacePillar(currentIndex);
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

    /// <summary>
    /// Check if pillars need to be placed at current index
    /// </summary>
    /// <param name="currentIndex">Index of cell in which to place pillars</param>
    private void PlacePillar(Vector3Int currentIndex)
    {
        //Transform trans1 = Instantiate(pillarPrefab, grid.GetCenterByIndices(currentIndex), Quaternion.identity, roomParent).transform;
        //trans1.localScale = cellDimensions;

        CellTypes type = grid.GetCell(currentIndex).cellType;

        //Place North East corner
        if (type != CellTypes.NONE //Cell is filled
            || (grid.IsValidCell(currentIndex + AStar.constNorth + AStar.constEast) && !grid.IsCellEmpty(currentIndex + AStar.constNorth + AStar.constEast)) //Corner is valid and filled
            || (grid.IsValidCell(currentIndex + AStar.constEast) && !grid.IsCellEmpty(currentIndex + AStar.constEast)) //East cell is valid and filled
            || (grid.IsValidCell(currentIndex + AStar.constNorth) && !grid.IsCellEmpty(currentIndex + AStar.constNorth))) //North cell is valid and filled
        {
            Transform trans = Instantiate(pillarPrefab, grid.GetCenterByIndices(currentIndex), Quaternion.Euler(new Vector3(0, 0, 0)), roomParent).transform;
            trans.localScale = cellDimensions;
        }

        //Place South West corner
        if ((type != CellTypes.NONE && (!grid.IsValidCell(currentIndex + AStar.constWest) || !grid.IsValidCell(currentIndex + AStar.constSouth) || !grid.IsValidCell(currentIndex + AStar.constWest + AStar.constSouth))) //Cell is filled and south or west or both are not valid (edge of map)
            || 
            (type == CellTypes.NONE //If cell is empty
            && ((!grid.IsValidCell(currentIndex + AStar.constSouth) && (grid.IsValidCell(currentIndex + AStar.constWest) && !grid.IsCellEmpty(currentIndex + AStar.constWest))) //South is invalid (on edge) and West is valid and filled
            || (!grid.IsValidCell(currentIndex + AStar.constWest) && (grid.IsValidCell(currentIndex + AStar.constSouth) && !grid.IsCellEmpty(currentIndex + AStar.constSouth)))))) //West is invalid (on edge) and South is valid and filled 
        {
            Transform trans = Instantiate(pillarPrefab, grid.GetCenterByIndices(currentIndex), Quaternion.Euler(new Vector3(0, 180, 0)), roomParent).transform;
            trans.localScale = cellDimensions;
        }

        //Place south east corner
        if (type != CellTypes.NONE && !grid.IsValidCell(currentIndex + AStar.constSouth + AStar.constEast) && !grid.IsValidCell(currentIndex + AStar.constSouth) && !grid.IsValidCell(currentIndex + AStar.constEast))
        {
            Transform trans = Instantiate(pillarPrefab, grid.GetCenterByIndices(currentIndex), Quaternion.Euler(new Vector3(0, 90, 0)), roomParent).transform;
            trans.localScale = cellDimensions;
        }

        //Place north west corner

        if (type != CellTypes.NONE && !grid.IsValidCell(currentIndex + AStar.constNorth + AStar.constWest) && !grid.IsValidCell(currentIndex + AStar.constNorth) && !grid.IsValidCell(currentIndex + AStar.constWest))
        {
            Transform trans = Instantiate(pillarPrefab, grid.GetCenterByIndices(currentIndex), Quaternion.Euler(new Vector3(0, 270, 0)), roomParent).transform;
            trans.localScale = cellDimensions;
        }
    }

    /// <summary>
    /// Check to place walls and doorways on specific wall between given indices for a room.
    /// </summary>
    /// <param name="currentIndex">Index of current cell</param>
    /// <param name="adjacentIndex">Index of cell adjacent to current cell</param>
    private void RoomWall(Vector3Int currentIndex, Vector3Int adjacentIndex)
    {
        if (grid.IsValidCell(adjacentIndex)) //Check if adjacent cell exists
        {
            Cell adjacentCell = grid.GetCell(adjacentIndex);

            GameObject spawnObject = null;

            //Place different prefabs for wall based on adjecent cell type
            //Spawns doorway if leaving or entering a room
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
                    spawnObject = archPrefab;

                    break;
                //No walls between rooms
                default:
                    
                    break;
            }

            //If the object set to be spawned is not null, instantiate the object
            if(spawnObject != null)
            {
                Transform trans = Instantiate(spawnObject, grid.GetCenterByIndices(currentIndex), GetWallRotation(currentIndex, adjacentIndex), roomParent).transform;
                trans.localScale = cellDimensions;

                //Random check to enable lights
                if (UnityEngine.Random.Range(0f, 1f) < percentEnableLights)
                {
                    StartLight light = trans.gameObject.GetComponent<StartLight>();
                    if(light != null ) { light.EnableLight(); }
                }
            }
        }
        else //If next index is not valid, it is empty and we need a wall
        {
            Transform trans = Instantiate(wallPrefab, grid.GetCenterByIndices(currentIndex), GetWallRotation(currentIndex, adjacentIndex), roomParent).transform;
            trans.localScale = cellDimensions;

            //Check to enable light
            if (UnityEngine.Random.Range(0f, 1f) < percentEnableLights)
            {
                StartLight light = trans.gameObject.GetComponent<StartLight>();
                if (light != null) { light.EnableLight(); }
            }
        }
    }

    /// <summary>
    /// Place walls and doorways on a hallway between given indices.
    /// </summary>
    /// <param name="currentIndex">Index of current cell</param>
    /// <param name="adjacentIndex">Index of cell adjacent to current cell</param>
    private void HallWall(Vector3Int currentIndex, Vector3Int adjacentIndex)
    {
        if (grid.IsValidCell(adjacentIndex)) //Check if on border of grid
        {
            Cell adjacentCell = grid.GetCell(adjacentIndex);

            GameObject spawnObject = null;

            //Determine type of prefab to place based on adjacent cell type
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

            //Spawn object if one needs to be spawned
            if (spawnObject != null)
            {
                Transform trans = Instantiate(spawnObject, grid.GetCenterByIndices(currentIndex), GetWallRotation(currentIndex, adjacentIndex), roomParent).transform;
                trans.localScale = cellDimensions;

                if (UnityEngine.Random.Range(0f, 1f) < percentEnableLights)
                {
                    StartLight light = trans.gameObject.GetComponent<StartLight>();
                    if (light != null) { light.EnableLight(); }
                }
            }
        }
        else //If next index is not valid, it is empty and we need a wall
        {
            Transform trans = Instantiate(wallPrefab, grid.GetCenterByIndices(currentIndex), GetWallRotation(currentIndex, adjacentIndex), roomParent).transform;
            trans.localScale = cellDimensions;

            if (UnityEngine.Random.Range(0f, 1f) < percentEnableLights)
            {
                StartLight light = trans.gameObject.GetComponent<StartLight>();
                if (light != null) { light.EnableLight(); }
            }
        }
    }

    /// <summary>
    /// Handle wall placement for the cell above stairwells
    /// </summary>
    /// <param name="currentIndex">Index of current cell (stair space)</param>
    /// <param name="adjacentIndex">Index of adjacent cell</param>
    private void StairSpaceWall(Vector3Int currentIndex, Vector3Int adjacentIndex)
    {
        if (grid.IsValidCell(adjacentIndex)) //Check if adjacent cell exists
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

            //Spawn object if one needs to be spawned
            if (spawnObject != null)
            {
                Transform trans = Instantiate(spawnObject, grid.GetCenterByIndices(currentIndex), GetWallRotation(currentIndex, adjacentIndex), roomParent).transform;
                trans.localScale = cellDimensions;

                if (UnityEngine.Random.Range(0f, 1f) < percentEnableLights)
                {
                    StartLight light = trans.gameObject.GetComponent<StartLight>();
                    if (light != null) { light.EnableLight(); }
                }
            }
        }
        else //If next index is not valid, it is empty and we need a wall
        {
            Transform trans = Instantiate(wallPrefab, grid.GetCenterByIndices(currentIndex), GetWallRotation(currentIndex, adjacentIndex), roomParent).transform;
            trans.localScale = cellDimensions;

            if (UnityEngine.Random.Range(0f, 1f) < percentEnableLights)
            {
                StartLight light = trans.gameObject.GetComponent<StartLight>();
                if (light != null) { light.EnableLight(); }
            }
        }
    }

    /// <summary>
    /// Check what wall to place between a stair cell and adjacent cells
    /// </summary>
    /// <param name="currentIndex">Index of current cell (stair cell)</param>
    /// <param name="adjacentIndex">Index of adjacent cell</param>
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
                Transform trans = Instantiate(spawnObject, grid.GetCenterByIndices(currentIndex), GetWallRotation(currentIndex, adjacentIndex), roomParent).transform;
                trans.localScale = cellDimensions;

                //if (UnityEngine.Random.Range(0f, 1f) < percentEnableLights)
                //{
                //    StartLight light = trans.gameObject.GetComponent<StartLight>();
                //    if (light != null) { light.EnableLight(); }
                //}
            }
        }
        else //If next index is not valid, it is empty and we need a wall
        {
            Transform trans = Instantiate(wallPrefab, grid.GetCenterByIndices(currentIndex), GetWallRotation(currentIndex, adjacentIndex), roomParent).transform;
            trans.localScale = cellDimensions;
        }
    }

    /// <summary>
    /// Check what wall to place for an empty cell and adjacent cell
    /// </summary>
    /// <param name="currentIndex">Index of current empty cell</param>
    /// <param name="adjacentIndex">Index of adjacent cell</param>
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
                case CellTypes.STAIRSPACE:
                    spawnObject = wallPrefab;
                    break;

                case CellTypes.STAIRS:
                    spawnObject = wallPrefab;
                    break;

                //Do nothing
                case CellTypes.NONE:
                default:
                    break;
            }

            if (spawnObject != null)
            {
                Transform trans = Instantiate(spawnObject, grid.GetCenterByIndices(currentIndex), GetWallRotation(currentIndex, adjacentIndex), roomParent).transform;
                trans.localScale = cellDimensions;

                if (adjacentCell.cellType != CellTypes.STAIRS 
                    && UnityEngine.Random.Range(0f, 1f) < percentEnableLights)
                {
                    StartLight light = trans.gameObject.GetComponent<StartLight>();
                    if (light != null) 
                    { 
                        light.EnableLight();
                        light?.GetLight().transform.Rotate(new Vector3(0, 180f, 0));
                    }                    
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

    /// <summary>
    /// Return wall rotation needed to place a wall between the two indices
    /// </summary>
    /// <param name="lastIndices">Last 3D cell index</param>
    /// <param name="currentIndices">Current 3D cell index</param>
    /// <returns></returns>
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

    /// <summary>
    /// Debug gizmos, currently just shows final edges in room graph
    /// </summary>
    private void OnDrawGizmos()
    {
        //TETRAHEDRALIZATION includes ROOM_MAP which includes MIN_SPAN_TREE so they are drawn in reverse
        //This displays all edges in Delaunay Tetrahedralization including excluded edges that do not have hallways
        if((debugMode & DebugMode.TETRAHEDRALIZATION) == DebugMode.TETRAHEDRALIZATION)
        {
            foreach (KeyValuePair<Vector3, List<Edge>> pair in totalEdges)
            {
                Gizmos.color = Color.red;
                foreach (Edge edge in pair.Value)
                {
                    Gizmos.DrawLine(edge.pointA, edge.pointB);
                }
            }
        }


        //ROOM_MAP draws finalized map of lines between rooms that represent hallways
        if ((debugMode & DebugMode.ROOM_MAP) == DebugMode.ROOM_MAP)
        {
            if (roomMap != null && roomMap.Count > 0)
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
        }

        //MIN_SPAN_TREE draws the minimum spanning tree on top of all other debug lines
        if ((debugMode & DebugMode.MIN_SPAN_TREE) == DebugMode.MIN_SPAN_TREE)
        {
            foreach (Edge edge in minSpanTree)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(edge.pointA, edge.pointB);
            }
        }
    }
}
