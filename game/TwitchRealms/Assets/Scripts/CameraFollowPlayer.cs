using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    public Transform player;
    public float followSpeed = 3f;
    public float rotationSpeed = 10f;
    public float acceleration = 2f;
    public float deceleration = 1f;

    // Offset from the player when moving
    public Vector3 offset = new(0f, 8f, -8f);

    // Offset from the player when stationary
    public Vector3 offsetRoot = new(0f, 5f, -5f);

    private Vector3 currentVelocity;
    private Vector3 currentOffset;
    private Vector3 currentPosition;

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("Set a player in script!");
            enabled = false;
            return;
        }

        currentOffset = offsetRoot;
        currentPosition = player.position;
        UpdateCameraPosition();
    }

    private void LateUpdate()
    {
        UpdateCameraPosition();
    }

    private Bounds GetMaxBounds(GameObject g)
    {
        var renderers = g.GetComponentsInChildren<Renderer>();        
        if (renderers.Length == 0) return new Bounds(g.transform.position, Vector3.zero);
        var b = renderers[0].bounds;
        foreach (Renderer r in renderers)
        {
            b.Encapsulate(r.bounds);
        }
        return b;
    }

    private void UpdateCameraPosition()
    {
        // Determine if the player is moving
        bool isMoving = (player.position - currentPosition).magnitude > 0.01f;       
        currentPosition = player.position;

        var b = GetMaxBounds(player.gameObject);
        float scalar = Mathf.Sqrt(b.max.magnitude) * 2;        
        Vector3 scalarOffset;

        // Update current offset based on whether the player is moving or stationary
        if (isMoving)
        {
            scalarOffset = new(offset.x, offset.y, offset.z + -0.33f * scalar);
            currentOffset = Vector3.Lerp(currentOffset, scalarOffset, acceleration * Time.deltaTime);
        }
        else
        {
            scalarOffset = new(offsetRoot.x, offsetRoot.y, offsetRoot.z + -0.33f * scalar);
            currentOffset = Vector3.Lerp(currentOffset, scalarOffset, deceleration * Time.deltaTime);
        }

        // Calculate the desired position based on the current offset
        Vector3 desiredPosition = player.position + player.forward * currentOffset.z + Vector3.up * currentOffset.y;

        // Smooth the camera movement to the desired position
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / followSpeed, Mathf.Infinity, Time.deltaTime);
        transform.position = smoothedPosition;

        // Smoothly rotate the camera to look at the player
        Vector3 directionToPlayer = player.position - transform.position;
        Quaternion desiredRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
    }


}