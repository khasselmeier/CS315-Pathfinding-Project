using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPath : Seek
{
    public GameObject[] path;
    private int currentPathIndex;
    public float targetRadius = 0.5f;

    public GameObject GetCurrentWaypoint()
    {
        if (path == null || path.Length == 0)
            return null;

        return path[currentPathIndex];
    }

    public override SteeringOutput getSteering()
    {
        if (path == null || path.Length == 0)
            return new SteeringOutput();

        // Assign target to current waypoint
        target = path[currentPathIndex];

        // Check if reached current waypoint
        float distanceToTarget = (target.transform.position - character.transform.position).magnitude;
        if (distanceToTarget < targetRadius)
        {
            currentPathIndex++;
            if (currentPathIndex >= path.Length)
            {
                currentPathIndex = path.Length - 1; // stop at final node
            }
        }
        return base.getSteering();
    }
}
