using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Circumsphere
{
    public Vector3 center;
    public float radius;
}

public struct Edge
{
    public Vector3 pointA;
    public Vector3 pointB;

    public Edge(Vector3 newPointA, Vector3 newPointB)
    {
        pointA = newPointA;
        pointB = newPointB;
    }

    public static bool operator ==(Edge lhs, Edge rhs)
        => ((lhs.pointA == rhs.pointA) && (lhs.pointB == rhs.pointB)) ||
           ((lhs.pointA == rhs.pointB) && (lhs.pointB == rhs.pointA));

    public static bool operator !=(Edge lhs, Edge rhs)
        => (lhs.pointA != rhs.pointA) || (lhs.pointB != rhs.pointB) &&
           ((lhs.pointA != rhs.pointB) || (lhs.pointB != rhs.pointA));
    //public bool Equals(Edge e)
    //{
    //    return this == e;
    //}

    //public override int GetHashCode()
    //{
    //    return pointA.GetHashCode() ^ pointB.GetHashCode();
    //}
}

public struct Triangle
{
    public Vector3 pointA;
    public Vector3 pointB;
    public Vector3 pointC;

    public Circumsphere circumSphere;

    public Triangle(Vector3 newPointA, Vector3 newPointB, Vector3 newPointC)
    {
        pointA = newPointA;
        pointB = newPointB;
        pointC = newPointC;

        circumSphere = DelaunayTriangulation.FindCircumcenter(pointA, pointB, pointC);
    }

    public bool CircumsphereContainsPoint(Vector3 point)
    {
        if (circumSphere.radius == 0)
        {
            circumSphere = DelaunayTriangulation.FindCircumcenter(this);
        }

        if((point - circumSphere.center).sqrMagnitude <= circumSphere.radius * circumSphere.radius)
        {
            return true;
        }

        return false;
    }

    public Edge[] GetEdges()
    {
        Edge[] edges = new Edge[3];

        edges[0] = new Edge(pointA, pointB);
        edges[1] = new Edge(pointA, pointC);
        edges[2] = new Edge(pointC, pointB);

        return edges;
    }

    public bool ContainsEdge(Edge edge)
    {
        if(edge == new Edge(pointA, pointB) ||
           edge == new Edge(pointA, pointC) ||
           edge == new Edge(pointC, pointB))
        {
            return true;
        }

        return false;
    }

    public void DrawGizmos()
    {
        Gizmos.DrawLine(pointA, pointB);
        Gizmos.DrawLine(pointA, pointC);
        Gizmos.DrawLine(pointC, pointB);
    }
}

public class DelaunayTriangulation
{
    //https://gamedev.stackexchange.com/questions/60630/how-do-i-find-the-circumcenter-of-a-triangle-in-3d
    public static Circumsphere FindCircumcenter(Triangle tri)
    {
        Circumsphere sphere;

        Vector3 ac = tri.pointC - tri.pointA;   //Vector from A to C
        Vector3 ab = tri.pointB - tri.pointA;   //Vector from A to B
        Vector3 abCrossAC = Vector3.Cross(ab, ac);  //Cross product of AC with AB

        Vector3 aToCenter = ((Vector3.Cross(abCrossAC, ab) * ac.sqrMagnitude) + (Vector3.Cross(ac, abCrossAC) * ab.sqrMagnitude)) 
                / (2.0f * abCrossAC.sqrMagnitude);
        sphere.radius = aToCenter.magnitude;    //Distance between sphere center and any point on triangle is the radius
        sphere.center = tri.pointA + aToCenter;     //The actual center of the circumsphere

        //sphere.center = (Vector3.Cross(abCrossAC, ab) * ac.magnitude + Vector3.Cross(ac, abCrossAC) * ab.magnitude) / (2.0f * abCrossAC.magnitude);
        //sphere.radius = Mathf.Abs((sphere.center - pointA).magnitude);

        return sphere;
    }

    public static Circumsphere FindCircumcenter(Vector3 pointA, Vector3 pointB, Vector3 pointC)
    {
        Circumsphere sphere;

        Vector3 ac = pointC - pointA;   //Vector from A to C
        Vector3 ab = pointB - pointA;   //Vector from A to B
        Vector3 abCrossAC = Vector3.Cross(ab, ac);  //Cross product of AC with AB

        Vector3 aToCenter = ((Vector3.Cross(abCrossAC, ab) * ac.sqrMagnitude) + (Vector3.Cross(ac, abCrossAC) * ab.sqrMagnitude))
                / (2.0f * abCrossAC.sqrMagnitude);
        sphere.radius = aToCenter.magnitude;    //Distance between sphere center and any point on triangle is the radius
        sphere.center = pointA + aToCenter;     //The actual center of the circumsphere

        //sphere.center = (Vector3.Cross(abCrossAC, ab) * ac.magnitude + Vector3.Cross(ac, abCrossAC) * ab.magnitude) / (2.0f * abCrossAC.magnitude);
        //sphere.radius = Mathf.Abs((sphere.center - pointA).magnitude);

        return sphere;
    }


}
