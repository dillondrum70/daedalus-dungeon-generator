using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimumSpanningTree : MonoBehaviour
{
    HashSet<Vector3> visited = new();       //Which nodes have already been visited
    List<Edge> solution = new();            //Stores edges of minimum spanning tree
    List<Edge> notSolution = new();         //Edges not in solution
    PriorityQueue<Edge> frontier = new();   //Edges left to check, sorted shortest to longest top to bottom
    Dictionary<Vector3, List<Edge>> adjacencyList = new();  //Edges of entire connected graph (undirected)

    public List<Edge> DerriveMST(out List<Edge> excluded, Vector3 start, Dictionary<Vector3, List<Edge>> map)
    {
        Clear();
        adjacencyList = new(map);   //Copy list, not reference, we may want original map later
        excluded = new();   //All edges that aren't in minSpanTree

        Visit(start);

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
            Visit(current.pointB);
            //Visit(current.pointA);
        }
        notSolution = excluded;
        return solution;
    }

    private void Visit(Vector3 v)
    {
        visited.Add(v);

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

    private void Clear()
    {
        visited.Clear();
        solution.Clear();
        frontier.Clear();
        adjacencyList.Clear();
    }

    private void OnDrawGizmos()
    {
        foreach (Edge e in notSolution)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(e.pointA, e.pointB);
        }
        foreach (Edge e in solution)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(e.pointA, e.pointB);
        }
    }
}

//Adapted from https://www.dotnetlovers.com/article/231/priority-queue#:~:text=Implementation%20of%20Priority%20Queue%20using%20Heap&text=Unlike%20ordinary%20queues%2C%20a%20priority,highest%20priority%20can%20be%20fetched.
//Decided to make it simpler using list.sort(), though likely less efficient than the source which uses a custom heap, depends on how sort is implemented
public class PriorityQueue<T>
{
    List<T> queue = new List<T>();

    public int Count
    {
        get { return queue.Count; }
    }

    public void Push(T obj)
    {
        queue.Add(obj);
        queue.Sort();
    }

    public void Pop()
    {
        if(queue.Count == 0)
        {
            throw new System.Exception("Queue is empty");
        }

        queue.Remove(queue[0]);

        //Don't need to resort since it should already be sorted, removing a value won't change that status
    }

    public T Top()
    {
        if (queue.Count == 0)
        {
            throw new System.Exception("Queue is empty");
        }

        return queue[0];
    }

    public bool Contains(T obj)
    {
        return queue.Contains(obj);
    }

    public void Clear()
    {
        queue.Clear();
    }

    public bool Empty()
    {
        return queue.Count == 0;
    }
}
