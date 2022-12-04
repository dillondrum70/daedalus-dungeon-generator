using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


public class AStarNode : IComparable<AStarNode>
{
    public AStarNode parent = null;

    public float h = 0; //Distance to goal
    public float g = 0; //Number of moves from start
    public float f = 0; //sum of g and h

    public Vector3Int indices = Vector3Int.zero; //indices in grid of this node

    public AStarNode(Vector3Int valIndices, float valG, float valH, AStarNode valParent = null)
    {
        indices = valIndices;
        g = valG;
        h = valH;
        f = valH + valG;
        parent = valParent;
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
    static PriorityQueue<AStarNode> open;
    static PriorityQueue<AStarNode> closed;

    public static Stack<AStarNode> Run(Vector3Int startIndex, Vector3Int goalIndex, Grid grid)
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

            //For each of the 3 levels, add the 4 possible movement directions
            //Hallways can only move:
                //Forward, back, left right, not diagonal horizontally
                //Diagonally up or down in any of the 4 previously mentioned directions, never straight up or down
            for (int y = -1; y <= 1; y++)
            {
                Stack<AStarNode> result;
                //Get index of cell to the north of the current cell
                Vector3Int north = new Vector3Int(current.indices.x, current.indices.y + y, current.indices.z + 1);
                result = CheckCell(current, north, goalIndex, grid);
                if(result != null)
                {
                    return result;
                }

                Vector3Int south = new Vector3Int(current.indices.x, current.indices.y + y, current.indices.z - 1);
                result = CheckCell(current, south, goalIndex, grid);
                if (result != null)
                {
                    return result;
                }

                Vector3Int west = new Vector3Int(current.indices.x + 1, current.indices.y + y, current.indices.z);
                result = CheckCell(current, west, goalIndex, grid);
                if (result != null)
                {
                    return result;
                }

                Vector3Int east = new Vector3Int(current.indices.x - 1, current.indices.y + y, current.indices.z);
                result = CheckCell(current, east, goalIndex, grid);
                if (result != null)
                {
                    return result;
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

    static Stack<AStarNode> CheckCell(AStarNode current, Vector3Int nextIndex, Vector3Int goalIndex, Grid grid)
    {
        //If the cell is within the bounds of the grid
        if (grid.IsValidCell(nextIndex))
        {
            AStarNode nextNode = new AStarNode(nextIndex, current.g + 1, FindH(nextIndex, goalIndex), current);

            //If nextIndex is the goal index
            if (nextIndex == goalIndex)
            {
                //End
                return TraceBackPath(current);
            }

            //Push the cell to the open list with its corresponding values for g and h, set current node as its parent
            if (grid.IsCellEmpty(nextIndex))
            {
                //Skip this cell if open or closed has a node with a lower f value, that means this one is obsolete
                //Make sure to null check the results before continuing in case a node does not exist with those indices
                AStarNode openNode = open[nextIndex];
                AStarNode closeNode = closed[nextIndex];
                if ((openNode == null || openNode?.f >= nextNode.f) && (closeNode == null || closeNode?.f >= nextNode.f))
                {
                    open.Push(nextNode);
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
}
