using UnityEngine;

public class Controllable : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 360f;  // Rotation speed in degrees per second
    public float acceleration = 2f;
    public float deceleration = 4f;

    private Vector3 velocity;
    private Vector3 smoothInputVelocity;
    private Vector3 currentInputVector;

    void Update()
    {
        // Get input
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        // Apply rotation based on horizontal input
        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            transform.Rotate(0f, horizontalInput * rotationSpeed * Time.deltaTime, 0f);
        }

        // Forward/backward movement based on vertical input
        Vector3 inputVector = transform.forward * verticalInput;

        // Smooth the input
        currentInputVector = Vector3.SmoothDamp(currentInputVector, inputVector, ref smoothInputVelocity, 0.1f);

        // Apply acceleration or deceleration
        if (currentInputVector.magnitude > 0.1f)
        {
            velocity = Vector3.Lerp(velocity, currentInputVector * moveSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            velocity = Vector3.Lerp(velocity, Vector3.zero, deceleration * Time.deltaTime);
        }

        // Move the player
        transform.Translate(velocity * Time.deltaTime, Space.World);
    }
}
