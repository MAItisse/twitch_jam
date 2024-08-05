using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapConnector : MonoBehaviour
{
    List<Combinable> combinables;
    Transform planeTransform;
    private WebSocketManager websocket;
    public Iteractable iteractable;

    void Start()
    {
        combinables = new List<Combinable>();
        // go to all current enabled children and get their children nodes and check them for combinable
        UpdateMapWorld();
        //combinables.AddRange(gameObject.GetComponentsInChildren<Combinable>());
        planeTransform = GameObject.Find("Ground").transform;/* reference to your plane transform */;
        websocket = FindObjectOfType<WebSocketManager>();
        StartCoroutine(UpdateLobby());
    }

    public void UpdateMapWorld()
    {
        combinables.Clear();
        foreach (Transform child in gameObject.transform)
        {
            if (child.gameObject.activeInHierarchy)
            {
                combinables.AddRange(child.GetComponentsInChildren<Combinable>());
            }
        }
    }

    public void AddCombinable(Combinable combinable)
    {
        combinables.Add(combinable);
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
            yield return new WaitForSeconds(.1f);

            var coords = "{'data':[";
            foreach (var combinable in combinables)
            {
                // Calculate the normalized coordinates on the plane
                Vector3 objectWorldPos = combinable.transform.position;
                float relativeX = Mathf.Clamp((objectWorldPos.x - planeMin.x) / (planeMax.x - planeMin.x), 0f, 1f);
                float relativeZ = Mathf.Clamp((objectWorldPos.z - planeMin.z) / (planeMax.z - planeMin.z), 0f, 1f);
                Vector2 normalizedCoords = new(relativeX, relativeZ);
                var kind = combinable.name.Split(' ')[0].Replace("(Clone)", "");
                if (combinable.gameObject.TryGetComponent<HeadMount>(out var headMount))
                    kind = headMount.name;
                // Send the normalized coordinates instead of raw positions
                MiniMapObject miniMap = new()
                {
                    id = combinable.GetInstanceID(),
                    x = normalizedCoords.x,
                    y = normalizedCoords.y,
                    kind = kind
                };
                coords += JsonUtility.ToJson(miniMap) + ",";
            }
            Vector2 loc = RelativeCoords(iteractable.transform.position, planeMin, planeMax);
            MiniMapObject miniMapObject = new()
            {
                id = 123321,
                x = loc.x,
                y = loc.y,
                kind = "Player"
            };
            coords += JsonUtility.ToJson(miniMapObject);
            websocket.SendMessage(coords.Replace('\'', '"') + "]}");
        }
    }

    private Vector2 RelativeCoords(Vector3 objectWorldPos, Vector3 planeMin, Vector3 planeMax)
    {
        return new Vector2(Mathf.Clamp((objectWorldPos.x - planeMin.x) / (planeMax.x - planeMin.x), 0f, 1f),
            Mathf.Clamp((objectWorldPos.z - planeMin.z) / (planeMax.z - planeMin.z), 0f, 1f));
    }
}

[Serializable]
public class MiniMapObject
{
    public int id;
    public float x, y;
    public string kind;
}