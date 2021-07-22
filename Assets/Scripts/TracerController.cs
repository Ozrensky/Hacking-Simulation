using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TracerController : MonoBehaviour
{
    public int shortestDistance = 0;
    public Path shortestPath = new Path();
    Node tracerNode;
    Coroutine tracingCoroutine;
    public static List<TracerController> Instances = new List<TracerController>();

    private void Awake()
    {
        HackingController.OnLost += StopTracer;
        HackingController.OnWon += StopTracer;
        Instances.Add(this);
    }

    //Dijkstra algorithm
    public void FindShortestPath(Node start, Node end)
    {
        tracerNode = start;
        //getting all nodes from HackingController
        List<Node> spawnedNodes = new List<Node>();
        spawnedNodes.AddRange(HackingController.Instance.spawnedNodes);
        Path path = new Path();
        //list of unvisited nodes
        List<Node> unvisited = new List<Node>();
        //previous nodes in optimal path from source
        Dictionary<Node, Node> previous = new Dictionary<Node, Node>();

        //all nodes distances(difficulties)
        Dictionary<Node, int> distances = new Dictionary<Node, int>();

        //adding all nodes to unvisited and setting their distances to infinite
        for (int i = 0; i < spawnedNodes.Count; i++)
        {
            Node node = spawnedNodes[i];
            unvisited.Add(node);
            distances.Add(node, int.MaxValue);
        }

        //set starting node distance to zero
        distances[start] = 0;
        while (unvisited.Count != 0)
        {
            //ordering the unvisited list by distance, smallest distance at start and largest at end
            unvisited = unvisited.OrderBy(node => distances[node]).ToList();

            //getting the node with smallest distance
            Node current = unvisited[0];

            //remove the current node from unvisisted list
            unvisited.Remove(current);

            //if current node is end node, break
            if (current == end)
            {
                //construct the shortest path
                while (previous.ContainsKey(current))
                {
                    //insert the node onto the final result
                    path.nodes.Insert(0, current);
                    //traverse from start to end
                    current = previous[current];
                }

                //insert the source onto the final result
                path.nodes.Insert(0, current);
                break;
            }

            //looping through the node connections, where the connected node is still in unvisited list
            for (int i = 0; i < current.connections.Count; i++)
            {
                Node connectedNode = current.connections[i].connectedNode;

                //getting the distance (difficulty) of a connected node
                int distance = connectedNode.hackingDifficulty;

                //the distance from start node to connectedNode
                int alt = distances[current] + distance;

                //shorter path to connectedNode has been found
                if (alt < distances[connectedNode])
                {
                    distances[connectedNode] = alt;
                    previous[connectedNode] = current;
                }
            }
        }
        path.nodes.RemoveAt(0);
        path.CalculateDistance();
        shortestPath = path;
        //starts tracing
        StartTracing();
    }

    void StartTracing()
    {
        if (!HackingController.minigameFinished)
            tracingCoroutine = StartCoroutine(TracingCoroutine());
    }

    IEnumerator TracingCoroutine()
    {
        Node previousNode = tracerNode;
        Node currentNode;
        //going through nodes in shortest paths and hacks them until it reaches final node (start node)
        for (int i = 0; i < shortestPath.nodes.Count(); i++)
        {
            currentNode = shortestPath.nodes[i];
            yield return new WaitUntil(() => !currentNode.IsBeingHacked());
            if (currentNode.isTrap)
                yield return new WaitForSeconds(HackingController.Instance.settings.trapDelay);
            currentNode.TracerHack(previousNode);
            yield return new WaitUntil(() => !currentNode.IsBeingHacked());
            previousNode = shortestPath.nodes[i];
        }

        HackingController.Instance.MinigameFailed();
    }

    void StopTracer()
    {
        StopAllCoroutines();
        Instances.Remove(this);
        HackingController.OnLost -= StopTracer;
        HackingController.OnWon -= StopTracer;
        Destroy(this);
    }

    [System.Serializable]
    public class Path
    {
        public List<Node> nodes = new List<Node>();
        public int totalDistance = 0;

        public void CalculateDistance()
        {
            totalDistance = 0;
            foreach (Node n in nodes)
            {
                totalDistance += n.hackingDifficulty;
            }
        }
    }
}