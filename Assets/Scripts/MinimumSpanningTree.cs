using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The class that handles creating a Minimum Spanning Tree out of edges
/// </summary>
public static class MinimumSpanningTree
{

    /// <summary>
    /// Main function for running MST algorithm
    /// </summary>
    /// <param name="excluded">Edges excluded from MST</param>
    /// <param name="start">Point to start at for MST</param>
    /// <param name="map">The map of all edges that can be used</param>
    /// <returns>A list of edges with the MST</returns>
    public static List<Edge> DerriveMST(out List<Edge> excluded, Vector3 start, Dictionary<Vector3, List<Edge>> map)
    {
        HashSet<Vector3> visited = new();       //Which nodes have already been visited
        List<Edge> solution = new();            //Stores edges of minimum spanning tree
                                                //List<Edge> notSolution = new();         //Edges not in solution
        PriorityQueue<Edge> frontier = new();   //Edges left to check, sorted shortest to longest top to bottom
        Dictionary<Vector3, List<Edge>> adjacencyList = new();  //Edges of entire connected graph (undirected)

        adjacencyList = new(map);   //Copy list, not reference, we may want original map later
        excluded = new();   //All edges that aren't in minSpanTree

        Visit(start, visited, adjacencyList, frontier);

        while(!frontier.Empty())
        {
            //Get next node and remove it from the priority queue
            Edge current = frontier.Top();
            frontier.Pop();

            //Check if already visited point
            if(visited.Contains(current.pointB))
            {
                excluded.Add(current);
                continue;
            }

            //Save this edge
            solution.Add(current);

            //Log that that point has been visited
            //Our graph is undirected so we visit both points since we might be coming from pointB to pointA
            Visit(current.pointB, visited, adjacencyList, frontier);
        }
        //notSolution = excluded;
        return solution;
    }

    /// <summary>
    /// Visits a node, updates the frontier, and updates the visited function
    /// </summary>
    /// <param name="v">Point to check</param>
    private static void Visit(Vector3 v, HashSet<Vector3> visited, Dictionary<Vector3, List<Edge>> adjacencyList, PriorityQueue<Edge> frontier)
    {
        visited.Add(v);

        //Loop through edges and push new ones to the frontier
        if(adjacencyList.TryGetValue(v, out List<Edge> nextList))
        {
            foreach (Edge e in nextList)
            {
                if (!visited.Contains(e.pointB))
                {
                    frontier.Push(e);
                }
            }
        }
    }

    //private void OnDrawGizmos()
    //{
    //    foreach (Edge e in notSolution)
    //    {
    //        Gizmos.color = Color.red;
    //        Gizmos.DrawLine(e.pointA, e.pointB);
    //    }
    //    foreach (Edge e in solution)
    //    {
    //        Gizmos.color = Color.green;
    //        Gizmos.DrawLine(e.pointA, e.pointB);
    //    }
    //}
}


