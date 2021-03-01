using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoObjectIK : MonoBehaviour
{
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
        // Calculate distance between the target position and the root of the arm
        float distanceToTarget = Vector2.Distance(joint0.position, armTarget.position);

        // If the target is further than the arm can reach
        if (distanceToTarget > lengthBone0 + lengthBone1)
        {
            float angle0 = Mathf.Atan2(armTarget.position.y - joint0.position.y, armTarget.position.x - joint0.position.x);
            float angle0degrees = angle0 * Mathf.Rad2Deg;

            joint0.rotation = Quaternion.Euler(0, 0, angle0degrees);
            joint1.localRotation = Quaternion.Euler(0, 0, 0);

        } else
        {
            float distanceToTargetSquared = distanceToTarget * distanceToTarget;
            float lengthBone0Squared = lengthBone0 * lengthBone0;
            float lengthBone1Squared = lengthBone1 * lengthBone1;

            // Calculate angle for joint 0
            float beef = Mathf.Atan2(armTarget.position.y - joint0.position.y, armTarget.position.x - joint0.position.x);
            float angle0 = Mathf.Acos((distanceToTargetSquared + lengthBone0Squared - lengthBone1Squared)
                                        / (2 * distanceToTarget * lengthBone0)) + beef;
            float angle0degrees = angle0 * Mathf.Rad2Deg;


            // Calculate angle for joint 1
            float angle1 = Mathf.Acos((lengthBone1Squared + lengthBone0Squared - distanceToTargetSquared)
                                        / (2 * lengthBone1 * lengthBone0)) - Mathf.PI;
            float angle1degrees = angle1 * Mathf.Rad2Deg;

            joint0.rotation = Quaternion.Euler(0, 0, angle0degrees);
            joint1.localRotation = Quaternion.Euler(0, 0, angle1degrees);
        }
    }
}
