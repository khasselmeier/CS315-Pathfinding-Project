using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Align : SteeringBehavior
{
    public Kinematic character;
    public GameObject target;

    float maxAngularAcceleration = 10f; // 5 (original comment) : degrees/second^2
    float maxRotation = 720f; // maxAngularVelocity : degrees/second

    // the radius for arriving at the target
    float targetRadius = 1f; // Considered aligned within this many degrees of target

    // the radius for beginning to slow down
    float slowRadius = 45f; // Begin slowing the turn this many degrees from facing target

    // the time over which to achieve target speed
    float timeToTarget = 0.1f;

    // returns the angle in degrees that we want to align with
    // Align will rotate to match the target's oriention
    // sub-classes can overwrite this function to set a different target angle e.g. to face a target
    public virtual float getTargetAngle()
    {
        return target.transform.eulerAngles.y;
    }

    public override SteeringOutput getSteering()
    {
        SteeringOutput result = new SteeringOutput();

        // get the naive direction to the target
        //float rotation = Mathf.DeltaAngle(character.transform.eulerAngles.y, target.transform.eulerAngles.y);
        float rotation = Mathf.DeltaAngle(character.transform.eulerAngles.y, getTargetAngle());
        float rotationSize = Mathf.Abs(rotation);

        // if we are outside the slow radius, then use maximum rotation
        float targetRotation = 0.0f;
        if (rotationSize > slowRadius)
        {
            targetRotation = maxRotation;
        }
        else // otherwise use a scaled rotation
        {
            targetRotation = maxRotation * rotationSize / slowRadius;
        }

        // the final targetRotation combines speed (already in the variable) and direction
        targetRotation *= rotation / rotationSize;

        // acceleration tries to get to the target rotation
        // something is breaking my angularVelocty... check if NaN and use 0 if so
        float currentAngularVelocity = float.IsNaN(character.angularVelocity) ? 0f : character.angularVelocity;
        result.angular = targetRotation - currentAngularVelocity;
        result.angular /= timeToTarget;

        // check if the acceleration is too great
        float angularAcceleration = Mathf.Abs(result.angular);
        if (angularAcceleration > maxAngularAcceleration)
        {
            result.angular /= angularAcceleration;
            result.angular *= maxAngularAcceleration;
        }

        result.linear = Vector3.zero;
        return result;
    }
}
