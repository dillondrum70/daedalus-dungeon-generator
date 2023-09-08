using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Members and helper functions representing an edge in a tetrahedron.
/// Can be used in dictionaries and be compared.
/// </summary>
public class Edge : IEquatable<Edge>, IComparable<Edge>
{
    public Vector3 pointA;
    public Vector3 pointB;

    float magnitude = 0; //Cache magnitude for speed

    public Edge(Vector3 newPointA, Vector3 newPointB)
    {
        pointA = newPointA;
        pointB = newPointB;
        magnitude = Vector3.Distance(pointA, pointB);
    }

    /// <summary>
    /// SqrMagnitude of edge
    /// </summary>
    /// <returns>The SqrMagnitude of the edge.</returns>
    public float SqrLength()
    {
        return (pointA - pointB).sqrMagnitude;
    }

    /// <summary>
    /// Actual length of edge.
    /// </summary>
    /// <returns>Distance between points of edge.</returns>
    public float Length()
    {
        return Vector3.Distance(pointA, pointB);
    }

    /// <summary>
    /// Compares the points points in an edge to determine equality.
    /// Edges are undirected so lhs.pointA == rhs.pointB is valid.
    /// </summary>
    /// <param name="lhs">Left Edge</param>
    /// <param name="rhs">Right Edge</param>
    /// <returns></returns>
    public static bool operator ==(Edge lhs, Edge rhs)
    {
        //Null check before accessing Edge member variables
        if (lhs is Edge && rhs is Edge)
        {
            return ((lhs.pointA == rhs.pointA) && (lhs.pointB == rhs.pointB)) ||
               ((lhs.pointA == rhs.pointB) && (lhs.pointB == rhs.pointA));
        }

        return lhs is null && rhs is null;
    }

    /// <summary>
    /// NOT ==, compares points, undirected (lhs.pointA can equal rhs.pointB)
    /// </summary>
    /// <param name="lhs">Left Edge</param>
    /// <param name="rhs">Right Edge</param>
    /// <returns></returns>
    public static bool operator !=(Edge lhs, Edge rhs)
        => !(lhs == rhs);
    public bool Equals(Edge e)
    {
        return this == e;
    }

    public override int GetHashCode()
    {
        return pointA.GetHashCode() ^ pointB.GetHashCode();
    }

    bool IEquatable<Edge>.Equals(Edge b)
    {
        return this == b;
    }

    public int CompareTo(Edge other)
    {
        if (other == null)
        {
            return 1;
        }
        else
        {
            ////////////// WARNING //////////////////
            //This is incorrect if either pointA or
            //pointB is changed after creation.
            //That should never happen in this system,
            //but if it does, change these to use
            //SqrLength() for performance while still
            //adapting to changes in points.
            /////////////////////////////////////////
            float sqr1 = magnitude;
            float sqr2 = other.magnitude;
            return (sqr1 == sqr2 ? 0 : sqr1 < sqr2 ? -1 : 1);
        }
    }

    //int IEquatable<Edge>.GetHashCode(Edge obj)
    //{
    //    return obj.pointA.GetHashCode() ^ obj.pointB.GetHashCode();
    //}
}
