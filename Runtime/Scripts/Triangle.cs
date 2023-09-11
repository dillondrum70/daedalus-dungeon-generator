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

	    Triangle.cs

	    ********************************************
	    ***      2D version of Tetrahedron       ***
	    ********************************************
 */



using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Triangle made up of 3 points, 3 triangles comprise a tetrahedron.
/// </summary>
public class Triangle : IEquatable<Triangle>
{
    public bool isInvalid = false;  //Only used in tetrahedralization
    public Vector3 pointA;
    public Vector3 pointB;
    public Vector3 pointC;

    //Defines a center and a radius
    public Circumcircle circumCircle;

    public Triangle(Vector3 newPointA, Vector3 newPointB, Vector3 newPointC)
    {
        pointA = newPointA;
        pointB = newPointB;
        pointC = newPointC;

        circumCircle = DelaunayTetrahedralization.FindCircumCircleCenter(pointA, pointB, pointC);
    }

    /// <summary>
    /// Caclulates if the given point is inside the circumcircle of this triangle
    /// </summary>
    /// <param name="point">Given point to check</param>
    /// <returns>Whether or not the circumcircle contains the point</returns>
    public bool CircumcircleContainsPoint(Vector3 point)
    {
        if (circumCircle.radius == 0)
        {
            circumCircle = DelaunayTetrahedralization.FindCircumCircleCenter(this);
        }

        if ((point - circumCircle.center).sqrMagnitude <= circumCircle.radius * circumCircle.radius)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Create array of edges using the points that make up the triangle
    /// </summary>
    /// <returns>An array of edges comprising the triangle</returns>
    public Edge[] GetEdges()
    {
        Edge[] edges = new Edge[3];

        edges[0] = new Edge(pointA, pointB);
        edges[1] = new Edge(pointA, pointC);
        edges[2] = new Edge(pointC, pointB);

        return edges;
    }

    /// <summary>
    /// Checks if the triangle contains the given edge
    /// </summary>
    /// <param name="edge"></param>
    /// <returns></returns>
    public bool ContainsEdge(Edge edge)
    {
        if (edge == new Edge(pointA, pointB) ||
           edge == new Edge(pointA, pointC) ||
           edge == new Edge(pointC, pointB))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Draw triangle
    /// </summary>
    public void DrawGizmos()
    {
        Gizmos.DrawLine(pointA, pointB);
        Gizmos.DrawLine(pointA, pointC);
        Gizmos.DrawLine(pointC, pointB);
    }

    /// <summary>
    /// Check equality of points between triangles
    /// </summary>
    /// <param name="lhs">Left Triangle</param>
    /// <param name="rhs">Right Triangle</param>
    /// <returns></returns>
    public static bool operator ==(Triangle lhs, Triangle rhs)
        => ((lhs.pointA == rhs.pointA) || (lhs.pointA == rhs.pointB) || (lhs.pointA == rhs.pointC)) &&
            ((lhs.pointB == rhs.pointA) || (lhs.pointB == rhs.pointB) || (lhs.pointB == rhs.pointC)) &&
            ((lhs.pointC == rhs.pointA) || (lhs.pointC == rhs.pointB) || (lhs.pointC == rhs.pointC));

    /// <summary>
    /// NOT of ==
    /// </summary>
    /// <param name="lhs">Left Triangle</param>
    /// <param name="rhs">Right Triangle</param>
    /// <returns></returns>
    public static bool operator !=(Triangle lhs, Triangle rhs)
        => !(lhs == rhs);
    public bool Equals(Triangle t)
    {
        return this == t;
    }

    public override int GetHashCode()
    {
        return pointA.GetHashCode() ^ pointB.GetHashCode() ^ pointC.GetHashCode();
    }
}
