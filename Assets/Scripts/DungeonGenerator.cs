using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] GameObject room;

    [SerializeField] int numRandomRooms = 4;
    [SerializeField] Vector3 maxSize = new Vector3(3, 1, 3);
    [SerializeField] Vector3 minSize = new Vector3(1, 1, 1);

    Grid grid;

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
                grid.CellDimensions.x * Random.Range(0, grid.GridDimensions.x),
                grid.CellDimensions.y * Random.Range(0, grid.GridDimensions.y),
                grid.CellDimensions.z * Random.Range(0, grid.GridDimensions.z)
            ) ;

            //Number of room segments in each direction
            Vector3 randSize = new Vector3(
                Random.Range(minSize.x, maxSize.x),
                Random.Range(minSize.y, maxSize.y),
                Random.Range(minSize.z, maxSize.z)
            );

            Vector3 gridCenter = grid.GetCenter(randPos);
            Vector3 gridIndices = grid.GetGridIndices(randPos);

            //Loop through random size to add rooms and make the one room the size of randSize
            for (int j = 0; j < randSize.x; j++)
            {
                for (int k = 0; k < randSize.y; k++)
                {
                    for (int l = 0; l < randSize.z; l++)
                    {
                        //Check that grid space exists
                        if (j < grid.GridDimensions.x &&
                            k < grid.GridDimensions.y &&
                            l < grid.GridDimensions.z)
                        {
                            //Get index of current space based on first grid index (gridIndices) plus the indices of (j, k, l)
                            Vector3 currentIndices = gridIndices + new Vector3(j, k, l);
                            Vector3 currentCenter = grid.GetCenterByIndices(currentIndices);

                            //If cell is not filled, fill it
                            if (!grid.GetCell(currentIndices).isRoom)
                            {
                                grid.GetCell(currentIndices).isRoom = true;

                                Transform trans = Instantiate(room, currentCenter, Quaternion.identity, null).transform;

                                trans.localScale = grid.CellDimensions;
                            }
                        }
                    }
                }
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
