using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleAvoidance : Seek
{
    [Header("Avoidance Settings")]
    public float avoidDistance = 2f;
    public float lookAhead = 1f;
    public LayerMask obstacleMask;

    [Header("Ray Settings")]
    public int rayCount = 20;
    public float raySpread = 60f;

    protected override Vector3 getTargetPosition()
    {
        // Determine movement direction
        Vector3 moveDir = character.linearVelocity.sqrMagnitude > 0.01f
            ? character.linearVelocity.normalized
            : (target != null ? (target.transform.position - character.transform.position).normalized
                              : character.transform.forward);

        Vector3 origin = character.transform.position + Vector3.up * 0.4f;

        bool hitSomething = false;
        float closestDist = float.MaxValue;
        RaycastHit closestHit = new RaycastHit();

        // Horizontal forward direction only
        Vector3 forward = new Vector3(moveDir.x, 0, moveDir.z).normalized;

        // Ray fan spread
        for (int i = 0; i < rayCount; i++)
        {
            float angle = -raySpread * 0.5f + (raySpread / (rayCount - 1)) * i;
            Vector3 rayDir = Quaternion.Euler(0, angle, 0) * forward;

            if (Physics.Raycast(origin, rayDir, out RaycastHit hit, lookAhead, obstacleMask, QueryTriggerInteraction.Ignore))
            {
                Debug.DrawLine(origin, hit.point, Color.red);

                // record closest obstacle
                if (hit.distance < closestDist)
                {
                    closestDist = hit.distance;
                    closestHit = hit;
                    hitSomething = true;
                }
            }
            else
            {
                Debug.DrawRay(origin, rayDir * lookAhead, Color.green);
            }
        }

        //  If an obstacle was hit -> compute avoidance
        if (hitSomething)
        {
            Vector3 hitNormal = closestHit.normal;
            hitNormal.y = 0;
            hitNormal.Normalize();

            // Compute slip direction (right or left) based on normal
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            float dot = Vector3.Dot(hitNormal, right);
            Vector3 slip = (dot > 0 ? -right : right); // choose safe side

            // slide along wall
            Vector3 avoidTarget = closestHit.point + slip * avoidDistance;
            avoidTarget.y = character.transform.position.y;

            // If avoidTarget is too close, push some forward motion
            Vector3 offset = avoidTarget - character.transform.position;
            if (offset.magnitude < 0.5f)
            {
                avoidTarget = character.transform.position
                              + (slip * avoidDistance * 0.7f)
                              + (forward * avoidDistance * 0.6f);
            }

            return avoidTarget;
        }

        // No obstacle detected -> use default Seek
        return base.getTargetPosition();
    }

    private void OnDrawGizmos()
    {
        if (character == null) return;

        Gizmos.color = Color.yellow;
        Vector3 origin = character.transform.position + Vector3.up * 0.4f;
        Gizmos.DrawLine(origin, origin + character.transform.forward * lookAhead);
    }
}