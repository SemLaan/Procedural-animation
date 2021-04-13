using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fabrik : MonoBehaviour
{
    [SerializeField] private int ChainLength = 2;
    [SerializeField] private Transform Target;
    [SerializeField] private Transform Pole;

    [Header("Solver Parameters")]
    [SerializeField] private int Iterations = 10;
    [SerializeField] private float Delta = 0.001f;


}
