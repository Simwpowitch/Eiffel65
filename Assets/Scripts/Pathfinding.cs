﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class Pathfinding
{
    /// <summary>
    /// Main function to recieve a path of nodes to follow to get to the target
    /// </summary>
    public static Path GetPathToFollow(PathNode startPos, PathNode target, PathNode nodeToAvoid)
    {
        Path shortestPath = new Path();
        shortestPath.length = float.MaxValue;
        Path pathSoFar = new Path();

        PathNode[] pathOptions = startPos.GetPathNodes();
        pathOptions = SortNodesByDistance(pathOptions, target);
        for (int i = 0; i < pathOptions.Length; i++)
        {
            pathSoFar.nodes.Clear();
            pathSoFar.length = 0f;
            pathSoFar.nodes.Add(startPos);

            pathSoFar.nodes.Add(pathOptions[i]);
            float distanceToNode = Vector3.Distance(startPos.transform.position, pathOptions[i].transform.position);
            pathSoFar.length += distanceToNode;


            if (pathOptions[i] == target)
            {
                shortestPath.nodes.Add(target);
                break;
            }

            SearchNode(pathOptions[i], target, ref shortestPath, pathSoFar, nodeToAvoid);

            pathSoFar.nodes.RemoveAt(pathSoFar.nodes.Count - 1);
            pathSoFar.length -= distanceToNode;
        }
        shortestPath.nodes.Remove(startPos);
        return shortestPath;
    }

    /// <summary>
    /// Searches for a path to the target node and checks for already traversed nodes, as well as the shortest path in total distance;
    /// </summary>
    private static void SearchNode(PathNode posToSearchFrom, PathNode target, ref Path shortestPath, Path pathSoFar, PathNode nodeToAvoid)
    {
        PathNode[] pathOptions = posToSearchFrom.GetPathNodes();
        if (pathOptions.Length > 1)
        {
            pathOptions = SortNodesByDistance(pathOptions, target);
        }
        for (int i = 0; i < pathOptions.Length; i++)
        {
            if (pathSoFar.nodes.Contains(pathOptions[i]))
            {
                continue;
            }

            if (pathOptions[i] == nodeToAvoid)
            {
                continue;
            }

            pathSoFar.nodes.Add(pathOptions[i]);
            float distanceToNode = Vector3.Distance(posToSearchFrom.transform.position, pathOptions[i].transform.position);
            pathSoFar.length += distanceToNode;

            if (pathSoFar.length >= shortestPath.length)
            {
                pathSoFar.nodes.RemoveAt(pathSoFar.nodes.Count - 1);
                pathSoFar.length -= distanceToNode;
                continue;
            }


            if (pathOptions[i] == target)
            {
                Path newPath = new Path();

                for (int j = 0; j < pathSoFar.nodes.Count; j++)
                {
                    newPath.nodes.Add(pathSoFar.nodes[j]);
                }

                newPath.length = pathSoFar.length;

                if (newPath.length < shortestPath.length)
                {
                    shortestPath = newPath;
                }

                pathSoFar.nodes.RemoveAt(pathSoFar.nodes.Count - 1);
                pathSoFar.length -= distanceToNode;
                continue;
            }

            SearchNode(pathOptions[i], target, ref shortestPath, pathSoFar, nodeToAvoid);
            pathSoFar.nodes.RemoveAt(pathSoFar.nodes.Count - 1);
            pathSoFar.length -= distanceToNode;
        }
    }


    /// <summary>
    /// Sorts the nodes by the distance to the target
    /// </summary>
    private static PathNode[] SortNodesByDistance(PathNode[] nodesToSort, PathNode target)
    {
        PathNode[] sortedArray = new PathNode[nodesToSort.Length];
        sortedArray = nodesToSort.OrderBy(x => Vector3.Distance(target.transform.position, x.transform.position)).ToArray();
        return sortedArray;
    }
}



public class Path
{
    public List<PathNode> nodes;
    public float length;

    public Path()
    {
        nodes = new List<PathNode>();
        length = 0f;
    }
}