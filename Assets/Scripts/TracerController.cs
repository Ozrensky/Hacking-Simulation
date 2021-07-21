using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TracerController : MonoBehaviour
{
    public int shortestDistance = 0;
    public Path shortestPath = new Path();
    public List<Path> paths = new List<Path>();

    int calculationsCount = 0;

    public void CalculateTracerPath(Node node, Path path)
    {
        StartCoroutine(CalculationCoroutine(node, path));
    }

    IEnumerator CalculationCoroutine(Node node, Path path)
    {
        yield return new WaitUntil(()=> calculationsCount <= 1);
        calculationsCount++;
        foreach (Node.Connection c in node.connections)
        {
            Path newPath = new Path();
            newPath.nodes.AddRange(path.nodes);
            newPath.currentDistance = path.currentDistance;
            newPath.AddNode(c.connectedNode);
            if (c.connectedNode.type == Node.Type.start)
            {
                if (newPath.currentDistance < shortestDistance || shortestDistance == 0)
                {
                    shortestDistance = newPath.currentDistance;
                    shortestPath = newPath;
                }
                paths.Add(newPath);
                continue;
            }
            else if (shortestDistance == 0 || !(path.currentDistance > shortestDistance))
            {
                CalculateTracerPath(c.connectedNode, newPath);
            }
        }
        path = null;
        calculationsCount--;
        yield return null;
    }

    [System.Serializable]
    public class Path
    {
        public List<Node> nodes = new List<Node>();
        public int currentDistance = 0;

        public void AddNode(Node n)
        {
            nodes.Add(n);
            if (n.type != Node.Type.start)
                currentDistance += n.hackingDifficulty;
        }
    }
}
