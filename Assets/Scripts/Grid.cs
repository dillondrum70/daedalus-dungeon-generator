using System.Linq;
using UnityEngine;

public enum CellTypes
{
    NONE = 0,
    ROOM,
    HALLWAY,
    STAIRS
}

public struct Cell
{
    public Vector3 center;
    public Vector3 sizes;

    public CellTypes cellType; //Specifies that this space is a room space

    public void DrawGizmo()
    {
        Gizmos.DrawWireCube(center, sizes);
    }
}

public class Grid : MonoBehaviour
{
    //Dimensions of cell
    float cellWidth = 0f;   //X
    float cellHeight = 0f;  //Y
    float cellDepth = 0f;   //Z

    public Vector3 CellDimensions
    {
        get { return new Vector3(cellWidth, cellHeight, cellDepth); }
    }

    //Number of cells in each direction
    int cellCountX = 0;
    int cellCountY = 0;
    int cellCountZ = 0;

    public Vector3 GridDimensions
    {
        get { return new Vector3(cellCountX, cellCountY, cellCountZ); }
    }

    private Cell[,,] cells;

    [SerializeField] bool drawGrid = true;

    public ref Cell GetCell(int x, int y, int z)
    {
        return ref cells[x, y, z];
    }
    public ref Cell GetCell(Vector3 indices)
    {
        return ref cells[(int)indices.x, (int)indices.y, (int)indices.z];
    }

    public void InitGrid(Vector3 cellDimensions, Vector3 cellCount)
    {
        cellWidth = cellDimensions.x;
        cellHeight = cellDimensions.y;
        cellDepth = cellDimensions.z;

        cellCountX = (int)cellCount.x;
        cellCountY = (int)cellCount.y;
        cellCountZ = (int)cellCount.z;

        cells = new Cell[cellCountX, cellCountY, cellCountZ];

        for (int i = 0; i < cellCountX; i++)
        {
            for (int j = 0; j < cellCountY; j++)
            {
                for (int k = 0; k < cellCountZ; k++)
                {
                    cells[i, j, k].cellType = CellTypes.NONE;
                    cells[i, j, k].center = new Vector3(i * cellWidth, j * cellHeight, k * cellDepth);
                    cells[i, j, k].sizes = new Vector3(cellWidth, cellHeight, cellDepth);
                }
            }
        }
    }

    public Vector3 GetCenter(Vector3 pos)
    {
        Vector3 indices = GetGridIndices(pos);
        float x = (cellWidth * indices.x);
        float y = (cellHeight * indices.y);
        float z = (cellDepth * indices.z);

        return new Vector3(x, y, z);
    }

    public Vector3 GetGridIndices(Vector3 pos)
    {
        float x = Mathf.Floor(pos.x / cellWidth);
        float y = Mathf.Floor(pos.y / cellHeight);
        float z = Mathf.Floor(pos.z / cellDepth);

        return new Vector3(x, y, z);
    }

    public Vector3 GetCenterByIndices(Vector3 indices)
    {
        float x = (cellWidth * indices.x);
        float y = (cellHeight * indices.y);
        float z = (cellDepth * indices.z);

        return new Vector3(x, y, z);
    }

    public bool CanPlaceRoom(Vector3 roomIndex, Room room)
    {
        //In Bounds
        if (roomIndex.x < 0 || roomIndex.x >= GridDimensions.x ||
            roomIndex.y < 0 || roomIndex.y >= GridDimensions.y ||
            roomIndex.z < 0 || roomIndex.z >= GridDimensions.z)
        {
            return false;
        }

        //Not inside room
        if (cells[(int)roomIndex.x, (int)roomIndex.y, (int)roomIndex.z].cellType != CellTypes.NONE)
        {
            return false;
        }

        //Not next to room cell that is not included in this room's cells
        if ((roomIndex.x + 1 < GridDimensions.x && cells[(int)roomIndex.x + 1, (int)roomIndex.y, (int)roomIndex.z].cellType != CellTypes.NONE && !room.cells.Contains(cells[(int)roomIndex.x + 1, (int)roomIndex.y, (int)roomIndex.z])) ||
            (roomIndex.x - 1 >= 0 && cells[(int)roomIndex.x - 1, (int)roomIndex.y, (int)roomIndex.z].cellType != CellTypes.NONE && !room.cells.Contains(cells[(int)roomIndex.x - 1, (int)roomIndex.y, (int)roomIndex.z])) ||
            (roomIndex.y + 1 < GridDimensions.y && cells[(int)roomIndex.x, (int)roomIndex.y + 1, (int)roomIndex.z].cellType != CellTypes.NONE && !room.cells.Contains(cells[(int)roomIndex.x, (int)roomIndex.y + 1, (int)roomIndex.z])) ||
            (roomIndex.y - 1 >= 0 && cells[(int)roomIndex.x, (int)roomIndex.y - 1, (int)roomIndex.z].cellType != CellTypes.NONE && !room.cells.Contains(cells[(int)roomIndex.x, (int)roomIndex.y - 1, (int)roomIndex.z])) ||
            (roomIndex.z + 1 < GridDimensions.z && cells[(int)roomIndex.x, (int)roomIndex.y, (int)roomIndex.z + 1].cellType != CellTypes.NONE && !room.cells.Contains(cells[(int)roomIndex.x, (int)roomIndex.y, (int)roomIndex.z + 1])) ||
            (roomIndex.z - 1 >= 0 && cells[(int)roomIndex.x, (int)roomIndex.y, (int)roomIndex.z - 1].cellType != CellTypes.NONE && !room.cells.Contains(cells[(int)roomIndex.x, (int)roomIndex.y, (int)roomIndex.z - 1])))
        {
            return false;
        }

        return true;
    }

    private void OnDrawGizmos()
    {
        if(Application.isPlaying && drawGrid)
        {
            foreach (Cell cell in cells)
            {
                cell.DrawGizmo();
            }
        }
    }
}
