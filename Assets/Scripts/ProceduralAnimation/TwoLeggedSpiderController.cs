using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoLeggedSpiderController : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private Transform leftLegTarget;
    [SerializeField] private Transform rightLegTarget;
    [SerializeField] private Transform leftLeg;
    [SerializeField] private Transform rightLeg;
    [Header("Variables")]
    [SerializeField] private float bodyToGroundDistance;
    [SerializeField] private float legWidth;
    [SerializeField] private LayerMask groundLayer;

    private void Awake()
    {
        InitLegTargetPositions();
        AlignBody();
    }

    private void Update()
    {
        
    }

    private void InitLegTargetPositions()
    {
        RaycastHit leftLegRay = new RaycastHit();
        Physics.Raycast(transform.position + new Vector3(-legWidth, 0), Vector3.down, out leftLegRay, 100, groundLayer.value);
        RaycastHit rightLegRay = new RaycastHit();
        Physics.Raycast(transform.position + new Vector3(legWidth, 0), Vector3.down, out rightLegRay, 100, groundLayer.value);

        leftLegTarget.position = transform.position + new Vector3(-legWidth, -leftLegRay.distance);
        rightLegTarget.position = transform.position + new Vector3(legWidth, -rightLegRay.distance);
        leftLegTarget.rotation = Quaternion.Euler(0, 0, -90);
        rightLegTarget.rotation = Quaternion.Euler(0, 0, -90);
    }

    private void AlignBody()
    {
        Ray rayDownFromBody = new Ray(transform.position, -transform.up);
        RaycastHit hit = new RaycastHit();
        Physics.Raycast(rayDownFromBody, out hit, 100, groundLayer.value);
        transform.position = hit.point + hit.normal * bodyToGroundDistance;

        transform.rotation *= Quaternion.FromToRotation(transform.up, hit.normal);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        //Gizmos.DrawSphere(transform.position + new Vector3(-legWidth, -legHeight), 0.3f);
        //Gizmos.DrawSphere(transform.position + new Vector3(legWidth, -legHeight), 0.3f);
    }
}
