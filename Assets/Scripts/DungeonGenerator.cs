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

    Dictionary<Room, List<Room>> roomMap;

    private void Start()
    {
        grid = GetComponent<Grid>();
        grid.InitGrid(new Vector3(cellWidth, cellHeight, cellDepth), new Vector3(cellCountX, cellCountY, cellCountZ));
        Generate();
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
        for(int i = 0; i < numRandomRooms; i++)
        {
            Vector3 randPos = new Vector3(
                CellDimensions.x * UnityEngine.Random.Range(0, GridDimensions.x),
                CellDimensions.y * UnityEngine.Random.Range(0, GridDimensions.y),
                CellDimensions.z * UnityEngine.Random.Range(0, GridDimensions.z)
            ) ;

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

                                    Transform trans = Instantiate(room, currentCenter, Quaternion.identity, null).transform;

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
    }

    void CreateConnectedMap()
    {
        roomMap = new Dictionary<Room, List<Room>>();
        for(int i = 0; i < rooms.Count; i++)
        {
            for(int j = i; j < rooms.Count; j++)
            {
                //Try to get value, if succeed, add room, if fail, add new entry to dictionary with room
                if (roomMap.TryGetValue(rooms[i], out List<Room> roomList))
                {
                    roomList.Add(rooms[j]);
                }
                else
                {
                    roomMap.Add(rooms[i], new List<Room> { rooms[j] });
                }
            }
        }

        foreach(KeyValuePair<Room, List<Room>> pair in roomMap)
        {
            Debug.Log(roomMap[pair.Key].Count);
        }
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
            Triangle tri = new Triangle(rooms[0].center, rooms[1].center, rooms[2].center);
            Circumsphere sphere = DelaunayTriangulation.FindCircumcenter(tri);

            Gizmos.DrawSphere(sphere.center, sphere.radius);
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
