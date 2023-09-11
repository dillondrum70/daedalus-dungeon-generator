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

	    AStarNode.cs

	    ********************************************
	    ***          A* Algorithm Nodes          ***
	    ********************************************
 */


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singular node within an A* graph
/// Stores info such as the node's parent, indices, type, etc.
/// </summary>
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

    /// <summary>
    /// Overload == to check f value and indices
    /// </summary>
    /// <param name="lhs">Left node</param>
    /// <param name="rhs">Right node</param>
    /// <returns></returns>
    public static bool operator ==(AStarNode lhs, AStarNode rhs)
    {
        //Null check before accessing AStarNode member variables
        if (lhs is AStarNode && rhs is AStarNode)
        {
            return (lhs.f == rhs.f) && (lhs.indices == rhs.indices);
        }

        return lhs is null && rhs is null;

    }

    /// <summary>
    /// Makes use of NOT == which checks indices and f
    /// </summary>
    /// <param name="lhs">Left node</param>
    /// <param name="rhs">Right node</param>
    /// <returns></returns>
    public static bool operator !=(AStarNode lhs, AStarNode rhs) => !(lhs == rhs);

    /*
     * The following operator overloads all deal with the f value for use within data structures like Stacks
     */
    public static bool operator <(AStarNode lhs, AStarNode rhs) => lhs.f < rhs.f;

    public static bool operator >(AStarNode lhs, AStarNode rhs) => lhs.f > rhs.f;

    public static bool operator <=(AStarNode lhs, AStarNode rhs) => lhs.f <= rhs.f;

    public static bool operator >=(AStarNode lhs, AStarNode rhs) => lhs.f >= rhs.f;

    public int CompareTo(AStarNode other)
    {
        return this == other ? 0 : (this < other ? -1 : 1);
    }
}
