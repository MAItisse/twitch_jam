using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public class MapConnector : MonoBehaviour
{
    HashSet<MapObject> mapObjects;
    Transform planeTransform;
    private WebSocketManager websocket;
    public Iteractable iteractable;

    void Start()
    {
        mapObjects = new();
        planeTransform = GameObject.Find("Ground").transform; /* reference to your plane transform */
        websocket = FindObjectOfType<WebSocketManager>();
        UpdateMapWorld();
        StartCoroutine(UpdateLobby());
    }

    public void UpdateMapWorld()
    {
        mapObjects.Clear();
        var updateMinimap = "{'data':{'reset':true}}";
        websocket.SendMessage(updateMinimap.Replace('\'', '"'));

        foreach (Transform world in gameObject.transform)
        {
            if (world.gameObject.activeInHierarchy)
            {
                MapObject[] worldCombinables = world.GetComponentsInChildren<MapObject>(true);
                mapObjects.AddRange(worldCombinables);
            }
        }
    }

    public void AddCombinable(MapObject mapObj)
    {
        mapObjects.Add(mapObj);
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

            List<MiniMapObject> miniMapObjects = new();

            foreach (var mapObject in mapObjects)
            {
                Vector3 objectWorldPos = mapObject.transform.position;
                float relativeX = Mathf.Clamp((objectWorldPos.x - planeMin.x) / (planeMax.x - planeMin.x), 0f, 1f);
                float relativeZ = Mathf.Clamp((objectWorldPos.z - planeMin.z) / (planeMax.z - planeMin.z), 0f, 1f);
                Vector2 normalizedCoords = new(relativeX, relativeZ);
                var kind = mapObject.name.Split(' ')[0].Replace("(Clone)", "");
                if (mapObject.cssClassName != "") kind = mapObject.cssClassName;
                MiniMapObject miniMap = new() {
                    id = mapObject.GetInstanceID(),
                    x = normalizedCoords.x,
                    y = normalizedCoords.y,
                    kind = kind
                };

                miniMapObjects.Add(miniMap);
            }

            string jsonData = JsonUtility.ToJson(new MiniMapObjectCollection { data = miniMapObjects });
            websocket.SendMessage(jsonData.Replace('\'', '"'));
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

[Serializable]
public class MiniMapObjectCollection
{
    public List<MiniMapObject> data;
}
