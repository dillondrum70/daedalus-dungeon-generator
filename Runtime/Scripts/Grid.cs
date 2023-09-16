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

	    Grid.cs

	    ********************************************
	    ***       Stores 3D array of cells       ***
	    ********************************************
 */


using System.Linq;
using UnityEngine;

/// <summary>
/// Enum of what can be inside a cell
/// </summary>
public enum CellTypes
{
    NONE = 0,
    ROOM,
    HALLWAY,
    STAIRS,     //The actual staircase cell
    STAIRSPACE  //The empty cell above a staircase that is necessary to be able to walk up a staircase
}


/// <summary>
/// Grid component
/// </summary>
public class Grid : MonoBehaviour
{
    //Dimensions of cell in terms of width, height, and depth
    [HideInInspector]
    public Vector3 cellDimensions = Vector3.zero;

    //Number of cells in each direction in terms of width, height, and depth
    [HideInInspector]
    public Vector3 gridDimensions = Vector3.zero;

    //3D array of cells
    private Cell[,,] cells;

    [Tooltip("Toggles whether or not the grid should be drawn.")]
    [SerializeField] bool drawGrid = false;

    /*
     * Getters
     */
    public ref Cell GetCell(int x, int y, int z)
    {
        return ref cells[x, y, z];
    }
    public ref Cell GetCell(Vector3Int indices)
    {
        return ref cells[indices.x, indices.y, indices.z];
    }

    /// <summary>
    /// Initializes grid with cell dimensions and count
    /// </summary>
    /// <param name="cellDimensions">Size of individual cells</param>
    /// <param name="cellCount">Dimensions of grid in terms of cells</param>
    public void InitGrid(Vector3 cellDimensions, Vector3 cellCount)
    {
        this.cellDimensions = cellDimensions;

        gridDimensions = cellCount;

        cells = new Cell[(int)gridDimensions.x, (int)gridDimensions.y, (int)gridDimensions.z];

        //Initializes values in each grid
        for (int i = 0; i < (int)gridDimensions.x; i++)
        {
            for (int j = 0; j < (int)gridDimensions.y; j++)
            {
                for (int k = 0; k < (int)gridDimensions.z; k++)
                {
                    cells[i, j, k].cellType = CellTypes.NONE;
                    cells[i, j, k].center = new Vector3(i * (int)cellDimensions.x, j * (int)cellDimensions.y, k * (int)cellDimensions.z);
                    cells[i, j, k].sizes = new Vector3((int)cellDimensions.x, (int)cellDimensions.y, (int)cellDimensions.z);
                    cells[i, j, k].index = new Vector3Int(i, j, k);
                }
            }
        }
    }

    /// <summary>
    /// Returns the world position center of the grid
    /// </summary>
    /// <returns>World Space center of the grid</returns>
    public Vector3 GetGridCenter()
    {
        return new Vector3((int)gridDimensions.x * .5f * (int)cellDimensions.x, 
            (int)gridDimensions.y * .5f * (int)cellDimensions.y, 
            (int)gridDimensions.z * .5f * (int)cellDimensions.z);
    }

    /// <summary>
    /// Returns the world space position of the closest cell to the passed position
    /// </summary>
    /// <param name="pos">Given position</param>
    /// <returns>World Space center of cell containing given point</returns>
    public Vector3 GetCenter(Vector3 pos)
    {
        Vector3 indices = GetGridIndices(pos);
        float x = ((int)cellDimensions.x * indices.x);
        float y = ((int)cellDimensions.y * indices.y);
        float z = ((int)cellDimensions.z * indices.z);

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Gets index in 3D array that the given point is contained in
    /// </summary>
    /// <param name="pos">Given point</param>
    /// <returns>Indices of cell in 3D array point is contained by</returns>
    public Vector3Int GetGridIndices(Vector3 pos)
    {
        int x = (int)Mathf.Floor(pos.x / (int)cellDimensions.x);
        int y = (int)Mathf.Floor(pos.y / (int)cellDimensions.y);
        int z = (int)Mathf.Floor(pos.z / (int)cellDimensions.z);

        return new Vector3Int(x, y, z);
    }

    /// <summary>
    /// Gets the center of a cell by its index
    /// </summary>
    /// <param name="indices">Indices of a cell</param>
    /// <returns>Center of cell belonging to given indices</returns>
    public Vector3 GetCenterByIndices(Vector3 indices)
    {
        float x = ((int)cellDimensions.x * indices.x);
        float y = ((int)cellDimensions.y * indices.y);
        float z = ((int)cellDimensions.z * indices.z);

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Checks if you can place a room on the grid
    /// </summary>
    /// <param name="roomIndex">Index of room in grid</param>
    /// <param name="room">Reference to room</param>
    /// <returns>If it is possible to place a room here</returns>
    public bool CanPlaceRoom(Vector3Int roomIndex, Room room)
    {
        return IsValidCell(roomIndex) && IsCellEmpty(roomIndex) && NotAdjacentToFilledRoom(roomIndex, room);
    }

    /// <summary>
    /// Checks if cell is valid by index
    /// </summary>
    /// <param name="index">Indices to check</param>
    /// <returns>Whether or not the indices are inside the bounds of the grid</returns>
    public bool IsValidCell(Vector3Int index)
    {
        //In Bounds
        if (index.x < 0 || index.x >= gridDimensions.x ||
            index.y < 0 || index.y >= gridDimensions.y ||
            index.z < 0 || index.z >= gridDimensions.z)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if a cell contains something
    /// </summary>
    /// <param name="index">Given indices</param>
    /// <returns>Whether or not the cell is empty of rooms, hallways, etc.</returns>
    public bool IsCellEmpty(Vector3Int index)
    {
        //Not inside room
        if (cells[index.x, index.y, index.z].cellType != CellTypes.NONE)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if index is adjacent to a filled room
    /// </summary>
    /// <param name="index">Indices to check</param>
    /// <param name="room">Reference to room to be checked</param>
    /// <returns>If the current room/indices don't have any filled rooms nearby</returns>
    public bool NotAdjacentToFilledRoom(Vector3Int index, Room room)
    {
        //Not next to room cell that is not included in this room's cells
        if ((index.x + 1 < gridDimensions.x && cells[index.x + 1, index.y, index.z].cellType != CellTypes.NONE && !room.cells.Contains(cells[index.x + 1, index.y, index.z])) ||
            (index.x - 1 >= 0 && cells[index.x - 1, index.y, index.z].cellType != CellTypes.NONE && !room.cells.Contains(cells[index.x - 1, index.y, index.z])) ||
            (index.y + 1 < gridDimensions.y && cells[index.x, index.y + 1, index.z].cellType != CellTypes.NONE && !room.cells.Contains(cells[index.x, index.y + 1, index.z])) ||
            (index.y - 1 >= 0 && cells[index.x, index.y - 1, index.z].cellType != CellTypes.NONE && !room.cells.Contains(cells[index.x, index.y - 1, index.z])) ||
            (index.z + 1 < gridDimensions.z && cells[index.x, index.y, index.z + 1].cellType != CellTypes.NONE && !room.cells.Contains(cells[index.x, index.y, index.z + 1])) ||
            (index.z - 1 >= 0 && cells[index.x, index.y, index.z - 1].cellType != CellTypes.NONE && !room.cells.Contains(cells[index.x, index.y, index.z - 1])))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Draw the cells in the grid.
    /// </summary>
    private void OnDrawGizmos()
    {
        if(cells == null)
        {
            return;
        }

        if(Application.isPlaying && drawGrid)
        {
            foreach (Cell cell in cells)
            {
                cell.DrawGizmo();
            }
        }
    }
}
