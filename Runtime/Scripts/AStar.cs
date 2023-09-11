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

	    AStar.cs

	    ********************************************
	    ***     A* Algorithm Implementation      ***
	    ********************************************
 */


using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Directions from a given cell
/// No diagonals
/// Vertical cells ignored since stairs handle those automatically
/// </summary>
public enum Directions
{
    UNDEFINED = 0,
    NORTH,
    SOUTH,
    EAST,
    WEST
}

/// <summary>
/// Main class for A* algorithm
/// </summary>
public static class AStar
{
    //Representations of each direction in index form
    public static Vector3Int constNorth = Vector3Int.forward;
    public static Vector3Int constSouth = -Vector3Int.forward;
    public static Vector3Int constEast = Vector3Int.right;
    public static Vector3Int constWest = -Vector3Int.right;
    public static Vector3Int constUndefined = Vector3Int.zero;

    const int stairCost = 5;    //Cost for A* to create stairs, disuades algorithm from needlessly creating stairs
    static PriorityQueue<AStarNode> open;   //Open nodes, not fully explored
    static PriorityQueue<AStarNode> closed; //Closed nodes, fully explored
    
    /// <summary>
    /// Main function that runs A* algorithm
    /// </summary>
    /// <param name="startIndex">Index to start A* at</param>
    /// <param name="goalIndex">Index to end A* at</param>
    /// <param name="goalRoom">Reference to the room containing the goalIndex (in case we reach a different cell within the room sooner)
    /// This may happen if the actual closest cell is already taken up by another hallway or room.</param>
    /// <param name="grid">Reference to grid</param>
    /// <returns>Stack of AStarNodes that represents the correct path.</returns>
    /// <exception cref="Exception">Make sure of certainties like goal index existing</exception>
    public static Stack<AStarNode> Run(Vector3Int startIndex, Vector3Int goalIndex, Room goalRoom, Grid grid)
    {
        if (!grid.IsValidCell(goalIndex)) //Sanity Check
        {
            throw new Exception("goalIndex is an invalid cell");
        }
         
        open = new();
        closed = new();

        //Add start node to open list
        open.Push(new AStarNode(startIndex, 0, FindH(startIndex, goalIndex)));  //Parent is null since it is start node

        //Repeat until the open list is empty
        while (!open.Empty())
        {
            //Take next open node with shortest path and remove from open
            AStarNode current = open.Top();
            open.Pop();

            Vector3Int lastDirection = constUndefined;  //Initialize
            
            //Need last direction to make sure algorithm does not try and check backwards
            //Fun fact, without this, the stairs start spawning on top of eachother making impossible hallways to traverse
            if(current.parent != null)
            {
                lastDirection = DirectionCameFrom(current.parent.indices, current.indices);
            }

            //For each of the 3 levels, add the 4 possible movement directions
            //Hallways can only move:
            //Forward, back, left right, not diagonal horizontally
            //Diagonally up or down in any of the 4 previously mentioned directions, never straight up or down
            for (int y = -1; y <= 1; y++)
            {
                Stack<AStarNode> result;

                //Make sure last direction wasn't North so we don't go backwards
                if(lastDirection != constNorth)
                {
                    //Get index of cell to the north of the current cell
                    Vector3Int north = new Vector3Int(current.indices.x, current.indices.y + y, current.indices.z + 1);
                    result = CheckCell(current, north, goalIndex, goalRoom, grid);  //Check cell for solution and add adjacent cells
                    if (result != null)
                    {
                        //If result != null, we have found the solution
                        return result;
                    }
                }

                //Subsequent if statements follow same format as above for constNorth
                if(lastDirection != constSouth)
                {
                    Vector3Int south = new Vector3Int(current.indices.x, current.indices.y + y, current.indices.z - 1);
                    result = CheckCell(current, south, goalIndex, goalRoom, grid);
                    if (result != null)
                    {
                        return result;
                    }
                }

                if (lastDirection != constEast)
                {
                    Vector3Int east = new Vector3Int(current.indices.x + 1, current.indices.y + y, current.indices.z);
                    result = CheckCell(current, east, goalIndex, goalRoom, grid);
                    if (result != null)
                    {
                        return result;
                    }
                }

                if (lastDirection != constWest)
                {
                    Vector3Int west = new Vector3Int(current.indices.x - 1, current.indices.y + y, current.indices.z);
                    result = CheckCell(current, west, goalIndex, goalRoom, grid);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            //Close out this node
            closed.Push(current);
        }

        return null;    //AStar failed to find a path if open is ever empty and we haven't already returned
    }

    /// <summary>
    /// Calculate H (distance from current to goal) of current index
    /// </summary>
    /// <param name="currentIndex">Index of current cell</param>
    /// <param name="goalIndex">Index of goal cell</param>
    /// <returns>H value Manhattan Distance of current cell</returns>
    static float FindH(Vector3Int currentIndex, Vector3Int goalIndex)
    {
        //The heuristic here is extracted so we can change it if we want
        //This is the Manhattan Distance method since we only move in 4 directions
        //We increase h a small amount so that the algorithm will prioritize checking spaces closer to the goal and do less checks to the side of the eventual path
        return (Mathf.Abs(currentIndex.x - goalIndex.x) + Mathf.Abs(currentIndex.y - goalIndex.y) + Mathf.Abs(currentIndex.z - goalIndex.z)) * 1.001f;
    }

    /// <summary>
    /// Check if we've reached the end of the A* path and return solution if so.
    /// Otherwise, add relevant nodes to open and run steps.
    /// </summary>
    /// <param name="current">Current AStarNode being checked</param>
    /// <param name="nextIndex">Index of cell being checked</param>
    /// <param name="goalIndex">Index of goal</param>
    /// <param name="goalRoom">Reference to room containing goal cell</param>
    /// <param name="grid">Reference to grid</param>
    /// <returns>Return entire path to this point</returns>
    static Stack<AStarNode> CheckCell(AStarNode current, Vector3Int nextIndex, Vector3Int goalIndex, Room goalRoom, Grid grid)
    {
        //If the cell is within the bounds of the grid
        if (grid.IsValidCell(nextIndex))
        {
            AStarNode nextNode = new AStarNode(nextIndex, current.g + 1, FindH(nextIndex, goalIndex), current);

            Cell outCell;

            //If the last 
            //if(current.parent != null && current.parent.nodeType == CellTypes.STAIRS && DirectionCameFrom(current.indices, nextIndex) == DirectionCameFrom(nextIndex, outCell.index))
            //{
            //    return TraceBackPath(current);
            //}

            if (goalRoom.ContainsIndex(nextIndex, out outCell) && current.indices.y == nextIndex.y)//If nextIndex is the in room, trace path with node just before that
            {
                //End
                return TraceBackPath(current);
            }

            //Check that the candidate cell is empty and not already part of our path (don't want to write over previous hallways/stairs
            if (grid.IsCellEmpty(nextIndex) && !IndexInPath(nextIndex, current))
            {

                //if nextIndex is adjacent to one of the cells in the room and the next node is not a staircase
                if (goalRoom.HasLevelAdjacentCell(nextIndex, out outCell) && current.indices.y == nextIndex.y)
                {
                    return TraceBackPath(nextNode);
                }

                //Skip this cell if open or closed has a node with a lower f value, that means this one is obsolete
                //Make sure to null check the results before continuing in case a node does not exist with those indices
                AStarNode openNode = open[nextIndex];
                AStarNode closeNode = closed[nextIndex];
                if ((openNode == null || openNode?.f > nextNode.f) && (closeNode == null || closeNode?.f > nextNode.f))
                {
                    //If this index isn't the same y as the last, that means we're adding stairs so we need to update the cost
                    if(current.indices.y != nextIndex.y)
                    {
                        nextNode.g += stairCost - 1; //Subtracting one since g was already incremented once for a normal hallway

                        //We can assume this index is valid since we know both current.indices and nextIndex are valid
                        nextNode.extraStairIndex = (nextIndex.y < current.indices.y ? nextIndex + Vector3Int.up : nextIndex - Vector3Int.up);
                        nextNode.nodeType = CellTypes.STAIRS;

                        //If cell for staircase isn't empty or in our path, then staircase is invalid
                        if(!grid.IsCellEmpty(nextNode.extraStairIndex) || IndexInPath(nextNode.extraStairIndex, current))
                        {
                            return null;
                        }

                        /////////////////////////////////
                        /// Enforce landings after stairs
                        /////////////////////////////////

                        //Adds the direction value
                        Vector3Int afterIndex = nextIndex - DirectionCameFrom(current.indices, nextIndex);

                        //We do check cell again on the new hallway, it should not cause an infinite recursive loop since the afterIndex should always
                        //be a hallway, pushing the actual node is handled in the CheckCell call
                        Stack<AStarNode> path = CheckCell(nextNode, afterIndex, goalIndex, goalRoom, grid);

                        //If CheckCell returns a path, that means it finished the path, just like normal
                        if(path != null)
                        {
                            return path;
                        }

                        //Get the node that comes after the staircase in the direction that the staircase faces and check
                        //This enforces landings for spiral staircases and forces the stairs to face a valid direction for the player to go
                        //Set the parent as the stairwell node so we don't lose the stairs
                        //AStarNode afterNode = new AStarNode(afterIndex, nextNode.g + 1, FindH(afterIndex, goalIndex), nextNode, CellTypes.HALLWAY);

                        //open.Push(afterNode);
                    }
                    else
                    {
                        //Push the cell to the open list with its corresponding values for g and h, set current node as its parent
                        open.Push(nextNode);
                    }
                }
            }
        }

        return null;    //Returns null if not an ending node
    }

    /// <summary>
    /// Loops through parents of subsequent AStarNodes to find path to start.
    /// Accepts the node that is adjacent to the goal node.
    /// </summary>
    /// <param name="adjecentEnd">Node that is adjacent to a cell in the goal room but NOT a cell in the room</param>
    /// <returns>A Stack containing the path of AStarNodes between the start and goal</returns>
    static Stack<AStarNode> TraceBackPath(AStarNode adjecentEnd)
    {
        Stack<AStarNode> path = new Stack<AStarNode>();

        AStarNode current = adjecentEnd;

        //While current parent node is not the first node, loop back through chain of nodes, ignore last node since it is the start node
        while(current.parent != null)
        {
            path.Push(current);
            current = current.parent;
        }

        return path;
    }

    /// <summary>
    /// Traces back and returns true if passed index appears in the path
    /// </summary>
    /// <param name="index">Index to check</param>
    /// <param name="currentNode">Current AStarNode (used to trace back through parents)</param>
    /// <returns>Whether or not the given index is in the path.</returns>
    static bool IndexInPath(Vector3Int index, AStarNode currentNode)
    {
        AStarNode current = currentNode;

        while(current != null)
        {
            if(current.indices == index ||  //Check not equal to node index
                (current.nodeType == CellTypes.STAIRS && current.extraStairIndex == index)) //and not equal to extra stair cell if the node is a staircase
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    /// <summary>
    /// Return the direction the current path just came from.  Mainly used to prevent overlap by going backwards on staircases.
    /// It is assumed that the passed indices are adjacent in the four cardinal directions, not vertically or diagonally.
    /// Vertical or diagonal differences may have undefined or unexpected results.
    /// </summary>
    /// <param name="parentIndices">Index of parent node before current node</param>
    /// <param name="currentIndices">Index of current node to check</param>
    /// <returns></returns>
    static Vector3Int DirectionCameFrom(Vector3Int parentIndices, Vector3Int currentIndices)
    {
        if(parentIndices == null || currentIndices == null) //Sanity Check
        {
            Debug.LogError("Passes indices are null");
            return constUndefined;
        }

        //Check which direction the differnce is and return that direction
        Vector3Int diff = currentIndices - parentIndices;

        if (diff.z > 0)
        {
            return constSouth;
        }

        if (diff.z < 0)
        {
            return constNorth;
        }

        if (diff.x > 0)
        {
            return constWest;
        }

        if (diff.x < 0)
        {
            return constEast;
        }

        //If x and y are equal, we are at the same node as the last step, haven't moved
        //Only other way to get here is if one passed Vector3Int is null
        //Should only happen for the first node
        return constUndefined;
    }
}
