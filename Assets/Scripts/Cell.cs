using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cell struct containing info on dimensions, center, what's inside the cell, etc.
/// Cells make up the grid.
/// </summary>
public struct Cell
{
    public Vector3 center;
    public Vector3 sizes;
    public Vector3Int index;

    public CellTypes cellType; //Specifies that this space is a room space
    public Directions faceDirection; //Only matters for stairs

    //Returns true if there is one free adjacent cell on the same y level as this cell
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

    /// <summary>
    /// Calculates what direction a cell is coming from
    /// </summary>
    /// <param name="parentIndices">Parent index</param>
    /// <param name="currentIndices">Current index</param>
    /// <returns></returns>
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

    /// <summary>
    /// Returns the opposite direction of a given direction
    /// </summary>
    /// <param name="dir">Given direction</param>
    /// <returns>Opposite of given direction</returns>
    public static Directions OppositeDirection(Directions dir)
    {
        switch (dir)
        {
            case Directions.NORTH:
                return Directions.SOUTH;
            case Directions.SOUTH:
                return Directions.NORTH;
            case Directions.WEST:
                return Directions.EAST;
            case Directions.EAST:
                return Directions.WEST;
        }

        return Directions.UNDEFINED;
    }

    /// <summary>
    /// Draws cube representing cell
    /// </summary>
    public void DrawGizmo()
    {
        Gizmos.DrawWireCube(center, sizes);
    }
}
