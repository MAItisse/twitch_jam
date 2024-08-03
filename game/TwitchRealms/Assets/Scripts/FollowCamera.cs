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
    private bool rotateOnSpace = false;

    void Start()
    {
        currentOffset = offset;
    }

    void Update()
    {
        HandleCameraFollow();
        HandleCameraRotation();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            RotateCamera();
        }
    }

    void HandleCameraFollow()
    {
        Vector3 desiredPosition = followMe.position + currentOffset;
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, followSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }

    void HandleCameraRotation()
    {
        Vector3 direction = followMe.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void RotateCamera()
    {
        // Rotate the camera by the specified angle around the player
        transform.RotateAround(followMe.position, Vector3.up, rotationAngle);

        // Update the offset based on the new camera position
        currentOffset = transform.position - followMe.position;
    }
}
