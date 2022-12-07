using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public enum Directions
{
    UNDEFINED = 0,
    NORTH,
    SOUTH,
    EAST,
    WEST
}

public class AStarNode : IComparable<AStarNode>
{
    public AStarNode parent = null;

    public float h = 0; //Distance to goal
    public float g = 0; //Number of moves from start
    public float f = 0; //sum of g and h

    public Vector3Int indices = Vector3Int.zero; //indices in grid of this node
    public Vector3Int extraStairIndex = Vector3Int.zero;    //Stores index of the extra space if this is a staircase node

    public CellTypes nodeType = CellTypes.HALLWAY;

    public AStarNode(Vector3Int valIndices, float valG, float valH, AStarNode valParent = null, CellTypes valType = CellTypes.HALLWAY)
    {
        indices = valIndices;
        g = valG;
        h = valH;
        f = valH + valG;
        parent = valParent;
        nodeType = valType;
    }

    //public float F() { return h + g; }  //F is the sum of h and g, the lower the F, the better the node is for the path

    public static bool operator ==(AStarNode lhs, AStarNode rhs)
    {
        //Null check before accessing AStarNode member variables
        if (lhs is AStarNode && rhs is AStarNode)
        {
            return (lhs.f == rhs.f) && (lhs.indices == rhs.indices);
        }

        return lhs is null && rhs is null;
        
    }

    public static bool operator !=(AStarNode lhs, AStarNode rhs) => !(lhs == rhs);

    public static bool operator <(AStarNode lhs, AStarNode rhs) => lhs.f < rhs.f;

    public static bool operator >(AStarNode lhs, AStarNode rhs) => lhs.f > rhs.f;

    public static bool operator <=(AStarNode lhs, AStarNode rhs) => lhs.f <= rhs.f;

    public static bool operator >=(AStarNode lhs, AStarNode rhs) => lhs.f >= rhs.f;

    public int CompareTo(AStarNode other)
    {
        return this == other ? 0 : (this < other ? -1 : 1);
    }
}

public class AStar : MonoBehaviour
{
    public const float sqrt2 = 1.414f;
    public static Vector3Int constNorth = Vector3Int.forward;
    public static Vector3Int constSouth = -Vector3Int.forward;
    public static Vector3Int constEast = Vector3Int.right;
    public static Vector3Int constWest = -Vector3Int.right;
    public static Vector3Int constUndefined = Vector3Int.zero;

    const int stairCost = 5;
    static PriorityQueue<AStarNode> open;
    static PriorityQueue<AStarNode> closed;

    public static Stack<AStarNode> Run(Vector3Int startIndex, Vector3Int goalIndex, Room goalRoom, Grid grid)
    {
        if (!grid.IsValidCell(goalIndex))
        {
            throw new Exception("goalIndex is an invalid cell");
        }

        open = new();
        closed = new();

        //Add start node to open list
        open.Push(new AStarNode(startIndex, 0, FindH(startIndex, goalIndex)));  //Parent is null since it is start node

        while (!open.Empty())
        {
            AStarNode current = open.Top();
            open.Pop();

            Vector3Int lastDirection = constUndefined;
            
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

                if(lastDirection != constNorth)
                {
                    //Get index of cell to the north of the current cell
                    Vector3Int north = new Vector3Int(current.indices.x, current.indices.y + y, current.indices.z + 1);
                    result = CheckCell(current, north, goalIndex, goalRoom, grid);
                    if (result != null)
                    {
                        return result;
                    }
                }

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

    static float FindH(Vector3Int currentIndex, Vector3Int goalIndex)
    {
        //The heuristic here is extracted so we can change it if we want
        //This is the Manhattan Distance method since we only move in 4 directions
        //We increase h a small amount so that the algorithm will prioritize checking spaces closer to the goal and do less checks to the side of the eventual path
        return (Mathf.Abs(currentIndex.x - goalIndex.x) + Mathf.Abs(currentIndex.y - goalIndex.y) + Mathf.Abs(currentIndex.z - goalIndex.z)) * 1.001f;
    }

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
                        if(!grid.IsCellEmpty(nextNode.extraStairIndex) || IndexInPath(nextIndex, current))
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

    //Accepts the node that is adjacent to the goal node
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

    //Traces back and returns true if passed index appears in the path
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

    //Return the direction the current path just came from, prevent overlap by going backwards on staircases
    static Vector3Int DirectionCameFrom(Vector3Int parentIndices, Vector3Int currentIndices)
    {
        if(parentIndices != null && currentIndices != null)
        {
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
        }

        //If x and y are equal, we are at the same node as the last step, haven't moved
        //Only other way to get here is if one passed Vector3Int is null
        //Should only happen for the first node
        return constUndefined;
    }
}
