using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Information specific to a circumsphere for a tetrahedron
/// </summary>
public struct Circumsphere
{
    public Vector3 center;
    public float radius;
}

/// <summary>
/// Information specific to a circumcircle for a tetrahedron
/// </summary>
public struct Circumcircle
{
    public Vector3 center;
    public float radius;
}

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
        if(lhs is Edge && rhs is Edge)
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
        if(other == null)
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
            return ( sqr1 == sqr2 ? 0 : sqr1 < sqr2 ? -1 : 1);
        }
    }

    //int IEquatable<Edge>.GetHashCode(Edge obj)
    //{
    //    return obj.pointA.GetHashCode() ^ obj.pointB.GetHashCode();
    //}
}


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

        if((point - circumCircle.center).sqrMagnitude <= circumCircle.radius * circumCircle.radius)
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
        if(edge == new Edge(pointA, pointB) ||
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
        =>  ((lhs.pointA == rhs.pointA) || (lhs.pointA == rhs.pointB) || (lhs.pointA == rhs.pointC)) &&
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

        circumSphere = DelaunayTriangulation.FindCircumSphereCenter(pointA, pointB, pointC, pointD);
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
        //    circumSphere = DelaunayTriangulation.FindCircumcenter(this);
        //}
        circumSphere = DelaunayTriangulation.FindCircumSphereCenter(this);

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
        if(tri == new Triangle(pointA, pointB, pointC) ||
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

/// <summary>
/// Main class for Tetrahedralization
/// </summary>
public class DelaunayTriangulation
{
    /// <summary>
    /// Finds circumcenter of passed triangle
    /// https://gamedev.stackexchange.com/questions/60630/how-do-i-find-the-circumcenter-of-a-triangle-in-3d
    /// </summary>
    /// <param name="tri">Triangle to check</param>
    /// <returns>Circumcircle describing triangle.</returns>
    public static Circumcircle FindCircumCircleCenter(Triangle tri)
    {
        Circumcircle circle;

        Vector3 ac = tri.pointC - tri.pointA;   //Vector from A to C
        Vector3 ab = tri.pointB - tri.pointA;   //Vector from A to B
        Vector3 abCrossAC = Vector3.Cross(ab, ac);  //Cross product of AC with AB

        Vector3 aToCenter = ((Vector3.Cross(abCrossAC, ab) * ac.sqrMagnitude) + (Vector3.Cross(ac, abCrossAC) * ab.sqrMagnitude)) 
                / (2.0f * abCrossAC.sqrMagnitude);
        circle.radius = aToCenter.magnitude;    //Distance between circle center and any point on triangle is the radius
        circle.center = tri.pointA + aToCenter;     //The actual center of the circumcircle

        //sphere.center = (Vector3.Cross(abCrossAC, ab) * ac.magnitude + Vector3.Cross(ac, abCrossAC) * ab.magnitude) / (2.0f * abCrossAC.magnitude);
        //sphere.radius = Mathf.Abs((sphere.center - pointA).magnitude);

        return circle;
    }

    /// <summary>
    /// Finds circumcenter of passed points of a triangle
    /// https://gamedev.stackexchange.com/questions/60630/how-do-i-find-the-circumcenter-of-a-triangle-in-3d
    /// </summary>
    /// <param name="pointA">First point</param>
    /// <param name="pointB">Second point</param>
    /// <param name="pointC">Third point</param>
    /// <returns>Circumcircle describing triangle.</returns>
    public static Circumcircle FindCircumCircleCenter(Vector3 pointA, Vector3 pointB, Vector3 pointC)
    {
        Circumcircle circle;

        Vector3 ac = pointC - pointA;   //Vector from A to C
        Vector3 ab = pointB - pointA;   //Vector from A to B
        Vector3 abCrossAC = Vector3.Cross(ab, ac);  //Cross product of AC with AB

        Vector3 aToCenter = ((Vector3.Cross(abCrossAC, ab) * ac.sqrMagnitude) + (Vector3.Cross(ac, abCrossAC) * ab.sqrMagnitude))
                / (2.0f * abCrossAC.sqrMagnitude);
        circle.radius = aToCenter.magnitude;    //Distance between circle center and any point on triangle is the radius
        circle.center = pointA + aToCenter;     //The actual center of the circumcircle

        //sphere.center = (Vector3.Cross(abCrossAC, ab) * ac.magnitude + Vector3.Cross(ac, abCrossAC) * ab.magnitude) / (2.0f * abCrossAC.magnitude);
        //sphere.radius = Mathf.Abs((sphere.center - pointA).magnitude);

        return circle;
    }

    /// <summary>
    /// Finds circumsphere center from passed tetrahedron
    /// http://rodolphe-vaillant.fr/entry/127/find-a-tetrahedron-circumcenter
    /// </summary>
    /// <param name="tet">Tetrahedron circumcenter to calculate</param>
    /// <returns>Returns the center of the circumsphere of passed tetrahedron</returns>
    public static Circumsphere FindCircumSphereCenter(Tetrahedron tet)
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

    /// <summary>
    /// Find circumsphere center given points of a tetrahedron
    /// </summary>
    /// <param name="pointA">First point</param>
    /// <param name="pointB">Second point</param>
    /// <param name="pointC">Third point</param>
    /// <param name="pointD">Fourth point</param>
    /// <returns>Returns circumsphere center</returns>
    public static Circumsphere FindCircumSphereCenter(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD)
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

    /// <summary>
    /// Run tetrahedralization algorithm
    /// </summary>
    /// <param name="superTetrahedron">Tetrahedron containing entire graph</param>
    /// <param name="pointList">List of points to insert into tetrahedralization that each represent the center of a room</param>
    /// <returns>Returns list of tetrahedrons representing the completed tetrahedralization</returns>
    public static List<Tetrahedron> Tetrahedralize(Tetrahedron superTetrahedron, List<Vector3> pointList)
    {
        List<Tetrahedron> tetrahedrons = new List<Tetrahedron>();

        //Add super tetrahedron initially
        tetrahedrons.Add(superTetrahedron);

        //Loop through all points
        foreach (Vector3 point in pointList)
        {
            //Triangles in tetrahedrons
            List<Triangle> triangles = new List<Triangle>();

            //Find which tetrahedrons whose circumspheres contain the center of the room
            //Store the triangles of the tetrahedrom
            foreach (Tetrahedron tet in tetrahedrons)
            {
                if (tet.CircumsphereContainsPoint(point))
                {
                    tet.isInvalid = true;

                    Triangle[] tetTriangles = tet.GetTriangles();
                    triangles.Add(tetTriangles[0]);
                    triangles.Add(tetTriangles[1]);
                    triangles.Add(tetTriangles[2]);
                    triangles.Add(tetTriangles[3]);
                }
            }

            //If same triangle is included in the circumsphere of multiple tetrahedrons, it should be removed so we mark it as such
            for (int i = 0; i < triangles.Count; i++)
            {
                for (int j = i + 1; j < triangles.Count; j++)
                {
                    if (triangles[i] == triangles[j])
                    {
                        triangles[i].isInvalid = true;
                        triangles[j].isInvalid = true;
                    }
                }
            }

            //Remove invalid tetrahedrons and triangles
            tetrahedrons.RemoveAll((Tetrahedron tet) => tet.isInvalid);
            triangles.RemoveAll((Triangle tri) => tri.isInvalid);

            //Create a new tetrahedron using each valid triangle and the room center
            foreach (Triangle tri in triangles)
            {
                tetrahedrons.Add(new Tetrahedron(tri.pointA, tri.pointB, tri.pointC, point));
            }
        }

        //Remove all tetrahedrons connected to the sueper tetrahedron
        tetrahedrons.RemoveAll((Tetrahedron t) => t.ContainsVertex(superTetrahedron.pointA) ||
                t.ContainsVertex(superTetrahedron.pointB) ||
                t.ContainsVertex(superTetrahedron.pointC) ||
                t.ContainsVertex(superTetrahedron.pointD));

        return tetrahedrons;
    }
}
