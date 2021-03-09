using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GradientDescentArmController : MonoBehaviour
{
    [Header("Transform references")]
    [SerializeField] private Transform armTarget;
    [SerializeField] private Transform hand;
    [SerializeField] private Transform[] joints;
    [Header("IK calculation variables")]
    [SerializeField] private float deltaRotation = 0.1f;
    [SerializeField] private float learningRate = 0.1f;
    [SerializeField] private float targetDistance = 0.1f;
    private float[] boneLengths;

    private Vector3[] JointRotations
    {
        get
        {
            Vector3[] rotations = new Vector3[joints.Length];
            for (int i = 0; i < rotations.Length; i++)
            {
                rotations[i] = joints[i].localRotation.eulerAngles;
            }
            return rotations;
        }
        set
        {
            for (int i = 0; i < joints.Length; i++)
            {
                joints[i].localRotation = Quaternion.Euler(value[i]);
            }
        }
    }

    private float CurrentDistanceFromTarget
    {
        get
        {
            return Vector3.Distance(hand.position, armTarget.position);
        }
    }

    private void Start()
    {
        boneLengths = new float[joints.Length];
        for (int i = 0; i < joints.Length; i++)
        {
            boneLengths[i] = joints[i].GetComponent<IKJoint>().boneLength;
        }
        Check();
    }

    private void Update()
    {
        Vector3[] gradients = CalculateGradients();
        UpdateRotations(gradients);
    }

    private Vector3[] CalculateGradients()
    {
        Vector3[] jointRotations = JointRotations;
        Vector3[] gradients = new Vector3[jointRotations.Length];

        if (CurrentDistanceFromTarget < targetDistance)
            return gradients;

        for (int i = 0; i < jointRotations.Length; i++)
        {
            //X
            jointRotations[i].x += deltaRotation;
            float updatedDistanceFromTarget = DistanceFromTarget(jointRotations, boneLengths);
            gradients[i].x = (updatedDistanceFromTarget - CurrentDistanceFromTarget) / deltaRotation;
            jointRotations = JointRotations;
            //Y
            jointRotations[i].y += deltaRotation;
            updatedDistanceFromTarget = DistanceFromTarget(jointRotations, boneLengths);
            gradients[i].y = (updatedDistanceFromTarget - CurrentDistanceFromTarget) / deltaRotation;
            jointRotations = JointRotations;
            //Z
            jointRotations[i].z += deltaRotation;
            updatedDistanceFromTarget = DistanceFromTarget(jointRotations, boneLengths);
            gradients[i].z = (updatedDistanceFromTarget - CurrentDistanceFromTarget) / deltaRotation;
            jointRotations = JointRotations;
        }
        return gradients;
    }

    private void UpdateRotations(Vector3[] gradients)
    {
        Vector3[] rotations = JointRotations;
        for (int i = 0; i < rotations.Length; i++)
        {
            rotations[i].x -= learningRate * gradients[i].x;
            rotations[i].y -= learningRate * gradients[i].y;
            rotations[i].z -= learningRate * gradients[i].z;
        }
        JointRotations = rotations;
    }

    private float DistanceFromTarget(Vector3[] rotations, float[] boneLenghts)
    {
        Vector3 armPosition = joints[0].position;
        Quaternion rotation = Quaternion.identity;
        // Calculating the position of the tip of the arm relative to the root of the arm 
        // and adding that to the position of the root of the arm
        for (int i = 0; i < rotations.Length; i++)
        {
            rotation *= Quaternion.Euler(rotations[i]);
            armPosition += rotation * (Vector3.right * boneLenghts[i]);
        }
        float distance = Vector3.Distance(armTarget.position, armPosition);
        return distance;
    }

    private float DistanceFromTarget(Vector3[] rotations, float[] boneLenghts, ref Quaternion finalJointRotation)
    {
        Vector3 armPosition = joints[0].position;
        Quaternion rotation = Quaternion.identity;
        // Calculating the position of the tip of the arm relative to the root of the arm 
        // and adding that to the position of the root of the arm
        for (int i = 0; i < rotations.Length; i++)
        {
            rotation *= Quaternion.Euler(rotations[i]);
            armPosition += rotation * (Vector3.right * boneLenghts[i]);
        }
        float distance = Vector3.Distance(armTarget.position, armPosition);
        finalJointRotation = rotation;
        return distance;
    }

    private void Check()
    {
        foreach (float length in boneLengths)
        {
            print("bone length: " + length);
        }
        foreach (Vector3 rotation in JointRotations)
        {
            print("Joint rotation: " + rotation);
        }
        print(joints[0].position);
        print("Distance from target: " + DistanceFromTarget(JointRotations, boneLengths));
        print("Distance from target check: " + CurrentDistanceFromTarget);
    }
}
