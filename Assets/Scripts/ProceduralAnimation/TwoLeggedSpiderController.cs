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
    [SerializeField] private float movementSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float positionCorrectionSpeed;
    [SerializeField] private float rotationCorrectionSpeed;
    [SerializeField] private float rotationCorrectionAngle;

    private void Awake()
    {
        //InitLegTargetPositions();
        AlignBody();
    }

    private void Update()
    {
        BodyMovement();
        LegMovement();
    }

    private void LegMovement()
    {
        leftLeg.position = transform.position;
        rightLeg.position = transform.position;
    }

    private void BodyMovement()
    {
        // Forwards and backwards movement
        if (Input.GetKey(KeyCode.UpArrow))
        {
            // Moving the body forward
            transform.position += transform.forward * movementSpeed * Time.deltaTime;
            // Correcting the position and rotation of the body so it stays parallel to the ground and at a good distance
            RaycastHit hit;
            CorrectPosition(out hit);
            CorrectRotation(hit);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            // Moving the body backward
            transform.position += -transform.forward * movementSpeed * Time.deltaTime;
            // Correcting the position and rotation of the body so it stays parallel to the ground and at a good distance
            RaycastHit hit;
            CorrectPosition(out hit);
            CorrectRotation(hit);
        }

        // Rotating left and right
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.rotation *= Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, Vector3.up);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.rotation *= Quaternion.AngleAxis(-rotationSpeed * Time.deltaTime, Vector3.up);
        }
    }

    private void CorrectPosition(out RaycastHit hit)
    {
        Ray rayDownFromBody = new Ray(transform.position, -transform.up);
        if (Physics.Raycast(rayDownFromBody, out hit, 100, groundLayer.value))
        {
            Vector3 targetPosition = hit.point + hit.normal * bodyToGroundDistance;
            float distanceToTargetPosition = Vector3.Distance(targetPosition, transform.position);
            if (distanceToTargetPosition < positionCorrectionSpeed * Time.deltaTime)
            {
                transform.position = targetPosition;
            }
            else
            {
                transform.position += (targetPosition - transform.position).normalized * positionCorrectionSpeed * Time.deltaTime;
            }
        }
    }

    private void CorrectRotation(RaycastHit hit)
    {
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        if (Mathf.Abs(Quaternion.Angle(targetRotation, transform.rotation)) > rotationCorrectionAngle)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationCorrectionSpeed * Time.deltaTime);
        }
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
