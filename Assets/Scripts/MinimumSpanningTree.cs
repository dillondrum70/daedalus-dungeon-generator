using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimumSpanningTree
{
    HashSet<Vector3> visited = new();
    List<Edge> minSpanTree = new();
    PriorityQueue<Edge> frontier = new();
    Dictionary<Vector3, List<Edge>> adjacencyList = new();

    public List<Edge> DerriveMST(out List<Edge> excluded, Vector3 start, Dictionary<Vector3, List<Edge>> map)
    {
        Clear();
        adjacencyList = new(map);   //Copy list, not reference, we may want original map later
        excluded = new();   //All edges that aren't in minSpanTree

        Visit(start);
        
        return minSpanTree;
    }

    private void Visit(Vector3 v)
    {
        visited.Add(v);

        foreach(Edge e in adjacencyList[v])
        {
            if(!visited.Contains(e.pointB))
            {
                frontier.Push(e);
            }
        }
    }

    private void Clear()
    {
        visited.Clear();
        minSpanTree.Clear();
        frontier.Clear();
        adjacencyList.Clear();
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
}
