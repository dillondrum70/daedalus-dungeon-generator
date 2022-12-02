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

    List<Triangle> triangles = new List<Triangle>();

    public Vector3 GridDimensions
    {
        get { return new Vector3(cellCountX, cellCountY, cellCountZ); }
    }

    [SerializeField] List<Room> rooms;

    //Dictionary<Room, List<Room>> roomMap;

    private void Start()
    {
        Edge a = new Edge(new Vector3(100.0f, 50.0f, 50.0f), new Vector3(50.0f, 50.0f, 100.0f));
        Edge b = new Edge(new Vector3(50.0f, 50.0f, 100.0f), new Vector3(100.0f, 50.0f, 50.0f));
        Debug.Log(a.GetHashCode());
        Debug.Log(b.GetHashCode());
        Debug.Log(a.Equals(b));
        Debug.Log(b.Equals(a));
        Dictionary<Edge, int> temp = new();
        
        temp.Add(a, 1);
        if (temp.TryGetValue(b, out int numB))
        {
            temp[b]++;
        }
        else
        {
            temp.Add(b, 1);
        }
        Debug.Log(temp[a] + "  " + temp[b]);

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
        triangles.Clear();
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
        triangles.Clear();
        Triangle superTriangle = new Triangle(
            new Vector3(-cellWidth * cellCountX, -cellHeight * cellCountY, -cellDepth * cellCountZ),
            new Vector3(cellWidth * cellCountX * 2, -CellDimensions.y, -CellDimensions.z),
            new Vector3(-CellDimensions.x, cellHeight * cellCountY * 2, cellDepth * cellCountZ * 2)
        );

        //Define super triangle that contains all rooms
        triangles.Add(superTriangle);

        //Debug.DrawLine(superTriangle.pointA, superTriangle.pointB, Color.red, 9999999.9f);
        //Debug.DrawLine(superTriangle.pointA, superTriangle.pointC, Color.red, 9999999.9f);
        //Debug.DrawLine(superTriangle.pointC, superTriangle.pointB, Color.red, 9999999.9f);

        foreach (Room room in rooms)
        {
            //Debug.Log("New Room");
            List<Triangle> containing = new List<Triangle>();

            //Number of occurrences of each point (points that overlap between triangles)
            Dictionary<Edge, int> occurrences = new Dictionary<Edge, int>();

            //Find which triangles whose circumspheres contain the center of the room
            //Log how many times each edge occurs for checking which edges to remove later on
            foreach(Triangle tri in triangles)
            {
                if(tri.CircumsphereContainsPoint(room.center))
                {
                    containing.Add(tri);

                    Edge[] edges = tri.GetEdges();

                    if (occurrences.TryGetValue(edges[0], out int numA))
                    {
                        occurrences[edges[0]]++;
                    }
                    else
                    {
                        occurrences.Add(edges[0], 1);
                    }

                    if (occurrences.TryGetValue(edges[1], out int numB))
                    {
                        occurrences[edges[1]]++;
                    }
                    else
                    {
                        occurrences.Add(edges[1], 1);
                    }

                    if (occurrences.TryGetValue(edges[2], out int numC))
                    {
                        occurrences[edges[2]]++;
                    }
                    else
                    {
                        occurrences.Add(edges[2], 1);
                    }
                }
            }
            
            //Edges are just two points from a triangle, each triangle has 3 edges
            //If an edge is shared by another triangle, we do not add the edge to polygon
            //We log the occurrences of edges between triangles when we first add them to "containing"
            List<Edge> polygon = new List<Edge>();
            foreach (Triangle tri in containing)
            {
                Edge[] edges = tri.GetEdges();

                foreach(Edge edge in edges)
                {
                    if (!(occurrences[edge] > 1))
                    {
                        polygon.Add(edge);
                    }
                }
            }

            //Remove triangles in containing from structure of all triangles 
            foreach (Triangle tri in containing)
            {
                triangles.Remove(tri);
            }

            //Create a new triangle using each valid edge and the room center
            foreach (Edge edge in polygon)
            {
                triangles.Add(new Triangle(edge.pointA, edge.pointB, room.center));
            }
            Debug.Log("New");
            foreach(KeyValuePair<Edge, int> pair in occurrences)
            {
                Debug.Log(pair.Value + " - " + pair.Key.pointA + " : " + pair.Key.pointB);
            }
        }

        //Delete all triangles connecting to super triangle
        for(int i = triangles.Count - 1; i >= 0; i--)
        {
            if (triangles[i].pointA == superTriangle.pointA ||
               triangles[i].pointB == superTriangle.pointA ||
               triangles[i].pointC == superTriangle.pointA ||
               triangles[i].pointA == superTriangle.pointB ||
               triangles[i].pointB == superTriangle.pointB ||
               triangles[i].pointC == superTriangle.pointB ||
               triangles[i].pointA == superTriangle.pointC ||
               triangles[i].pointB == superTriangle.pointC ||
               triangles[i].pointC == superTriangle.pointC)
            {
                triangles.Remove(triangles[i]);
            }
        }

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
        if(Application.isPlaying)
        {
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

            foreach (Triangle tri in triangles)
            {
                //Room currentRoom = pair.Key;
                Gizmos.color = Color.red;
                Gizmos.DrawLine(tri.pointA, tri.pointB);
                Gizmos.DrawLine(tri.pointA, tri.pointC);
                Gizmos.DrawLine(tri.pointC, tri.pointB);
            }

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
