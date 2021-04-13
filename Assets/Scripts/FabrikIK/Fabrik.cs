using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fabrik : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform pole;

    [Header("Solver Parameters")]
    [SerializeField] private int iterations = 10;
    [SerializeField] private float delta = 0.001f;

    private int chainLength;
    private float[] boneLengths;
    private float completeBoneLength;
    private Transform[] joints;
    private Vector3[] jointPositions;
    private Vector3[] startDirectionSucc;
    private Quaternion[] startRotationBone;
    private Quaternion startRotationTarget;
    private Transform endEffector;

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
        startDirectionSucc = new Vector3[chainLength + 1];
        startRotationBone = new Quaternion[chainLength + 1];

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
    }

    private void ResolveIK()
    {
        // Getting current joint positions
        for (int i = 0; i < joints.Length; i++)
            jointPositions[i] = joints[i].position;
        print("beef");
        // Calculating new joint positions
        if((target.position - endEffector.position).sqrMagnitude >= completeBoneLength * completeBoneLength)
        {
            print("beef2");
            // Stretch the chain in the direction of the target
            var direction = (target.position - endEffector.position).normalized;
            // Update joint positions except for the root joint's position
            for (int i = 1; i < jointPositions.Length; i++)
                jointPositions[i] = jointPositions[i - 1] + direction * boneLengths[i - 1];
        }

        // Updating joint positions
        for (int i = 0; i < joints.Length; i++)
            joints[i].position = jointPositions[i];
    }
}