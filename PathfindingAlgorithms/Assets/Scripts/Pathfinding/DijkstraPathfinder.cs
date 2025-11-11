using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DijkstraPathfinder : Kinematic
{
    public Node start;
    public Node goal;

    Graph myGraph;
    FollowPath myMoveType;
    LookWhereGoing myRotateType;
    ObstacleAvoidance myAvoid;
    GameObject[] myPath;

    void Start()
    {
        myRotateType = new LookWhereGoing();
        myRotateType.character = this;
        myRotateType.target = myTarget;

        Graph myGraph = new Graph();
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

        myMoveType = new FollowPath();
        myMoveType.character = this;
        myMoveType.path = myPath;

        myAvoid = new ObstacleAvoidance();
        myAvoid.character = this;      
        myAvoid.target = myTarget;     
        myAvoid.lookAhead = 5f;       
        myAvoid.avoidDistance = 4f;     
    }

    protected override void Update()
    {
        //steeringUpdate = new SteeringOutput();
        //steeringUpdate.angular = myRotateType.getSteering().angular;
        //steeringUpdate.linear = myMoveType.getSteering().linear;
        //base.Update();

        /*steeringUpdate = new SteeringOutput();

        // Get individual steering behaviors
        SteeringOutput pathSteering = myMoveType.getSteering();
        SteeringOutput avoidSteering = myAvoid.getSteering();
        SteeringOutput rotateSteering = myRotateType.getSteering();

        // Combine obstacle avoidance + path following
        steeringUpdate.linear = pathSteering.linear + avoidSteering.linear;
        steeringUpdate.angular = rotateSteering.angular;

        base.Update();*/

        steeringUpdate = new SteeringOutput();

        SteeringOutput pathSteering = myMoveType.getSteering();
        SteeringOutput avoidSteering = myAvoid.getSteering();

        // Blend them together (weighted)
        float avoidWeight = 1.5f; 
        float pathWeight = 1.0f;   

        steeringUpdate.linear = (pathSteering.linear * pathWeight + avoidSteering.linear * avoidWeight).normalized;

        steeringUpdate.angular = myRotateType.getSteering().angular;

        base.Update();
    }
}
