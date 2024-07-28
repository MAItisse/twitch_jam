using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controllable : MonoBehaviour
{
    public float speed;
    public bool enabled = false;

    // Update is called once per frame
    void Update()
    {
        Vector3 desiredPosition = transform.position;
        // check to see if space is pressed
        if (Input.GetKey(KeyCode.W))
        {
            desiredPosition += new Vector3(0, 0, speed);
        }
        if (Input.GetKey(KeyCode.A))
        {
            desiredPosition += new Vector3(-speed, 0, 0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            desiredPosition += new Vector3(0, 0, -speed);
        }
        if (Input.GetKey(KeyCode.D))
        {
            desiredPosition += new Vector3(speed, 0, 0);
        }

        if (enabled)
        {
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, speed * Time.deltaTime);
            transform.position = smoothedPosition;
        }
        //transform.LookAt(followMe);
        //        if (Input.GetKeyDown(KeyCode.Space))
    }
}
