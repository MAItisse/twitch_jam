using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controllable : MonoBehaviour
{
    public float speed;
    public float acceleration = 2f;
    public bool enabled = false;
    public Vector2 velocity;

    void UpdateVelocity()
    {
        bool movingX = false;
        bool movingZ = false;
        // check to see if space is pressed
        if (Input.GetKey(KeyCode.W))
        {
            velocity.y += speed * Time.deltaTime * acceleration * 0.5f;
            movingZ = true;
        }
        if (Input.GetKey(KeyCode.A))
        {
            velocity.x -= speed * Time.deltaTime * acceleration * 0.5f;
            movingX = true;
        }
        if (Input.GetKey(KeyCode.S))
        {
            velocity.y -= speed * Time.deltaTime * acceleration * 0.5f;
            movingZ = true;
        }
        if (Input.GetKey(KeyCode.D))
        {
            velocity.x += speed * Time.deltaTime * acceleration * 0.5f;
            movingX = true;
        }

        velocity.x = Mathf.Clamp(velocity.x, -speed, speed);
        velocity.y = Mathf.Clamp(velocity.y, -speed, speed);

        if (!movingX)
        {
            if (velocity.x > 0f) velocity.x = Mathf.Max(velocity.x - speed * Time.deltaTime * 0.5f, 0f);
            if (velocity.x < 0f) velocity.x = Mathf.Min(velocity.x + speed * Time.deltaTime * 0.5f, 0f);
        }
        if (!movingZ)
        {
            if (velocity.y > 0f) velocity.y = Mathf.Max(velocity.y - speed * Time.deltaTime * 0.5f, 0f);
            if (velocity.y < 0f) velocity.y = Mathf.Min(velocity.y + speed * Time.deltaTime * 0.5f, 0f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 desiredPosition = transform.position;

        UpdateVelocity();
        desiredPosition += new Vector3(velocity.x * Time.deltaTime, 0, velocity.y * Time.deltaTime);
        UpdateVelocity();

        if (enabled)
        {
            // Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, speed * Time.deltaTime);
            transform.position = desiredPosition;
        }
        //transform.LookAt(followMe);
        //        if (Input.GetKeyDown(KeyCode.Space))
    }
}