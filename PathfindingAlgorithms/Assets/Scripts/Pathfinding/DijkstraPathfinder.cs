using System.Collections.Generic;
using UnityEngine;

public class DijkstraPathfinder : Kinematic
{
    [Header("Pathfinding Settings")]
    public Node StartNode;
    public Node EndNode;

    public LayerMask obstacleMask;

    Graph myGraph;
    FollowPath myMoveType;
    LookWhereGoing myRotateType;
    ObstacleAvoidance myAvoid;
    Arrive myArrive;
    GameObject[] myPath;

    private List<Node> path = new List<Node>();

    void Start()
    {
        // Rotation
        myRotateType = new LookWhereGoing();
        myRotateType.character = this;
        myRotateType.target = myTarget; //"Facing" target

        // Pathfinding
        myGraph = new Graph();
        myGraph.Build();
        List<Connection> path = Dijkstra.pathfind(myGraph, StartNode, EndNode);
        myPath = new GameObject[path.Count + 1];

        int i = 0;

        foreach (Connection c in path)
        {
            myPath[i] = c.getFromNode().gameObject;
            i++;
        }

        myPath[i] = EndNode.gameObject;

        // Follow
        myMoveType = new FollowPath();
        myMoveType.character = this;
        myMoveType.path = myPath;

        // Avoid
        myAvoid = new ObstacleAvoidance();
        myAvoid.character = this;
        myAvoid.obstacleMask = obstacleMask;
        myAvoid.lookAhead = 4f;
        myAvoid.avoidDistance = 4f;

        // Arrive
        myArrive = new Arrive();
        myArrive.character = this;
        myArrive.target = EndNode.gameObject;
    }

    protected override void Update()
    {
        steeringUpdate = new SteeringOutput();

        // Get current waypoint
        GameObject currentWaypoint = myMoveType.GetCurrentWaypoint();
        myAvoid.target = currentWaypoint;

        // Check for final goal
        bool atGoal = currentWaypoint == EndNode.gameObject &&
                      Vector3.Distance(transform.position, EndNode.transform.position) < 1.5f;

        if (atGoal)
        {
            // Stop moving
            steeringUpdate = myArrive.getSteering();
        }
        else
        {
            // Keep moving
            SteeringOutput pathSteering = myMoveType.getSteering();
            SteeringOutput avoidSteering = myAvoid.getSteering();

            bool isAvoiding = avoidSteering.linear.sqrMagnitude > 0.01f;

            float avoidWeight = isAvoiding ? .3f : 0f;
            float pathWeight = 1.0f;

            // Weight and normalize steering behaviors
            Vector3 combined = pathSteering.linear * pathWeight + avoidSteering.linear * avoidWeight;
            float maxAccel = 5f;
            steeringUpdate.linear = Vector3.ClampMagnitude(combined, maxAccel);

            steeringUpdate.angular = myRotateType.getSteering().angular;
        }
        base.Update();
    }

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
                //Debug.Log(unvisited + " nodes in list");
                if (distances[node] < minDist)
                {
                    minDist = distances[node];
                    current = node;
                }
            }

            if (current == null)
            {
                Debug.Log("Current is null");
                break;
            }


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