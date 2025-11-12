using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject pathfinderPrefab;
    [SerializeField] private MazeUIPathController uiController;
    [SerializeField] private NodeGenerator nodeGenerator;

    private GameObject activePathfinder;

    public void SpawnPathfinder()
    {
        //make sure there are start and end points
        if (uiController.StartPointObject == null || uiController.EndPointObject == null)
        {
            Debug.LogWarning("[PathfindingSpawner] Start or End points not set");
            return;
        }

        StartCoroutine(SpawnAfterNodes());
    }

    private IEnumerator SpawnAfterNodes()
    {
        yield return new WaitForSeconds(0.1f); //small delay to ensure nodes are ready

        Node[] nodes = FindObjectsOfType<Node>();
        if (nodes.Length == 0)
        {
            Debug.LogWarning("[PathfindingSpawner] No nodes found in scene");
            yield break;
        }

        //find closest nodes to start/end objects
        Node startNode = FindClosestNode(uiController.StartPointObject.transform.position, nodes);
        Node endNode = FindClosestNode(uiController.EndPointObject.transform.position, nodes);

        if (startNode == null || endNode == null)
        {
            Debug.LogWarning("[PathfindingSpawner] Could not find valid Start or End node");
            yield break;
        }

        //spawn pathfinder at start node
        activePathfinder = Instantiate(pathfinderPrefab, startNode.transform.position, Quaternion.identity);
        DijkstraPathfinder pathfinder = activePathfinder.GetComponentInChildren<DijkstraPathfinder>();
        if (pathfinder == null)
        {
            Debug.LogError("[PathfindingSpawner] Pathfinder prefab does not contain a DijkstraPathfinder component");
            yield break;
        }

        pathfinder.StartNode = startNode;
        pathfinder.EndNode = endNode;

        //Debug.Log($"[PathfindingSpawner] Pathfinder spawned at {startNode.name} heading to {endNode.name}");

        //run Dijkstra
        pathfinder.FindPath();
    }

    private Node FindClosestNode(Vector3 position, Node[] allNodes)
    {
        Node closest = null;
        float minDist = Mathf.Infinity;

        foreach (Node node in allNodes)
        {
            float dist = Vector3.Distance(position, node.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = node;
            }
        }
        return closest;
    }
}