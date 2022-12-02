using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Room
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

    List<Tetrahedron> tetrahedrons = new List<Tetrahedron>();

    public Vector3 GridDimensions
    {
        get { return new Vector3(cellCountX, cellCountY, cellCountZ); }
    }

    [SerializeField] List<Room> rooms;

    Tetrahedron superTetrahedron;
    //Dictionary<Room, List<Room>> roomMap;

    private void Start()
    {
        grid = GetComponent<Grid>();
        grid.InitGrid(new Vector3(cellWidth, cellHeight, cellDepth), new Vector3(cellCountX, cellCountY, cellCountZ));
        Generate();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Clear();
            grid.InitGrid(new Vector3(cellWidth, cellHeight, cellDepth), new Vector3(cellCountX, cellCountY, cellCountZ));
            Generate();
        }
    }

    void Clear()
    {
        //Empty all arrays and delete all current rooms
        tetrahedrons.Clear();
        rooms.Clear();
        foreach(Transform child in roomParent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    void Generate()
    {
        GenerateRandomRooms();
        CreateConnectedMap();
        DerriveMinimumSpanningTree();
        AddRandomHallways();
        CarveHallways();
    }

    void GenerateRandomRooms()
    {
        for (int i = 0; i < numRandomRooms; i++)
        {
            Vector3 randPos = new Vector3(
                CellDimensions.x * UnityEngine.Random.Range(0, GridDimensions.x),
                CellDimensions.y * UnityEngine.Random.Range(0, GridDimensions.y),
                CellDimensions.z * UnityEngine.Random.Range(0, GridDimensions.z)
            );

            //Number of room segments in each direction
            Vector3 randSize = new Vector3(
                UnityEngine.Random.Range(minSize.x, maxSize.x),
                UnityEngine.Random.Range(minSize.y, maxSize.y),
                UnityEngine.Random.Range(minSize.z, maxSize.z)
            );

            //Ensure at least one floor will be on a different floor, ensures that tetrahedralization will not be degenerate
            if(i == 0)
            {
                randPos.y = 0;
                randSize.y = 1;
            }
            else if(i == 1)
            {
                randPos.y = GridDimensions.y;
                randSize.y = 1;
            }

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
                    //Break early if outside of dimensions
                    if (gridIndices.x + j >= GridDimensions.x)
                    {
                        break;
                    }

                    for (int k = 0; k < randSize.y; k++)
                    {
                        //Break early if outside of dimensions
                        if (gridIndices.y + k >= GridDimensions.y)
                        {
                            break;
                        }

                        for (int l = 0; l < randSize.z; l++)
                        {
                            if (gridIndices.z + l >= GridDimensions.z)
                            {
                                break;
                            }

                            Vector3 currentIndices = gridIndices + new Vector3(j, k, l);
                            //Check that grid space exists
                            //if (currentIndices.x < GridDimensions.x && currentIndices.x >= 0 &&
                            //    currentIndices.y < GridDimensions.y && currentIndices.y >= 0 &&
                            //    currentIndices.z < GridDimensions.z && currentIndices.z >= 0)
                            //{
                            //Get index of current space based on first grid index (gridIndices) plus the indices of (j, k, l)

                            Vector3 currentCenter = grid.GetCenterByIndices(currentIndices);

                            //If cell is not filled, fill it
                            if (grid.GetCell(currentIndices).cellType == CellTypes.NONE)
                            {
                                grid.GetCell(currentIndices).cellType = CellTypes.ROOM;
                                newRoom.cells.Add(grid.GetCell(currentIndices));

                                Transform trans = Instantiate(room, currentCenter, Quaternion.identity, roomParent.transform).transform;

                                trans.localScale = CellDimensions;
                            }
                            //}
                        }
                    }
                }

                newRoom.CalculateCenter();

                rooms.Add(newRoom);
            }
        }

        //Room room1 = new Room();
        //room1.cells.Add(grid.GetCell(grid.GetGridIndices(new Vector3(30, 0, 25))));
        //room1.center = new Vector3(30, 0, 25);
        //Transform trans1 = Instantiate(room, room1.center, Quaternion.identity, null).transform;
        //trans1.localScale = CellDimensions;

        //Room room2 = new Room();
        //room2.cells.Add(grid.GetCell(grid.GetGridIndices(new Vector3(7, 0, 32))));
        //room2.center = new Vector3(7, 0, 32);
        //Transform trans2 = Instantiate(room, room2.center, Quaternion.identity, null).transform;
        //trans2.localScale = CellDimensions;

        //Room room3 = new Room();
        //room3.cells.Add(grid.GetCell(grid.GetGridIndices(new Vector3(7, 0, 35))));
        //room3.center = new Vector3(7, 0, 35);
        //Transform trans3 = Instantiate(room, room3.center, Quaternion.identity, null).transform;
        //trans3.localScale = CellDimensions;

        //Room room4 = new Room();
        //room4.cells.Add(grid.GetCell(grid.GetGridIndices(new Vector3(23, 0, 30))));
        //room4.center = new Vector3(23, 0, 30);
        //Transform trans4 = Instantiate(room, room4.center, Quaternion.identity, null).transform;
        //trans4.localScale = CellDimensions;

        //Room room5 = new Room();
        //room5.cells.Add(grid.GetCell(grid.GetGridIndices(new Vector3(9, 0, 18))));
        //room5.center = new Vector3(9, 0, 18);
        //Transform trans5 = Instantiate(room, room5.center, Quaternion.identity, null).transform;
        //trans5.localScale = CellDimensions;

        //rooms.Add(room1);
        //rooms.Add(room2);
        //rooms.Add(room3);
        //rooms.Add(room4);
        //rooms.Add(room5);
    }

    void CreateConnectedMap()
    {
        tetrahedrons.Clear();
        superTetrahedron = new Tetrahedron(
            new Vector3(-cellWidth, -cellHeight, -cellDepth),
            new Vector3(cellWidth * cellCountX * 4, -cellHeight, -cellDepth),
            new Vector3(-cellWidth, cellHeight * cellCountY * 4, -cellDepth),
            new Vector3(-cellWidth, -cellHeight, cellDepth * cellCountZ * 4)
        );

        //Define super tetrahedron that contains all rooms
        tetrahedrons.Add(superTetrahedron);

        //Debug.DrawLine(superTriangle.pointA, superTriangle.pointB, Color.red, 9999999.9f);
        //Debug.DrawLine(superTriangle.pointA, superTriangle.pointC, Color.red, 9999999.9f);
        //Debug.DrawLine(superTriangle.pointC, superTriangle.pointB, Color.red, 9999999.9f);

        foreach (Room room in rooms)
        {
            //Debug.Log("New Room");
            List<Tetrahedron> containing = new List<Tetrahedron>();

            //Number of occurrences of each triangle (triangles that overlap between tetrahedrons)
            Dictionary<Triangle, int> occurrences = new Dictionary<Triangle, int>();

            //Find which tetrahedrons whose circumspheres contain the center of the room
            //Log how many times each triangle occurs for checking which triangles to remove later on
            foreach (Tetrahedron tet in tetrahedrons)
            {
                if(tet.CircumsphereContainsPoint(room.center))
                {
                    containing.Add(tet);

                    Triangle[] triangles = tet.GetTriangles();

                    if (occurrences.TryGetValue(triangles[0], out int numA))
                    {
                        occurrences[triangles[0]]++;
                    }
                    else
                    {
                        occurrences.Add(triangles[0], 1);
                    }

                    if (occurrences.TryGetValue(triangles[1], out int numB))
                    {
                        occurrences[triangles[1]]++;
                    }
                    else
                    {
                        occurrences.Add(triangles[1], 1);
                    }

                    if (occurrences.TryGetValue(triangles[2], out int numC))
                    {
                        occurrences[triangles[2]]++;
                    }
                    else
                    {
                        occurrences.Add(triangles[2], 1);
                    }

                    if (occurrences.TryGetValue(triangles[3], out int numD))
                    {
                        occurrences[triangles[3]]++;
                    }
                    else
                    {
                        occurrences.Add(triangles[3], 1);
                    }
                }
            }

            //triangles are just three points from a tetrahedrons, each tetrahedron has 4 triangles
            //If an triangle is shared by another tetrahedron, we do not add the edge to shape
            //We log the occurrences of triangles between tetrahedrons when we first add them to "containing"
            List<Triangle> shape = new List<Triangle>();
            foreach (Tetrahedron tet in containing)
            {
                Triangle[] triangles = tet.GetTriangles();

                foreach(Triangle tri in triangles)
                {
                    if (!(occurrences[tri] > 1))
                    {
                        shape.Add(tri);
                    }
                }
            }

            //Remove tetrahedrons in containing from structure of all triangles 
            foreach (Tetrahedron tet in containing)
            {
                tetrahedrons.Remove(tet);
            }

            //Create a new tetrahedron using each valid triangle and the room center
            foreach (Triangle tri in shape)
            {
                tetrahedrons.Add(new Tetrahedron(tri.pointA, tri.pointB, tri.pointC, room.center));
            }
            Debug.Log("New");
            foreach(KeyValuePair<Triangle, int> pair in occurrences)
            {
                Debug.Log(pair.Value + " - " + pair.Key.pointA + " : " + pair.Key.pointB + " : " + pair.Key.pointC);
            }
        }

        int before = tetrahedrons.Count;
        //Delete all tetrahedrons connecting to super tetrahedron
        for (int i = tetrahedrons.Count - 1; i >= 0; i--)
        {
            if (tetrahedrons[i].ContainsVertex(superTetrahedron.pointA) ||
                tetrahedrons[i].ContainsVertex(superTetrahedron.pointB) ||
                tetrahedrons[i].ContainsVertex(superTetrahedron.pointC) ||
                tetrahedrons[i].ContainsVertex(superTetrahedron.pointD))
            {
                //tetrahedrons.Remove(tetrahedrons[i]);
            }
        }

        Debug.Log(before + " -> " + tetrahedrons.Count);

        //roomMap = new Dictionary<Room, List<Room>>();

        //for(int i = 0; i < rooms.Count; i++)
        //{
        //    for(int j = i; j < rooms.Count; j++)
        //    {
        //        //Try to get value, if succeed, add room, if fail, add new entry to dictionary with room
        //        if (roomMap.TryGetValue(rooms[i], out List<Room> roomList))
        //        {
        //            roomList.Add(rooms[j]);
        //        }
        //        else
        //        {
        //            roomMap.Add(rooms[i], new List<Room> { rooms[j] });
        //        }
        //    }
        //}

        //foreach(KeyValuePair<Room, List<Room>> pair in roomMap)
        //{
        //    Debug.Log(roomMap[pair.Key].Count);
        //}
    }

    void DerriveMinimumSpanningTree()
    {

    }

    void AddRandomHallways()
    {

    }

    void CarveHallways()
    {

    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {

            //Tetrahedron tet = new Tetrahedron(Vector3.zero, Vector3.up, Vector3.right, Vector3.forward);
            //tet.DrawGizmos();
            //Gizmos.DrawSphere(tet.circumSphere.center, tet.circumSphere.radius);
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

            foreach (Tetrahedron tet in tetrahedrons)
            {
                //Room currentRoom = pair.Key;
                Gizmos.color = Color.red;
                tet.DrawGizmos();
            }

            List<Tetrahedron> tempTets = new List<Tetrahedron>(tetrahedrons);
            for (int i = tempTets.Count - 1; i >= 0; i--)
            {
                if (tempTets[i].ContainsVertex(superTetrahedron.pointA) ||
                    tempTets[i].ContainsVertex(superTetrahedron.pointB) ||
                    tempTets[i].ContainsVertex(superTetrahedron.pointC) ||
                    tempTets[i].ContainsVertex(superTetrahedron.pointD))
                {
                    tempTets.Remove(tempTets[i]);
                }
            }

            foreach (Tetrahedron tet in tempTets)
            {
                //Room currentRoom = pair.Key;
                Gizmos.color = Color.green;
                tet.DrawGizmos();
            }

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
