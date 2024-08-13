using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Iteractable : MonoBehaviour
{
    private readonly float radius = 1.25f;
    private WebSocketManager websocket;
    // Start is called before the first frame update
    void Start()
    {
        var rigidBody = gameObject.AddComponent<Rigidbody>();
        rigidBody.constraints = RigidbodyConstraints.FreezePosition;
        var collider = gameObject.AddComponent<BoxCollider>();
        var triggerRange = gameObject.AddComponent<SphereCollider>();
        collider.center = collider.center + new Vector3(0, .5f, 0);
        triggerRange.radius = radius;
        triggerRange.isTrigger = true;
        websocket = GameObject.FindObjectOfType<WebSocketManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.gameObject.name);
        websocket.SendWsMessage($"following {other.gameObject.name}");
        // combine 

        // todo set combine based on results of poll
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(gameObject.transform.position, Vector3.one);
        Gizmos.DrawWireSphere(gameObject.transform.position, radius);
    }
}
