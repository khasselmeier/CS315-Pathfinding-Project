using System.Collections.Generic;
using UnityEngine;

public class DijkstraPathfinder : MonoBehaviour
{
    [Header("Pathfinding Settings")]
    public Node StartNode;  //these are assigned automatically by the PathfindingSpawner script
    public Node EndNode;

    private List<Node> path = new List<Node>();

    public void FindPath()
    {
        if (StartNode == null || EndNode == null)
        {
            Debug.LogWarning("StartNode or EndNode not assigned");
            return;
        }

        //dijkstra’s algorithm
        Dictionary<Node, float> distances = new Dictionary<Node, float>();
        Dictionary<Node, Node> previous = new Dictionary<Node, Node>();
        List<Node> unvisited = new List<Node>(FindObjectsOfType<Node>());

        //initialize distances
        foreach (Node node in unvisited)
            distances[node] = Mathf.Infinity;

        distances[StartNode] = 0f;

        while (unvisited.Count > 0)
        {
            //find node with smallest distance
            Node current = null;
            float minDist = Mathf.Infinity;

            foreach (Node node in unvisited)
            {
                if (distances[node] < minDist)
                {
                    minDist = distances[node];
                    current = node;
                }
            }

            if (current == null)
                break;

            //if we reached the end node, stop
            if (current == EndNode)
                break;

            unvisited.Remove(current);

            //evaluate neighbors
            foreach (Node neighbor in current.ConnectsTo)
            {
                if (neighbor == null || !unvisited.Contains(neighbor)) continue;

                float newDist = distances[current] + Vector3.Distance(current.transform.position, neighbor.transform.position);
                if (newDist < distances[neighbor])
                {
                    distances[neighbor] = newDist;
                    previous[neighbor] = current;
                }
            }
        }

        //reconstruct path
        path.Clear();
        Node step = EndNode;
        while (step != null && previous.ContainsKey(step))
        {
            path.Insert(0, step);
            step = previous.ContainsKey(step) ? previous[step] : null;
        }

        if (!path.Contains(StartNode))
            path.Insert(0, StartNode);

        Debug.Log($"Dijkstra Path found with {path.Count} nodes");
    }
}