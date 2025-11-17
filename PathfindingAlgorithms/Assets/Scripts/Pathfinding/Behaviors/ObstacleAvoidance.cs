using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleAvoidance : Seek
{
    public float avoidDistance = 4f;
    public float lookAhead = 5f;
    public LayerMask obstacleMask;
    public int rayCount = 5;
    public float raySpread = 45f;

    protected override Vector3 getTargetPosition()
    {
        Vector3 direction = character.linearVelocity.sqrMagnitude > 0.01f ? character.linearVelocity.normalized : (target != null ? (target.transform.position - character.transform.position).normalized : character.transform.forward);

        Vector3 start = character.transform.position + Vector3.up * 0.5f; // small offset upward
        RaycastHit closestHit;
        bool hitSomething = false;
        float closestDistance = float.MaxValue;
        Vector3 bestAvoidTarget = Vector3.zero;

        Vector3 horizontalDir = new Vector3(direction.x, 0f, direction.z).normalized;

        // Sending out raycasts in a circle
        for (int i = 0; i < rayCount; i++)
        {
            // Evenly distribute rays across the spread
            float angle = -raySpread * 0.5f + (raySpread / (rayCount - 1)) * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * horizontalDir;

            RaycastHit hit;
            if (Physics.Raycast(start, dir, out hit, lookAhead, obstacleMask, QueryTriggerInteraction.Ignore))
            {
                // Ignore ground hits
                if (Vector3.Dot(hit.normal, Vector3.up) > 0.6f)
                    continue;

                Debug.DrawLine(start, hit.point, Color.red);

                if (hit.distance < closestDistance)
                {
                    hitSomething = true;
                    closestHit = hit;
                    closestDistance = hit.distance;
                    Debug.Log(hit.collider.gameObject.name + " was hit ");
                    Vector3 horizontalNormal = new Vector3(hit.normal.x, 0f, hit.normal.z).normalized;
                    bestAvoidTarget = hit.point + horizontalNormal * Mathf.Max(avoidDistance, 0.5f);
                    bestAvoidTarget.y = character.transform.position.y; // Y-axis fixed

                    
                }
            }
            else
            {
                Debug.DrawRay(start, dir * lookAhead, Color.green);
            }
        }

        if (hitSomething)
        {
            return bestAvoidTarget;
        }

        // No obstacle detected -> follow the base Seek target
        return base.getTargetPosition();
    }

    // Visual debugging
    void OnDrawGizmos()
    {
        if (character == null) return;

        Vector3 start = character.transform.position + Vector3.up * 0.5f;
        Vector3 forward = character.transform.forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(start, start + forward * lookAhead);

        // Optional: draw the ray spread
        for (int i = 0; i < rayCount; i++)
        {
            float angle = -raySpread * 0.5f + (raySpread / (rayCount - 1)) * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * forward;
            Gizmos.DrawLine(start, start + dir * lookAhead);
        }
    }
}
