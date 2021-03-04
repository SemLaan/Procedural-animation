using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKJoint : MonoBehaviour
{
    [HideInInspector] public float boneLength;

    private void Awake()
    {
        boneLength = transform.GetChild(0).localScale.x;
    }
}
