using System;
using System.Collections;
using System.Collections.Generic;
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
    const int stairCost = 5;
    static PriorityQueue<AStarNode> open;
    static PriorityQueue<AStarNode> closed;

    public static Stack<AStarNode> Run(Vector3Int startIndex, Vector3Int goalIndex, Room goalRoom, Grid grid)
    {
        if(!grid.IsValidCell(goalIndex))
        {
            throw new Exception("goalIndex is an invalid cell");
        }

        open = new();
        closed = new();

        //Add start node to open list
        open.Push(new AStarNode(startIndex, 0, FindH(startIndex, goalIndex)));  //Parent is null since it is start node


        while(!open.Empty())
        {
            AStarNode current = open.Top();
            open.Pop();

            if (current.indices == goalIndex || goalRoom.HasLevelAdjacentCell(current.indices)/* && current.indices.y == goalIndex.y*/)
            {
                //End
                return TraceBackPath(current);
            }

            //Figure out which direction we came from so we know not to backtrack that direction
            Directions cameFromDirection = Directions.UNDEFINED;
            if(current.parent != null)
            {
                cameFromDirection = DirectionCameFrom(current.parent.indices, current.indices);
            }

            //For each of the 3 levels, add the 4 possible movement directions
            //Hallways can only move:
                //Forward, back, left right, not diagonal horizontally
                //Diagonally up or down in any of the 4 previously mentioned directions, never straight up or down
            for (int y = -1; y <= 1; y++)
            {
                Stack<AStarNode> result;

                if(cameFromDirection != Directions.NORTH)
                {
                    //Get index of cell to the north of the current cell
                    Vector3Int north = new Vector3Int(current.indices.x, current.indices.y + y, current.indices.z + 1);
                    result = CheckCell(current, north, goalIndex, goalRoom, grid, (y == 0), cameFromDirection);
                    if (result != null)
                    {
                        return result;
                    }
                }
                
                if(cameFromDirection != Directions.SOUTH)
                {
                    Vector3Int south = new Vector3Int(current.indices.x, current.indices.y + y, current.indices.z - 1);
                    result = CheckCell(current, south, goalIndex, goalRoom, grid, (y == 0), cameFromDirection);
                    if (result != null)
                    {
                        return result;
                    }
                }

                if(cameFromDirection != Directions.WEST)
                {
                    Vector3Int west = new Vector3Int(current.indices.x + 1, current.indices.y + y, current.indices.z);
                    result = CheckCell(current, west, goalIndex, goalRoom, grid, (y == 0), cameFromDirection);
                    if (result != null)
                    {
                        return result;
                    }
                }
                
                if(cameFromDirection != Directions.EAST)
                {
                    Vector3Int east = new Vector3Int(current.indices.x - 1, current.indices.y + y, current.indices.z);
                    result = CheckCell(current, east, goalIndex, goalRoom, grid, (y == 0), cameFromDirection);
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

    static Stack<AStarNode> CheckCell(AStarNode current, Vector3Int nextIndex, Vector3Int goalIndex, Room goalRoom, Grid grid, bool hallsCanOverlap, Directions lastDirection)
    {
        //If the cell is within the bounds of the grid
        if (grid.IsValidCell(nextIndex))
        {
            AStarNode nextNode = new AStarNode(nextIndex, current.g + 1, FindH(nextIndex, goalIndex), current);

            //If nextIndex is the goal index AND if the last hallway is level with the goal room
            //If we want to allow staircases to lead down into rooms with the staircase physically inside the room instead of
            //right outside the room entrance, we can remove "current.indices.y == goalIndex.y" or make it <= for just staircases
            //leading up from below
            //if (nextIndex == goalIndex/* && current.indices.y == goalIndex.y*/)
            //{
            //    //End
            //    return TraceBackPath(current);
            //}

            //If we are adjacent to a cell the room contains, return path
            //if(goalRoom.HasAdjacentCell(nextIndex))
            //{
            //    return TraceBackPath(nextNode);
            //}

            //Push the cell to the open list with its corresponding values for g and h, set current node as its parent
            //We can have hallways overlap, but we don't want staircases to overlap hallways or that could get messy, gives us more 
            if (/*(hallsCanOverlap && grid.GetCell(nextIndex).cellType == CellTypes.HALLWAY) ||*/ grid.IsCellEmpty(nextIndex))
            {
                //Skip this cell if open or closed has a node with a lower f value, that means this one is obsolete
                //Make sure to null check the results before continuing in case a node does not exist with those indices
                AStarNode openNode = open[nextIndex];
                AStarNode closeNode = closed[nextIndex];
                if ((openNode == null || openNode?.f > nextNode.f) && (closeNode == null || closeNode?.f > nextNode.f))
                {
                    //Make sure this index does not already appear in the path
                    if(!IndexInPath(nextIndex, current))
                    {
                        //Transform trans = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube), new Vector3((nextIndex.x * grid.CellDimensions.x), (nextIndex.y * grid.CellDimensions.y), (nextIndex.z * grid.CellDimensions.z)), Quaternion.identity, GameObject.Find("Rooms").transform).transform;

                        //trans.localScale = grid.CellDimensions;

                        //Stairwell case
                        if (current.indices.y != nextIndex.y && //current and next node are on different levels, must be stairs
                            //and if last cell was stairs but the next cell does not move in the same direction, do not create staircase
                            //This should emerge landings in staircases when they turn
                            !(current.nodeType == CellTypes.STAIRS && lastDirection != DirectionCameFrom(current.indices, nextIndex)))  
                        {
                            //We can assume this index is inside the grid since the current node and the next one have already been
                            //verified by this point
                            Vector3Int levelIndex = new Vector3Int(nextIndex.x, current.indices.y, nextIndex.z);

                            //Grid index MUST be empty since the stairs will block anything or, if the stairs are going down, the space above
                            //the stairs would cause the floor of a hallway or rooom to be missing
                            //We might not care if its a room?  We could put railings on the sides of the empty space and have a little balcony
                            //if the stairs are going down
                            if (grid.IsCellEmpty(levelIndex))
                            {
                                nextNode.nodeType = CellTypes.STAIRS;
                                nextNode.g += stairCost - 1;
                                open.Push(nextNode);
                            }
                        }
                        else //Otherwise, its a hallway
                        {
                            open.Push(nextNode);
                        }
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
            if(current.indices == index)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    //Return the direction the current path just came from, prevent overlap by going backwards on staircases
    static Directions DirectionCameFrom(Vector3Int parentIndices, Vector3Int currentIndices)
    {
        if(parentIndices != null && currentIndices != null)
        {
            Vector3Int diff = currentIndices - parentIndices;

            if (diff.z > 0)
            {
                return Directions.NORTH;
            }

            if (diff.z < 0)
            {
                return Directions.SOUTH;
            }

            if (diff.x > 0)
            {
                return Directions.EAST;
            }

            if (diff.x < 0)
            {
                return Directions.WEST;
            }
        }

        //If x and y are equal, we are at the same node as the last step, haven't moved
        //Only other way to get here is if one passed Vector3Int is null
        //Should only happen for the first node
        return Directions.UNDEFINED;
    }
}
