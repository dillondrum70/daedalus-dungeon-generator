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

	    Tetrahedron.cs

	    ********************************************
	    ***     Used for tetrahedralization      ***
	    ********************************************
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Expands triangle to 3D
/// </summary>
public class Tetrahedron
{
    public bool isInvalid = false;  //Only used in tetrahedralization
    public Vector3 pointA;
    public Vector3 pointB;
    public Vector3 pointC;
    public Vector3 pointD;

    public Circumsphere circumSphere;

    public Tetrahedron(Vector3 newPointA, Vector3 newPointB, Vector3 newPointC, Vector3 newPointD)
    {
        pointA = newPointA;
        pointB = newPointB;
        pointC = newPointC;
        pointD = newPointD;

        circumSphere = DelaunayTetrahedralization.FindCircumSphereCenter(pointA, pointB, pointC, pointD);
    }

    /// <summary>
    /// Checks if circumsphere contains supplied point
    /// </summary>
    /// <param name="point">Point to check</param>
    /// <returns>True if circumsphere contains point</returns>
    public bool CircumsphereContainsPoint(Vector3 point)
    {
        //if (circumSphere.radius == 0)
        //{
        //    circumSphere = DelaunayTetrahedralization.FindCircumcenter(this);
        //}
        circumSphere = DelaunayTetrahedralization.FindCircumSphereCenter(this);

        if ((point - circumSphere.center).sqrMagnitude <= circumSphere.radius * circumSphere.radius)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Creates array of edges from points
    /// </summary>
    /// <returns>Array of edges in tetrahedron</returns>
    public Edge[] GetEdges()
    {
        Edge[] edges = new Edge[6];

        edges[0] = new Edge(pointA, pointB);
        edges[1] = new Edge(pointA, pointC);
        edges[2] = new Edge(pointA, pointD);
        edges[3] = new Edge(pointB, pointC);
        edges[4] = new Edge(pointB, pointD);
        edges[5] = new Edge(pointC, pointD);

        return edges;
    }

    /// <summary>
    /// Checks if edge is in tetrahedron by constructing edges from points
    /// </summary>
    /// <param name="edge">Edge to check</param>
    /// <returns>Whether or not edge is in tetrahedron</returns>
    public bool ContainsEdge(Edge edge)
    {
        if (edge == new Edge(pointA, pointB) ||
           edge == new Edge(pointA, pointC) ||
           edge == new Edge(pointA, pointD) ||
           edge == new Edge(pointB, pointC) ||
           edge == new Edge(pointB, pointD) ||
           edge == new Edge(pointC, pointD))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Creates array of triangles from points
    /// </summary>
    /// <returns>Array of triangles representing points in tetrahedron</returns>
    public Triangle[] GetTriangles()
    {
        Triangle[] triangles = new Triangle[4];

        triangles[0] = new Triangle(pointA, pointB, pointC);
        triangles[1] = new Triangle(pointA, pointC, pointD);
        triangles[2] = new Triangle(pointA, pointB, pointD);
        triangles[3] = new Triangle(pointB, pointC, pointD);

        return triangles;
    }

    /// <summary>
    /// Checks if tetrahedron contains triangle
    /// </summary>
    /// <param name="tri">Triangle to check</param>
    /// <returns>Whether or not triangle is in tetrahedron</returns>
    public bool ContainsTriangle(Triangle tri)
    {
        if (tri == new Triangle(pointA, pointB, pointC) ||
           tri == new Triangle(pointA, pointC, pointD) ||
           tri == new Triangle(pointA, pointB, pointD) ||
           tri == new Triangle(pointB, pointC, pointD))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if point is in tetrahedron
    /// </summary>
    /// <param name="vert">Point to check</param>
    /// <returns>True if point is in tetrahedron</returns>
    public bool ContainsVertex(Vector3 vert)
    {
        return vert == pointA ||
            vert == pointB ||
            vert == pointC ||
            vert == pointD;
    }

    /// <summary>
    /// Draw tetrahedron
    /// </summary>
    public void DrawGizmos()
    {
        Gizmos.DrawLine(pointA, pointB);
        Gizmos.DrawLine(pointA, pointC);
        Gizmos.DrawLine(pointA, pointD);
        Gizmos.DrawLine(pointB, pointC);
        Gizmos.DrawLine(pointB, pointD);
        Gizmos.DrawLine(pointC, pointD);
    }
}
