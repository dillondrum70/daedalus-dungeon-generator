using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Circumsphere
{
    public Vector3 center;
    public float radius;
}

public struct Triangle
{
    public Vector3 pointA;
    public Vector3 pointB;
    public Vector3 pointC;

    public Triangle(Vector3 newPointA, Vector3 newPointB, Vector3 newPointC)
    {
        pointA = newPointA;
        pointB = newPointB;
        pointC = newPointC;
    }

    public void DrawGizmos()
    {
        Gizmos.DrawLine(pointA, pointB);
        Gizmos.DrawLine(pointA, pointC);
        Gizmos.DrawLine(pointC, pointB);
    }

    public bool CircumsphereContainsPoint(Vector3 point)
    {
        Circumsphere sphere = DelaunayTriangulation.FindCircumcenter(this);

        if(Vector3.Distance(point, sphere.center) < sphere.radius)
        {
            return true;
        }

        return false;
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

    
}
