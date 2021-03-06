using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoLeggedSpiderController : MonoBehaviour
{
    private enum Leg
    {
        none,
        right,
        left,
    }

    [Header("Objects")]
    [SerializeField] private Transform leftLegTarget;
    [SerializeField] private Transform rightLegTarget;
    [SerializeField] private Transform leftLeg;
    [SerializeField] private Transform rightLeg;
    [Header("Body movement variables")]
    [SerializeField] private float bodyToGroundDistance;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float positionCorrectionSpeed;
    [SerializeField] private float rotationCorrectionSpeed;
    [SerializeField] private float rotationCorrectionAngle;
    [Header("Leg movement variables")]
    [SerializeField] private float maxLegDistance;
    [SerializeField] private float legWidth;
    [SerializeField] private float legSteppingDistance;
    [SerializeField] private float legMoveTime;
    [SerializeField] private float stepHeight;

    private Vector3 movementDirection;
    private int rotationDirection;
    private Leg movingLeg = Leg.none;
    private (Vector3 position, Quaternion rotation) newLegTarget;
    private float legMoveStartTime;
    private (Vector3 position, Quaternion rotation) oldLegTarget;


    private void Update()
    {
        BodyMovement();
        LegMovement();
    }

    private void LegMovement()
    {
        leftLeg.position = transform.position;
        rightLeg.position = transform.position;

        // Checking if any of the legs should move
        if (movingLeg == Leg.none)
        {
            float rightLegAngleToBody = Vector3.SignedAngle(transform.forward, 
                    Vector3.ProjectOnPlane(rightLegTarget.position - transform.position, transform.up), transform.up);
            float leftLegAngleToBody = Vector3.SignedAngle(transform.forward,
                    Vector3.ProjectOnPlane(leftLegTarget.position - transform.position, transform.up), transform.up);
            // If the left leg has fallen behind
            if (Vector3.Distance(transform.position, leftLegTarget.position) > maxLegDistance)
            {
                InitiateStep(Leg.left);
            }
            // If the right leg has fallen behind
            else if (Vector3.Distance(transform.position, rightLegTarget.position) > maxLegDistance)
            {
                InitiateStep(Leg.right);
            }
            // If both legs are starting to fall behind
            else if (rightLegAngleToBody > 90 && leftLegAngleToBody < -90)
            {
                if (Vector3.Distance(transform.position, leftLegTarget.position) > Vector3.Distance(transform.position, rightLegTarget.position))
                    InitiateStep(Leg.left);
                else
                    InitiateStep(Leg.right);
            } 
            // If the right leg is not on the right side of the body anymore
            else if (rightLegAngleToBody < 0)
            {
                InitiateStep(Leg.right);
            }
            // If the left leg is not on the left side of the body anymore
            else if (leftLegAngleToBody > 0)
            {
                InitiateStep(Leg.left);
            }
        }
        else // Moving the leg that is currently making a step
        {
            MoveSteppingLeg();
        }
    }

    private void MoveSteppingLeg()
    {
        Transform currentMovingLeg = rightLegTarget;
        if (movingLeg == Leg.left)
            currentMovingLeg = leftLegTarget;

        float moveProgress = (Time.time - legMoveStartTime) / legMoveTime;
        if (moveProgress > 1)
        {
            moveProgress = 1;
            movingLeg = Leg.none;
        }
        currentMovingLeg.position = Vector3.Lerp(oldLegTarget.position, newLegTarget.position, moveProgress)
                + transform.up * stepHeight * Mathf.Sin(moveProgress * Mathf.PI);
        currentMovingLeg.rotation = Quaternion.Lerp(oldLegTarget.rotation, newLegTarget.rotation, moveProgress);
    }

    private void InitiateStep(Leg leg)
    {
        movingLeg = leg;
        legMoveStartTime = Time.time;
        if (leg == Leg.right)
        {
            newLegTarget = CalculateNewLegPositionAndRotation(transform.right);
            oldLegTarget = (rightLegTarget.position, rightLegTarget.rotation);
        } else if (leg == Leg.left)
        {
            newLegTarget = CalculateNewLegPositionAndRotation(-transform.right);
            oldLegTarget = (leftLegTarget.position, leftLegTarget.rotation);
        }
    }

    private (Vector3, Quaternion) CalculateNewLegPositionAndRotation(Vector3 legDirection)
    {
        Vector3 rayOrigin = transform.position + legDirection * legWidth + movementDirection * legSteppingDistance;
        Ray ray = new Ray(rayOrigin, -transform.up);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 100, groundLayer.value);

        return (hit.point, Quaternion.FromToRotation(Vector3.right, -hit.normal));
    }

    private void BodyMovement()
    {
        // Forwards and backwards movement
        if (Input.GetKey(KeyCode.UpArrow))
        {
            // Moving the body forward
            transform.position += transform.forward * movementSpeed * Time.deltaTime;
            movementDirection = transform.forward;
            // Correcting the position and rotation of the body so it stays parallel to the ground and at a good distance
            RaycastHit hit;
            CorrectPosition(out hit);
            CorrectRotation(hit);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            // Moving the body backward
            transform.position += -transform.forward * movementSpeed * Time.deltaTime;
            movementDirection = -transform.forward;
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
        // hit is the raycast hit that hit the ground below the creature
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        if (Mathf.Abs(Quaternion.Angle(targetRotation, transform.rotation)) > rotationCorrectionAngle)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationCorrectionSpeed * Time.deltaTime);
        }
    }
}
