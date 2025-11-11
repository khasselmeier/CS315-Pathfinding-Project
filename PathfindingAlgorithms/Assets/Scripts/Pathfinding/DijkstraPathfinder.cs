using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DijkstraPathfinder : Kinematic
{
    public Node start; //Start point
    public Node goal; //End point

    public LayerMask obstacleMask;

    Graph myGraph;
    FollowPath myMoveType;
    LookWhereGoing myRotateType;
    ObstacleAvoidance myAvoid;
    Arrive myArrive;
    GameObject[] myPath;

    void Start()
    {
        // Rotation
        myRotateType = new LookWhereGoing();
        myRotateType.character = this;
        myRotateType.target = myTarget; //"Facing" target

        // Pathfinding
        myGraph = new Graph();
        myGraph.Build();
        List<Connection> path = Dijkstra.pathfind(myGraph, start, goal);
        myPath = new GameObject[path.Count + 1];

        int i = 0;

        foreach (Connection c in path)
        {
            myPath[i] = c.getFromNode().gameObject;
            i++;
        }

        myPath[i] = goal.gameObject;

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
        myArrive.target = goal.gameObject;
    }

    protected override void Update()
    {
        steeringUpdate = new SteeringOutput();

        // Get current waypoint
        GameObject currentWaypoint = myMoveType.GetCurrentWaypoint();
        myAvoid.target = currentWaypoint;

        // Check for final goal
        bool atGoal = currentWaypoint == goal.gameObject &&
                      Vector3.Distance(transform.position, goal.transform.position) < 1.5f;

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

            float avoidWeight = isAvoiding ? 1.5f : 0f;
            float pathWeight = 1.0f;

            // Weight and normalize steering behaviors
            Vector3 combined = pathSteering.linear * pathWeight + avoidSteering.linear * avoidWeight;
            float maxAccel = 5f;
            steeringUpdate.linear = Vector3.ClampMagnitude(combined, maxAccel);

            steeringUpdate.angular = myRotateType.getSteering().angular;
        }
        base.Update();
    }
}
