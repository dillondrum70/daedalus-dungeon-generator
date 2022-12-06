using System.Linq;
using UnityEngine;

public enum CellTypes
{
    NONE = 0,
    ROOM,
    HALLWAY,
    STAIRS,     //The actual staircase cell
    STAIRSPACE  //The empty cell above a staircase that is necessary to be able to walk up a staircase
}

public struct Cell
{
    public Vector3 center;
    public Vector3 sizes;
    public Vector3Int index;

    public CellTypes cellType; //Specifies that this space is a room space
    public Directions faceDirection; //Only matters for stairs

    //Returns true if there is one free adjacent cell on the same y level as the passed cell
    public bool HasFreeLevelAdjacentCell(Grid grid)
    {
        //Check north
        if (grid.IsValidCell(index + Vector3Int.forward) && grid.GetCell(index + Vector3Int.forward).cellType == CellTypes.NONE)
        {
            return true;
        }
        //South
        if (grid.IsValidCell(index - Vector3Int.forward) && grid.GetCell(index - Vector3Int.forward).cellType == CellTypes.NONE)
        {
            return true;
        }
        //East
        if (grid.IsValidCell(index + Vector3Int.right) && grid.GetCell(index + Vector3Int.right).cellType == CellTypes.NONE)
        {
            return true;
        }
        //West
        if (grid.IsValidCell(index - Vector3Int.right) && grid.GetCell(index - Vector3Int.right).cellType == CellTypes.NONE)
        {
            return true;
        }
        return false;
    }

    public static Directions DirectionCameFrom(Vector3Int parentIndices, Vector3Int currentIndices)
    {
        if (parentIndices != null && currentIndices != null)
        {
            Vector3Int diff = currentIndices - parentIndices;

            if (diff.z > 0)
            {
                return Directions.SOUTH;
            }

            if (diff.z < 0)
            {
                return Directions.NORTH;
            }

            if (diff.x > 0)
            {
                return Directions.WEST;
            }

            if (diff.x < 0)
            {
                return Directions.EAST;
            }
        }

        //If x and y are equal, we are at the same node as the last step, haven't moved
        //Only other way to get here is if one passed Vector3Int is null
        //Should only happen for the first node
        return Directions.UNDEFINED;
    }

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
    public ref Cell GetCell(Vector3Int indices)
    {
        return ref cells[indices.x, indices.y, indices.z];
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
                    cells[i, j, k].index = new Vector3Int(i, j, k);
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

    public Vector3Int GetGridIndices(Vector3 pos)
    {
        int x = (int)Mathf.Floor(pos.x / cellWidth);
        int y = (int)Mathf.Floor(pos.y / cellHeight);
        int z = (int)Mathf.Floor(pos.z / cellDepth);

        return new Vector3Int(x, y, z);
    }

    public Vector3 GetCenterByIndices(Vector3 indices)
    {
        float x = (cellWidth * indices.x);
        float y = (cellHeight * indices.y);
        float z = (cellDepth * indices.z);

        return new Vector3(x, y, z);
    }

    public bool CanPlaceRoom(Vector3Int roomIndex, Room room)
    {
        return IsValidCell(roomIndex) && IsCellEmpty(roomIndex) && NotAdjacentToFilledRoom(roomIndex, room);
    }

    public bool IsValidCell(Vector3Int index)
    {
        //In Bounds
        if (index.x < 0 || index.x >= GridDimensions.x ||
            index.y < 0 || index.y >= GridDimensions.y ||
            index.z < 0 || index.z >= GridDimensions.z)
        {
            return false;
        }

        return true;
    }

    public bool IsCellEmpty(Vector3Int index)
    {
        //Not inside room
        if (cells[index.x, index.y, index.z].cellType != CellTypes.NONE)
        {
            return false;
        }

        return true;
    }

    public bool NotAdjacentToFilledRoom(Vector3Int index, Room room)
    {
        //Not next to room cell that is not included in this room's cells
        if ((index.x + 1 < GridDimensions.x && cells[index.x + 1, index.y, index.z].cellType != CellTypes.NONE && !room.cells.Contains(cells[index.x + 1, index.y, index.z])) ||
            (index.x - 1 >= 0 && cells[index.x - 1, index.y, index.z].cellType != CellTypes.NONE && !room.cells.Contains(cells[index.x - 1, index.y, index.z])) ||
            (index.y + 1 < GridDimensions.y && cells[index.x, index.y + 1, index.z].cellType != CellTypes.NONE && !room.cells.Contains(cells[index.x, index.y + 1, index.z])) ||
            (index.y - 1 >= 0 && cells[index.x, index.y - 1, index.z].cellType != CellTypes.NONE && !room.cells.Contains(cells[index.x, index.y - 1, index.z])) ||
            (index.z + 1 < GridDimensions.z && cells[index.x, index.y, index.z + 1].cellType != CellTypes.NONE && !room.cells.Contains(cells[index.x, index.y, index.z + 1])) ||
            (index.z - 1 >= 0 && cells[index.x, index.y, index.z - 1].cellType != CellTypes.NONE && !room.cells.Contains(cells[index.x, index.y, index.z - 1])))
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
