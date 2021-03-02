using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoObjectIK : MonoBehaviour
{
    [SerializeField] private Transform rootObject;
    [SerializeField] private Transform joint0;
    [SerializeField] private Transform joint1;
    [SerializeField] private Transform hand;
    [SerializeField] private Transform armTarget;

    private float lengthBone0;
    private float lengthBone1;

    private void Start()
    {
        // Calculate the length of the bones
        lengthBone0 = Vector2.Distance(joint0.position, joint1.position);
        lengthBone1 = Vector2.Distance(joint1.position, hand.position);
    }

    private void Update()
    {
        // Calculate the y rotation
        float yRotation = Mathf.Rad2Deg * Mathf.Atan2(joint0.localPosition.z - armTarget.localPosition.z, joint0.localPosition.x - armTarget.localPosition.x);
        yRotation = 180 - yRotation;

        // Rotate the target position to the XY plane so that the z rotations of the joints can be calculated in 2D
        Vector2 targetPosition = Quaternion.Euler(0, -yRotation, 0) * armTarget.localPosition;

        // Calculate distance between the target position and the root of the arm
        float distanceToTarget = Vector2.Distance(joint0.localPosition, targetPosition);

        // If the target is further than the arm can reach
        if (distanceToTarget > lengthBone0 + lengthBone1)
        {
            float angle0 = Mathf.Atan2(targetPosition.y - joint0.localPosition.y, targetPosition.x - joint0.localPosition.x);
            float angle0degrees = angle0 * Mathf.Rad2Deg;

            joint0.localRotation = Quaternion.Euler(0, yRotation, angle0degrees);
            joint1.localRotation = Quaternion.Euler(0, 0, 0);

        } else
        {
            float distanceToTargetSquared = distanceToTarget * distanceToTarget;
            float lengthBone0Squared = lengthBone0 * lengthBone0;
            float lengthBone1Squared = lengthBone1 * lengthBone1;

            // Calculate angle for joint 0
            float beef = Mathf.Atan2(targetPosition.y - joint0.localPosition.y, targetPosition.x - joint0.localPosition.x);
            float angle0 = Mathf.Acos((distanceToTargetSquared + lengthBone0Squared - lengthBone1Squared)
                                        / (2 * distanceToTarget * lengthBone0)) + beef;
            float angle0degrees = angle0 * Mathf.Rad2Deg;


            // Calculate angle for joint 1
            float angle1 = Mathf.Acos((lengthBone1Squared + lengthBone0Squared - distanceToTargetSquared)
                                        / (2 * lengthBone1 * lengthBone0)) - Mathf.PI;
            float angle1degrees = angle1 * Mathf.Rad2Deg;

            joint0.localRotation = Quaternion.Euler(0, yRotation, angle0degrees);
            joint1.localRotation = Quaternion.Euler(0, 0, angle1degrees);
        }
    }
}
