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

	    DelaunayTetrahedralization.cs

	    ********************************************
	    ***      Finds streamlined hallways      ***
	    ********************************************
 */


using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main class for Tetrahedralization
/// </summary>
public static class DelaunayTetrahedralization
{
    /// <summary>
    /// Finds circumcircle of passed triangle
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
    /// Finds circumcircle of passed points of a triangle
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
    /// Finds circumsphere from passed tetrahedron
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
    /// Find circumsphere given points of a tetrahedron
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
