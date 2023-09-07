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

    //Circumspheres have a center and radius which is the same for a circumcircle so I'm just reusing the class here.
    public Circumcircle circumCircle;

    public Triangle(Vector3 newPointA, Vector3 newPointB, Vector3 newPointC)
    {
        pointA = newPointA;
        pointB = newPointB;
        pointC = newPointC;

        circumCircle = DelaunayTriangulation.FindCircumCircleCenter(pointA, pointB, pointC);
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
            circumCircle = DelaunayTriangulation.FindCircumCircleCenter(this);
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
