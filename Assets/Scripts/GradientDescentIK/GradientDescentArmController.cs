using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GradientDescentArmController : MonoBehaviour
{
    [Header("Transform references")]
    [SerializeField] private Transform armTarget;
    [SerializeField] private Transform hand;
    [SerializeField] private Transform rootJoint;
    private Transform[] joints;
    [Header("IK calculation variables")]
    [SerializeField] private float deltaRotation = 0.1f;
    [SerializeField] private float targetDistance = 0.1f;
    [SerializeField] private float learningRate = 0.1f;
    [SerializeField] private float distanceImpact = 0.1f;
    [SerializeField] private float rotationImpact = 0.1f;
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

    private float CurrentRotationDifferenceFromTarget
    {
        get
        {
            return Mathf.Abs(Quaternion.Angle(hand.rotation, armTarget.rotation) / 180);
        }
    }

    private void Awake()
    {
        Transform currentJoint = rootJoint;
        List<Transform> jointList = new List<Transform>();
        jointList.Add(currentJoint);

        while (true)
        {
            try
            {
                currentJoint = currentJoint.GetChild(1);
                jointList.Add(currentJoint);
            }
            catch
            {
                break;
            }            
        }
        jointList.RemoveAt(jointList.Count - 1);
        joints = jointList.ToArray();
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
        for (int i = 0; i < 10; i++)
        {
            Vector3[] gradients = CalculateGradients();
            UpdateRotations(gradients);
        }
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
            float xGradient = CalculatePartialGradient(jointRotations, i, Vector3.right);
            gradients[i].x = xGradient;
            jointRotations = JointRotations;
            //Y
            float yGradient = CalculatePartialGradient(jointRotations, i, Vector3.up);
            gradients[i].y = yGradient;
            jointRotations = JointRotations;
            //Z
            float zGradient = CalculatePartialGradient(jointRotations, i, Vector3.forward);
            gradients[i].z = zGradient;
            jointRotations = JointRotations;
        }
        return gradients;
    }

    private float CalculatePartialGradient(Vector3[] jointRotations, int joint, Vector3 axis)
    {
        jointRotations[joint] += deltaRotation * axis;

        Quaternion finalJointRotation = new Quaternion();
        
        float updatedDistanceFromTarget = DistanceFromTarget(jointRotations, boneLengths, ref finalJointRotation);
        float distanceGradient = (updatedDistanceFromTarget - CurrentDistanceFromTarget) / deltaRotation;

        float updatedRotationDifferenceFromTarget = Mathf.Abs(Quaternion.Angle(finalJointRotation, armTarget.rotation) / 180);
        float rotationGradient = (updatedRotationDifferenceFromTarget - CurrentRotationDifferenceFromTarget) / deltaRotation;
        float totalGradient = distanceGradient * distanceImpact + rotationGradient * rotationImpact;
        return totalGradient;
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
        print("hand rotation: " + hand.rotation.eulerAngles);
        print("target rotation: " + armTarget.rotation.eulerAngles);
        print("angle between rotations: " + Quaternion.Angle(hand.rotation, armTarget.rotation));
        print("Rotation difference: " + CurrentRotationDifferenceFromTarget);
    }
}
