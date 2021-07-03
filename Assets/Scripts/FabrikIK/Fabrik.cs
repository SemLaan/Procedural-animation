using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fabrik : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform pole;

    [Header("Solver Parameters")]
    [SerializeField] private int iterations = 10;
    [SerializeField] private float targetSize = 0.001f;

    private int chainLength;
    private float[] boneLengths;
    private float completeBoneLength;
    private Transform[] joints;
    private Vector3[] jointPositions;
    private Vector3[] initialJointDirections;
    private Quaternion[] initialJointRotations;
    private Quaternion initialTargetRotation;
    private Transform endEffector;
    private Transform root;

    private void Awake()
    {
        Init();
    }

    private void LateUpdate()
    {
        ResolveIK();
    }

    private void Init()
    {
        // Find end effector and chain length
        chainLength = 0;
        root = transform;
        endEffector = transform;
        while (true)
        {
            Transform child;
            try
            {
                child = endEffector.GetChild(0);
            }
            catch
            {
                break;
            }
            endEffector = child;
            chainLength++;
        }

        joints = new Transform[chainLength + 1];
        jointPositions = new Vector3[chainLength + 1];
        boneLengths = new float[chainLength];
        initialJointDirections = new Vector3[chainLength + 1];
        initialJointRotations = new Quaternion[chainLength + 1];

        // Put the bones in an array
        Transform currentJoint = transform;
        for (int i = 0; i < joints.Length; i++)
        {
            joints[i] = currentJoint;
            if (i != joints.Length-1)
                currentJoint = currentJoint.GetChild(0);
        }

        // Calculate total chain length and saving the lenghts of the individual bones
        completeBoneLength = 0;
        for (int i = 0; i < chainLength; i++)
        {
            float currentBoneLength = Vector3.Distance(joints[i].position, joints[i + 1].position);
            completeBoneLength += currentBoneLength;
            boneLengths[i] = currentBoneLength;
        }

        // Saving information for rotating the joints in the correct direction
        initialTargetRotation = target.rotation;
        for (int i = 0; i < joints.Length; i++)
        {
            initialJointRotations[i] = joints[i].rotation;
            if (i == joints.Length - 1)
            {
                initialJointDirections[i] = target.position - joints[i].position;
            }
            else
            {
                initialJointDirections[i] = joints[i + 1].position - joints[i].position;
            }
        }
    }

    private void ResolveIK()
    {
        // Getting current joint positions
        for (int i = 0; i < joints.Length; i++)
            jointPositions[i] = joints[i].position;

        // Calculating new joint positions
        if ((target.position - root.position).sqrMagnitude >= completeBoneLength * completeBoneLength)
        {
            // Stretch the chain in the direction of the target
            Vector3 direction = (target.position - root.position).normalized;
            // Update joint positions except for the root joint's position
            for (int i = 1; i < jointPositions.Length; i++)
                jointPositions[i] = jointPositions[i - 1] + direction * boneLengths[i - 1];
        }
        else
        {
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                // Backwards
                for (int i = jointPositions.Length - 1; i > 0; i--)
                {
                    // Set the end effector to the target position
                    if (i == jointPositions.Length - 1)
                    {
                        jointPositions[i] = target.position;
                    }
                    else
                    {
                        // Slide the current joint towards or away from the next joint in line so that 
                        // they are the correct distance away from each other
                        // The correct distance being the length of the bone between those joints
                        jointPositions[i] = jointPositions[i + 1] + (jointPositions[i] - jointPositions[i + 1]).normalized * boneLengths[i];
                    }
                }

                // Forwards
                // This does the same thing as the backwards loop except it works from the root instead of the end effector
                // The root does not have to be set back to the root position in this case because in the previous loop the root wasn't touched
                for (int i = 1; i < jointPositions.Length; i++)
                {
                    jointPositions[i] = jointPositions[i - 1] + (jointPositions[i] - jointPositions[i - 1]).normalized * boneLengths[i - 1];
                }

                // If the end effector is now close enough to the target: skip the remaining iterations
                if ((jointPositions[jointPositions.Length - 1] - target.position).sqrMagnitude < targetSize * targetSize)
                {
                    break;
                }
            }
        }

        // Rotating all joints except for the root and end effector towards the pole
        for (int i = 1; i < jointPositions.Length - 1; i++)
        {
            Plane plane = new Plane(jointPositions[i + 1] - jointPositions[i - 1], jointPositions[i - 1]);
            Vector3 poleProjection = plane.ClosestPointOnPlane(pole.position);
            Vector3 jointProjection = plane.ClosestPointOnPlane(jointPositions[i]);
            float angle = Vector3.SignedAngle(jointProjection - jointPositions[i - 1], poleProjection - jointPositions[i - 1], plane.normal);
            jointPositions[i] = (Quaternion.AngleAxis(angle, plane.normal) * (jointPositions[i] - jointPositions[i - 1])) + jointPositions[i - 1];
        }

        // Updating joint positions
        for (int i = 0; i < joints.Length; i++)
        {
            if (i == joints.Length - 1)
            {
                joints[i].rotation = Quaternion.Inverse(target.rotation) * initialTargetRotation * Quaternion.Inverse(initialJointRotations[i]);
            }
            else
            {
                joints[i].rotation = Quaternion.FromToRotation(initialJointDirections[i], jointPositions[i + 1] - jointPositions[i]) * Quaternion.Inverse(initialJointRotations[i]);
            }
            joints[i].position = jointPositions[i];
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(target.position, targetSize);
        }
    }
}
