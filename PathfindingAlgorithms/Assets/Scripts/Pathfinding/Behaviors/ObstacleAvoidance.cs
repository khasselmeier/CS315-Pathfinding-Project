using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleAvoidance : Seek
{
    public float avoidDistance = 4f;
    public float lookAhead = 5f;
    public LayerMask obstacleMask = 3;

    protected override Vector3 getTargetPosition()
    {
        /*RaycastHit hit;

        if (Physics.Raycast(character.transform.position, character.linearVelocity, out hit, lookAhead))
        {
            Debug.DrawRay(character.transform.position, character.linearVelocity.normalized * hit.distance, Color.red, 0.5f);
            return hit.point + (hit.normal * avoidDistance);
        }

        else
        {
            Debug.DrawRay(character.transform.position, character.linearVelocity.normalized * hit.distance, Color.green, 0.5f);
            return base.getTargetPosition();
        }*/

        Vector3 velocity = character.linearVelocity;
        if (velocity.sqrMagnitude < 0.001f)
            return base.getTargetPosition();

        Vector3 direction = velocity.normalized;
        RaycastHit hit;

        // Cast ray forward using normalized velocity
        if (Physics.Raycast(character.transform.position, direction, out hit, lookAhead, obstacleMask))
        {
            // Ignore ground hits (anything mostly horizontal)
            if (Vector3.Dot(hit.normal, Vector3.up) > 0.7f)
                return base.getTargetPosition();

            Debug.DrawRay(character.transform.position, direction * hit.distance, Color.red, 0.1f);
            return hit.point + (hit.normal * avoidDistance);
        }
        else
        {
            Debug.DrawRay(character.transform.position, direction * lookAhead, Color.green, 0.1f);
            return base.getTargetPosition();
        }
    }
}
