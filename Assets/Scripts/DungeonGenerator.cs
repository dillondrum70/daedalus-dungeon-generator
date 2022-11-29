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
}

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] GameObject room;

    [SerializeField] int numRandomRooms = 4;
    [SerializeField] Vector3 maxSize = new Vector3(3, 1, 3);
    [SerializeField] Vector3 minSize = new Vector3(1, 1, 1);

    Grid grid;

    [SerializeField]
    public List<Room> rooms;

    private void Start()
    {
        grid = GetComponent<Grid>();
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
                grid.CellDimensions.x * UnityEngine.Random.Range(0, grid.GridDimensions.x),
                grid.CellDimensions.y * UnityEngine.Random.Range(0, grid.GridDimensions.y),
                grid.CellDimensions.z * UnityEngine.Random.Range(0, grid.GridDimensions.z)
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
            if (gridIndices.x < grid.GridDimensions.x && gridIndices.x >= 0 &&
               gridIndices.y < grid.GridDimensions.y && gridIndices.y >= 0 &&
               gridIndices.z < grid.GridDimensions.z && gridIndices.z >= 0)
            {
                Room newRoom = new Room();

                //Loop through random size to add rooms and make the one room the size of randSize
                for (int j = 0; j < randSize.x; j++)
                {
                    for (int k = 0; k < randSize.y; k++)
                    {
                        for (int l = 0; l < randSize.z; l++)
                        {
                            Vector3 currentIndices = gridIndices + new Vector3(j, k, l);
                            //Check that grid space exists
                            if (currentIndices.x < grid.GridDimensions.x && currentIndices.x >= 0 &&
                                currentIndices.y < grid.GridDimensions.y && currentIndices.y >= 0 &&
                                currentIndices.z < grid.GridDimensions.z && currentIndices.z >= 0)
                            {
                                //Get index of current space based on first grid index (gridIndices) plus the indices of (j, k, l)

                                Vector3 currentCenter = grid.GetCenterByIndices(currentIndices);

                                //If cell is not filled, fill it
                                if (!grid.GetCell(currentIndices).isRoom)
                                {
                                    grid.GetCell(currentIndices).isRoom = true;
                                    newRoom.cells.Add(grid.GetCell(currentIndices));

                                    Transform trans = Instantiate(room, currentCenter, Quaternion.identity, null).transform;

                                    trans.localScale = grid.CellDimensions;
                                }
                            }
                        }
                    }
                }

                rooms.Add(newRoom);
            }
        }
    }

    void CreateConnectedMap()
    {

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
}
