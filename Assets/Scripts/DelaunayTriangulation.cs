using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Circumsphere
{
    public Vector3 center;
    public float radius;
}

public class Edge : IEquatable<Edge>
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
    public bool Equals(Edge e)
    {
        return this == e;
    }

    public override int GetHashCode()
    {
        return pointA.GetHashCode() ^ pointB.GetHashCode();
    }

    //bool IEqualityComparer<Edge>.Equals(Edge a, Edge b)
    //{
    //    return ((a.pointA == b.pointA) && (a.pointB == b.pointB)) ||
    //       ((a.pointA == b.pointB) && (a.pointB == b.pointA));
    //}

    //int IEqualityComparer<Edge>.GetHashCode(Edge obj)
    //{
    //    return obj.pointA.GetHashCode() ^ obj.pointB.GetHashCode();
    //}
}


public class Triangle : IEquatable<Triangle>
{
    public bool isInvalid = false;  //Only used in tetrahedralization
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

    public static bool operator ==(Triangle lhs, Triangle rhs)
        =>  ((lhs.pointA == rhs.pointA) || (lhs.pointA == rhs.pointB) || (lhs.pointA == rhs.pointC)) &&
            ((lhs.pointB == rhs.pointA) || (lhs.pointB == rhs.pointB) || (lhs.pointB == rhs.pointC)) &&
            ((lhs.pointC == rhs.pointA) || (lhs.pointC == rhs.pointB) || (lhs.pointC == rhs.pointC));

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

//Expand triangle to 3D
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

        circumSphere = DelaunayTriangulation.FindCircumcenter(pointA, pointB, pointC, pointD);
    }

    public bool CircumsphereContainsPoint(Vector3 point)
    {
        //if (circumSphere.radius == 0)
        //{
        //    circumSphere = DelaunayTriangulation.FindCircumcenter(this);
        //}
        circumSphere = DelaunayTriangulation.FindCircumcenter(this);

        if ((point - circumSphere.center).sqrMagnitude <= circumSphere.radius * circumSphere.radius)
        {
            return true;
        }

        return false;
    }

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

    public Triangle[] GetTriangles()
    {
        Triangle[] triangles = new Triangle[4];

        triangles[0] = new Triangle(pointA, pointB, pointC);
        triangles[1] = new Triangle(pointA, pointC, pointD);
        triangles[2] = new Triangle(pointA, pointB, pointD);
        triangles[3] = new Triangle(pointB, pointC, pointD);

        return triangles;
    }

    public bool ContainsTriangle(Triangle tri)
    {
        if(tri == new Triangle(pointA, pointB, pointC) ||
           tri == new Triangle(pointA, pointC, pointD) ||
           tri == new Triangle(pointA, pointB, pointD) ||
           tri == new Triangle(pointB, pointC, pointD))
        {
            return true;
        }

        return false;
    }

    public bool ContainsVertex(Vector3 vert)
    {
        return vert == pointA ||
            vert == pointB ||
            vert == pointC ||
            vert == pointD;
    }

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

    //http://rodolphe-vaillant.fr/entry/127/find-a-tetrahedron-circumcenter
    public static Circumsphere FindCircumcenter(Tetrahedron tet)
    {
        Circumsphere sphere;

        Vector3 AB = tet.pointB - tet.pointA;   //Vector from A to B
        Vector3 AC = tet.pointC - tet.pointA;   //Vector from A to C
        Vector3 AD = tet.pointD - tet.pointA;   //Vector from A to D
        Vector3 ABCrossAC = Vector3.Cross(AB, AC);  //Cross product of AB with AC
        Vector3 ADCrossAB = Vector3.Cross(AD, AB);  //Cross product of AD with AB
        Vector3 ACCrossAD = Vector3.Cross(AC, AD);  //Cross product of AC with AD

        float denominator = .5f / ((AB.x * ACCrossAD.x) + (AB.y * ACCrossAD.y) + (AB.z * ACCrossAD.z));

        float ABLen = AB.sqrMagnitude, ACLen = AC.sqrMagnitude, ADLen = AD.sqrMagnitude;

        sphere.center = tet.pointA + new Vector3(    //The actual center of the circumsphere
            (ABLen * ACCrossAD.x) + (ACLen * ADCrossAB.x) + (ADLen * ABCrossAC.x),
            (ABLen * ACCrossAD.y) + (ACLen * ADCrossAB.y) + (ADLen * ABCrossAC.y),
            (ABLen * ACCrossAD.z) + (ACLen * ADCrossAB.z) + (ADLen * ABCrossAC.z)
        ) * denominator;

        sphere.radius = Vector3.Distance(sphere.center, tet.pointA);    //Distance between sphere center and any point on triangle is the radius   

        //sphere.center = (Vector3.Cross(abCrossAC, ab) * ac.magnitude + Vector3.Cross(ac, abCrossAC) * ab.magnitude) / (2.0f * abCrossAC.magnitude);
        //sphere.radius = Mathf.Abs((sphere.center - pointA).magnitude);

        return sphere;
    }

    public static Circumsphere FindCircumcenter(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD)
    {
        Circumsphere sphere;

        Vector3 AB = pointB - pointA;   //Vector from A to B
        Vector3 AC = pointC - pointA;   //Vector from A to C
        Vector3 AD = pointD - pointA;   //Vector from A to D
        Vector3 ABCrossAC = Vector3.Cross(AB, AC);  //Cross product of AB with AC
        Vector3 ADCrossAB = Vector3.Cross(AD, AB);  //Cross product of AD with AB
        Vector3 ACCrossAD = Vector3.Cross(AC, AD);  //Cross product of AC with AD

        float denominator = .5f / ((AB.x * ACCrossAD.x) + (AB.y * ACCrossAD.y) + (AB.z * ACCrossAD.z));

        float ABLen = AB.sqrMagnitude, ACLen = AC.sqrMagnitude, ADLen = AD.sqrMagnitude;

        sphere.center = pointA + new Vector3(    //The actual center of the circumsphere
            (ABLen * ACCrossAD.x) + (ACLen * ADCrossAB.x) + (ADLen * ABCrossAC.x),
            (ABLen * ACCrossAD.y) + (ACLen * ADCrossAB.y) + (ADLen * ABCrossAC.y),
            (ABLen * ACCrossAD.z) + (ACLen * ADCrossAB.z) + (ADLen * ABCrossAC.z)
        ) * denominator;

        sphere.radius = Vector3.Distance(sphere.center, pointA);    //Distance between sphere center and any point on triangle is the radius   

        //sphere.center = (Vector3.Cross(abCrossAC, ab) * ac.magnitude + Vector3.Cross(ac, abCrossAC) * ab.magnitude) / (2.0f * abCrossAC.magnitude);
        //sphere.radius = Mathf.Abs((sphere.center - pointA).magnitude);

        return sphere;
    }
}
