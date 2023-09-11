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
        Adapted from https://www.dotnetlovers.com/article/231/priority-queue#:~:text=Implementation%20of%20Priority%20Queue%20using%20Heap&text=Unlike%20ordinary%20queues%2C%20a%20priority,highest%20priority%20can%20be%20fetched

	    PriorityQueue.cs

	    ********************************************
	    ***  Implementation of a priority queue  ***
	    ********************************************
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Adapted from https://www.dotnetlovers.com/article/231/priority-queue#:~:text=Implementation%20of%20Priority%20Queue%20using%20Heap&text=Unlike%20ordinary%20queues%2C%20a%20priority,highest%20priority%20can%20be%20fetched.
///Chose to reference a previous implementation due to time constraints on the original project.
///Decided to make it simpler using list.sort(), though likely less efficient than the source which uses a custom heap, depends on how sort is implemented
/// </summary>
/// <typeparam name="T">Generic type as queue member</typeparam>
public class PriorityQueue<T>   //Could use where T : class here to ensure nullability, but not hugely important since default == null for classes and can be returned
{
    List<T> queue = new List<T>();

    public int Count
    {
        get { return queue.Count; }
    }

    /// <summary>
    /// Push object to the priority queue and sort the queue
    /// </summary>
    /// <param name="obj">Object to push</param>
    public void Push(T obj)
    {
        queue.Add(obj);
        queue.Sort();
    }

    /// <summary>
    /// Pop object from queue
    /// </summary>
    public void Pop()
    {
        if (queue.Count == 0)
        {
            Debug.LogError("Queue is empty");
        }

        queue.Remove(queue[0]);

        //Don't need to resort since it should already be sorted, removing a value won't change that status
    }

    /// <summary>
    /// Get item on top of queue
    /// </summary>
    /// <returns>Item on top of the queue</returns>
    public T Top()
    {
        if (queue.Count == 0)
        {
            Debug.LogError("Queue is empty");
        }

        return queue[0];
    }

    /// <summary>
    /// Check if queue contains object
    /// </summary>
    /// <param name="obj">Object to look for</param>
    /// <returns>True if the obejct is in the queue</returns>
    public bool Contains(T obj)
    {
        return queue.Contains(obj);
    }

    /// <summary>
    /// Clears queue
    /// </summary>
    public void Clear()
    {
        queue.Clear();
    }

    /// <summary>
    /// Checks if queue is empty
    /// </summary>
    /// <returns>True if empty</returns>
    public bool Empty()
    {
        return queue.Count == 0;
    }

    /// <summary>
    /// Gets reference to List backing the queue
    /// Must be VERY careful toying with the list directly or you may violate some of its rules
    ///You can only get a copy of the list, that way if you want to set the list, we can ensure that it will be sorted
    /// </summary>
    /// <returns>Returns reference to list</returns>
    public List<T> GetList()
    {
        List<T> copyList = new List<T>(queue);
        return copyList;
    }

    /// <summary>
    /// Copies passed list and sorts it
    /// </summary>
    /// <param name="newList">List to copy</param>
    public void SetList(List<T> newList)
    {
        queue = new List<T>(newList);
        queue.Sort();
    }

    /// <summary>
    /// A little dirty, but this way, you can search the PriorityQueue for an AStarNode.indices, allows us to get and set AStarNodes based on their indices
    ///This is used for replacing nodes in PriorityQueues based on whether their F value is less or greater than another
    /// </summary>
    /// <param name="key">Key for indices</param>
    /// <returns>Returns AStarNode associated with indices</returns>
    public T this[Vector3Int key]
    {
        get
        {
            if (typeof(T) == typeof(AStarNode))
            {
                foreach (T node in queue)
                {
                    AStarNode current = node as AStarNode;

                    if (current.indices == key)
                    {
                        return node;
                    }
                }
            }

            return default; //null for classes, including AStar
        }

        set
        {
            if (typeof(T) == typeof(AStarNode))
            {
                foreach (T node in queue)
                {
                    AStarNode current = node as AStarNode;

                    if (current.indices == key)
                    {
                        queue.Remove(node);
                        Push(value);
                    }
                }
            }
        }
    }
}
