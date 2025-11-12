using UnityEngine;

public class PathfindingSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject pathfinderPrefab;
    [SerializeField] private MazeUIPathController uiController; //reference to UI script

    private GameObject currentPathfinder;

    public void SpawnPathfinder()
    {
        //get the current start/end GameObjects from the UI controller
        GameObject startObj = uiController.StartPointObject;
        GameObject endObj = uiController.EndPointObject;

        if (startObj == null || endObj == null)
        {
            //Debug.LogWarning("Start or End point not set");
            return;
        }

        //get Node components or find nearest nodes
        Node startNode = startObj.GetComponent<Node>();
        Node endNode = endObj.GetComponent<Node>();

        if (startNode == null) startNode = FindClosestNode(startObj.transform.position);
        if (endNode == null) endNode = FindClosestNode(endObj.transform.position);

        if (startNode == null || endNode == null)
        {
            Debug.LogWarning("Start or End objects do not contain Node components and no nearby nodes were found");
            return;
        }

        //remove old pathfinder if one exists
        if (currentPathfinder != null)
            Destroy(currentPathfinder);

        //spawn new pathfinder
        currentPathfinder = Instantiate(pathfinderPrefab, startNode.transform.position, Quaternion.identity);

        //get the DijkstraPathfinder (even if it’s on a child)
        DijkstraPathfinder pathfinder = currentPathfinder.GetComponentInChildren<DijkstraPathfinder>();
        if (pathfinder == null)
        {
            Debug.LogError("Pathfinder Prefab does not have a DijkstraPathfinder component script");
            return;
        }

        //set start/end nodes and run the algorithm
        pathfinder.StartNode = startNode;
        pathfinder.EndNode = endNode;
        pathfinder.FindPath();
    }

    //finds the nearest Node if the object isn't one itself
    private Node FindClosestNode(Vector3 position)
    {
        Node[] allNodes = FindObjectsOfType<Node>();
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