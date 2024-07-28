using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Iteractable : MonoBehaviour
{
    private readonly float radius = 1f;
    // Start is called before the first frame update
    void Start()
    {
        var collider = gameObject.AddComponent<BoxCollider>();
        var triggerRange = gameObject.AddComponent<SphereCollider>();
        triggerRange.radius = radius;
        triggerRange.isTrigger = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<Controllable>(out var controllableCharacter))
        {
            controllableCharacter.enabled = false;
            var newControllable = gameObject.GetOrAddComponent<Controllable>();
            newControllable.enabled = true;
            newControllable.speed = 2;
            Camera.main.GetComponent<FollowCamera>().followMe = newControllable.gameObject.transform;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(gameObject.transform.position, Vector3.one);
        Gizmos.DrawWireSphere(gameObject.transform.position, radius);
    }
}
