using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kinematic : MonoBehaviour
{
    public Vector3 linearVelocity;
    public float angularVelocity;  // Millington calls this rotation
    // because I'm attached to a gameobject, we also have:
    // rotation <<< Millington calls this orientation
    // position
    public float maxSpeed = 10.0f;
    public float maxAngularVelocity = 45.0f; // degrees

    public GameObject myTarget;

    public LayerMask collisionMask;

    // small offset to avoid clipping into walls
    private const float skinWidth = 0.05f;

    // child classes will get new steering data for use in our update function
    protected SteeringOutput steeringUpdate;

    // Start is called before the first frame update
    void Start()
    {
        steeringUpdate = new SteeringOutput(); // default to nothing. should be overriden by children
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        // something is breaking my angular velocity
        // check here and reset it if it broke
        if (float.IsNaN(angularVelocity))
        {
            angularVelocity = 0.0f;
        }

        Vector3 displacement = linearVelocity * Time.deltaTime;
        Vector3 start = transform.position;
        Vector3 end = start + displacement;

        for (int i = 0; i < 3; i++)
        {
            if (displacement.sqrMagnitude < 0.000001f)
                break;

            start = transform.position;
            end = start + displacement;

            if (Physics.Linecast(start, end, out RaycastHit hit, collisionMask, QueryTriggerInteraction.Ignore))
            {
                // Move to just outside the hit point
                transform.position = hit.point + hit.normal * skinWidth;

                // Remove velocity pushing into the obstacle
                linearVelocity -= Vector3.Project(linearVelocity, hit.normal);

                // Recompute remaining displacement with the updated velocity
                displacement = linearVelocity * Time.deltaTime;
            }
            else
            {
                // No collision then apply movement normally
                transform.position = end;
                break;
            }
        }

        if (Mathf.Abs(angularVelocity) > 0.01f)
        {
            Vector3 v = new Vector3(0, angularVelocity, 0);
            transform.eulerAngles += v * Time.deltaTime;
        }

        // update linear and angular velocities - I might be accelerating or decelerating, etc.
        // Millington p. 58, lines 11-13
        if (steeringUpdate != null)
        {
            linearVelocity += steeringUpdate.linear * Time.deltaTime;
            angularVelocity += steeringUpdate.angular * Time.deltaTime;
        }

        // check for speeding and clip
        // Millington p.58, lines 15-18
        // note that Millington's pseudocode on p.58 does not clip angular velocity, but we do here
        if (linearVelocity.magnitude > maxSpeed)
        {
            linearVelocity = linearVelocity.normalized * maxSpeed;
        }

        if (Mathf.Abs(angularVelocity) > maxAngularVelocity)
        {
            angularVelocity = maxAngularVelocity * Mathf.Sign(angularVelocity);
        }
    }
}
