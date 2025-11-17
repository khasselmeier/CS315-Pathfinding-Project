using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPath : Seek
{
    public GameObject[] path;
    private int currentPathIndex = 0;

    [Header("Path Settings")]
    public float waypointRadius = .4f;     // When close enough to consider waypoint reached
    public float lookAheadDistance = .7f;  // Helps avoid corners

    // --- Added back so external scripts can still use it ---
    public GameObject GetCurrentWaypoint()
    {
        if (path == null || path.Length == 0)
            return null;

        return path[currentPathIndex];
    }

    public override SteeringOutput getSteering()
    {
        SteeringOutput result = new SteeringOutput();

        if (path == null || path.Length == 0)
            return result;

        GameObject currentWaypoint = GetCurrentWaypoint();

        // --- Look-ahead target to avoid obstacle corners ---
        Vector3 dir = (currentWaypoint.transform.position - character.transform.position).normalized;
        Vector3 lookAheadTargetPos = currentWaypoint.transform.position + (dir * lookAheadDistance);

        // Assign new offset target
        target = currentWaypoint;
        Vector3 targetPos = lookAheadTargetPos;

        float distance = Vector3.Distance(character.transform.position, currentWaypoint.transform.position);

        // --- Waypoint advancement ---
        if (distance < waypointRadius)
        {
            currentPathIndex++;

            if (currentPathIndex >= path.Length)
            {
                currentPathIndex = path.Length - 1;
                return new SteeringOutput();         // Stop at final target
            }

            currentWaypoint = GetCurrentWaypoint();
        }

        // --- Steering force ---
        result.linear = (targetPos - character.transform.position).normalized * 10f;  // maxAccel
        result.angular = 0;

        return result;
    }
}