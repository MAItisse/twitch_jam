using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform followMe;
    public Vector3 offset;
    public float followSpeed = 5f;
    public float rotationSpeed = 10f;
    public float rotationAngle = 90f; // The angle by which to rotate when space is pressed

    private Vector3 currentOffset;
    private Vector3 currentVelocity;

    void Update()
    {
        HandleCameraFollow();
    }

    void HandleCameraFollow()
    {
        Vector3 desiredPosition = followMe.position + offset;
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, followSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
}
