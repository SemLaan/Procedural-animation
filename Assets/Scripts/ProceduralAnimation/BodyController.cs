using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RotationDirection
{
    left,
    right,
}

public class BodyController : MonoBehaviour
{
    [Header("Body movement variables")]
    [SerializeField] private float bodyToGroundDistance;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float positionCorrectionSpeed;
    [SerializeField] private float rotationCorrectionSpeed;
    [SerializeField] private float rotationCorrectionAngle;

    public void MoveBody(Vector3 direction, float deltaTime, float speedMultiplier = 1)
    {
        // Moving the body
        transform.position += direction.normalized * (movementSpeed * speedMultiplier * deltaTime);
        // Correcting the position and rotation of the body so it stays parallel to the ground and at a good distance
        RaycastHit hit;
        CorrectPosition(out hit);
        CorrectRotation(hit);
    }

    public void RotateBody(RotationDirection rotationDirection)
    {
        if (rotationDirection == RotationDirection.right)
        {
            transform.rotation *= Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, Vector3.up);
        }
        else
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
        // hit is the raycast hit that hit the ground below the creature
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        if (Mathf.Abs(Quaternion.Angle(targetRotation, transform.rotation)) > rotationCorrectionAngle)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationCorrectionSpeed * Time.deltaTime);
        }
    }
}
