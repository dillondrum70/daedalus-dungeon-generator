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

	    Room.cs

	    ********************************************
	    ***  Room in the dungeon made of cells   ***
	    ********************************************
 */


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A room is a collection of cells in a grid.  Each room is connected to at least one other by the end of the generation process.
/// </summary>
[Serializable]
public class Room : IEquatable<Room>
{
    //lowest x, y, and z valued cell in room
    //Cell parentCell;

    //cells contained by room
    //index 0 is parent cell, lowest x, y, and z value in the room
    public List<Cell> cells = new List<Cell>();

    public Vector3 center;

    /// <summary>
    /// Find average position of cells to find center of room
    /// </summary>
    public void CalculateCenter()
    {
        Vector3 avgPos = Vector3.zero;

        foreach (Cell c in cells)
        {
            avgPos += c.center;
        }

        avgPos = avgPos / cells.Count;

        center = avgPos;
    }

    /// <summary>
    /// If one of the cells in the room is next to the passed index (including above or below), return true
    /// </summary>
    /// <param name="index">Index to check</param>
    /// <returns></returns>
    public bool HasAdjacentCell(Vector3Int index)
    {
        foreach (Cell c in cells)
        {
            if ((c.index - index).sqrMagnitude == 1)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// If one of the cells in the room is next to the passed index (not above or below), return true
    /// </summary>
    /// <param name="index">Index to be checked</param>
    /// <param name="adjacentCell">The adjacent cell in question that is part of the target room</param>
    /// <returns></returns>
    public bool HasLevelAdjacentCell(Vector3Int index, out Cell adjacentCell)
    {
        adjacentCell = default;

        foreach (Cell c in cells)
        {
            if ((c.index - index).sqrMagnitude == 1 && index.y == c.index.y)
            {
                adjacentCell = c;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// If one of the cells in the room matches the passed index, return true
    /// </summary>
    /// <param name="index">Index of the cell to check inside the room</param>
    /// <param name="outCell">The cell at said index</param>
    /// <returns></returns>
    public bool ContainsIndex(Vector3Int index, out Cell outCell)
    {
        outCell = default;

        //Loop through cells in room
        foreach (Cell c in cells)
        {
            if (index == c.index)
            {
                outCell = c;
                return true;
            }
        }

        return false;
    }


    /// <summary>
    /// Returns the indices of the closest cell to the goal index in the room that has an open side so A star can perform
    /// </summary>
    /// <param name="goalIndex">Index of the goal room (center of the room)</param>
    /// <param name="grid">The grid of all cells in the dungeon</param>
    /// <returns></returns>
    public Vector3Int ClosestValidStartCell(Vector3 goalIndex, Grid grid)
    {
        //Initialize closest cell to a value far away which would be an invalid value
        Vector3Int closest = new Vector3Int((int)grid.gridDimensions.x, (int)grid.gridDimensions.y, (int)grid.gridDimensions.z) * 3;

        //Loop through all cells in the room to find which one is closest
        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].HasFreeLevelAdjacentCell(grid) &&                                      //If has free adjacent space to the N, S, E, or W 
               (closest - goalIndex).sqrMagnitude > (cells[i].index - goalIndex).sqrMagnitude) //and closer to goal
            {
                //Then set the closest cell to this cell
                closest = cells[i].index;
            }
        }

        return closest;
    }

    /// <summary>
    /// Returns whether or not passed room equals this room
    /// </summary>
    /// <param name="other">Room to be checked</param>
    /// <returns></returns>
    public bool Equals(Room other)
    {
        return this == other;
    }

    /// <summary>
    /// Operater overload for == that compares room centers and cell counts
    /// We could go in depth here and check if each cell is equivalent, but in reality, 
    /// rooms should theoretically never have the same center so we shouldn't need to worry
    /// </summary>
    /// <param name="lhs">Left Room</param>
    /// <param name="rhs">Right Room</param>
    /// <returns></returns>
    public static bool operator ==(Room lhs, Room rhs)
    {
        //Check if either is null before accessing variables
        if (lhs is Room && rhs is Room)
        {
            return (lhs.center == rhs.center) && (lhs.cells.Count == rhs.cells.Count);
        }

        //If at least one parameter was null, return true if both are
        return lhs is null && rhs is null;
    }

    /// <summary>
    /// Operator overload for !=, simply takes NOT of == operator overload
    /// </summary>
    /// <param name="lhs">Left Room</param>
    /// <param name="rhs">Right Room</param>
    /// <returns></returns>
    public static bool operator !=(Room lhs, Room rhs)
        => !(lhs == rhs);

    //public static bool operator ==(Room lhs, Edge rhs)
    //    => ((lhs.center == rhs.pointA) || (lhs.center == rhs.pointB));

    //public static bool operator !=(Room lhs, Edge rhs)
    //    => !(lhs == rhs);
}
