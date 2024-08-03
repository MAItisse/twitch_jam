using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combinable : MonoBehaviour
{
    private WebSocketManager websocket;
    private Transform previousTransform;
    Transform planeTransform;

    private void Start()
    {
        websocket = GameObject.FindObjectOfType<WebSocketManager>();
        planeTransform = GameObject.Find("Ground").transform;/* reference to your plane transform */;
        StartCoroutine(UpdateLobby());
    }

    private IEnumerator UpdateLobby()
    {
        Vector3 planeScale = planeTransform.localScale;
        float planeSizeX = 10f * planeScale.x;
        float planeSizeZ = 10f * planeScale.z;
        Vector3 planeCenter = planeTransform.position;
        Vector3 planeMin = planeCenter - new Vector3(planeSizeX / 2f, 0f, planeSizeZ / 2f);
        Vector3 planeMax = planeCenter + new Vector3(planeSizeX / 2f, 0f, planeSizeZ / 2f);

        while (websocket != null)
        {
            yield return new WaitForSeconds(1);
            if (previousTransform == transform)
            {
                yield return new WaitForSeconds(4);
            }

            // Calculate the normalized coordinates on the plane
            Vector3 objectWorldPos = transform.position;
            float relativeX = Mathf.Clamp((objectWorldPos.x - planeMin.x) / (planeMax.x - planeMin.x), 0f, 1f);
            float relativeZ = Mathf.Clamp((objectWorldPos.z - planeMin.z) / (planeMax.z - planeMin.z), 0f, 1f);
            Vector2 normalizedCoords = new Vector2(relativeX, relativeZ);

            // Send the normalized coordinates instead of raw positions
            websocket.SendMessage("{" + "'x:'" + normalizedCoords.x + ", 'y':" + normalizedCoords.y + "}");

            previousTransform = transform;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent != null && other.transform.parent.TryGetComponent<CharacterBuilder>(out var characterBuilder)) 
        {
            if (!characterBuilder.ContainsCombinable(this))
            {
                characterBuilder.AddCombinable(this);
            }
        }
    }
}
