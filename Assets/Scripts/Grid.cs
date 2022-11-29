using UnityEngine;

public struct Cell
{
    public Vector3 center;
    public Vector3 sizes;

    public bool isRoom; //Specifies that this space is a room space

    public void DrawGizmo()
    {
        Gizmos.DrawWireCube(center, sizes);
    }
}

public class Grid : MonoBehaviour
{
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

    private Cell[,,] cells;

    public ref Cell GetCell(int x, int y, int z)
    {
        return ref cells[x, y, z];
    }
    public ref Cell GetCell(Vector3 indices)
    {
        return ref cells[(int)indices.x, (int)indices.y, (int)indices.z];
    }

    private void Awake()
    {
        cells = new Cell[cellCountX, cellCountY, cellCountZ];

        for (int i = 0; i < cellCountX; i++)
        {
            for(int j = 0; j < cellCountY; j++)
            {
                for(int k = 0; k < cellCountZ; k++)
                {
                    cells[i, j, k].isRoom = false;
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

    private void OnDrawGizmos()
    {
        if(Application.isPlaying)
        {
            foreach (Cell cell in cells)
            {
                cell.DrawGizmo();
            }
        }
    }
}
