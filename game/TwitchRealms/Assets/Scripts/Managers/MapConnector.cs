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

    void Start()
    {
        mapObjects = new HashSet<MapObject>();
        planeTransform = GameObject.Find("Ground").transform; // Reference to your plane transform
        websocket = FindObjectOfType<WebSocketManager>();
        UpdateMapWorld();
        StartCoroutine(UpdateLobby());
    }

    public void UpdateMapWorld()
    {
        mapObjects.Clear();
        var updateMinimap = "{\"data\":{\"reset\":true}}";
        websocket.SendWsMessage(updateMinimap);

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

        StringBuilder stylesBuilder = new();

        while (websocket != null)
        {
            yield return new WaitForSeconds(0.1f);

            List<MiniMapObject> miniMapObjects = new();
            stylesBuilder.Clear();
            stylesBuilder.Append("{");
            stylesBuilder.Append("\"data\":{");
            stylesBuilder.Append("\"css\":{");

            foreach (var mapObject in mapObjects)
            {
                Vector2 normalizedCoords = RelativeCoords(mapObject.transform.position, planeMin, planeMax);
                string kind = mapObject.name.Split(' ')[0].Replace("(Clone)", "");

                MiniMapObject miniMap = new()
                {
                    id = Math.Abs(mapObject.GetInstanceID()),
                    x = Math.Round(normalizedCoords.x, 3),
                    y = Math.Round(normalizedCoords.y, 3),
                    kind = kind,
                };

                miniMapObjects.Add(miniMap);

                // Handle color and extra CSS
                string id = "_" + Math.Abs(mapObject.GetInstanceID());
                stylesBuilder.Append($"\".{id}\":{{");
                stylesBuilder.Append($"\"background-color\":\"{ColorToRgbString(mapObject.mapColor)}\"");

                string extraCss = mapObject.extraCss;
                if (!string.IsNullOrEmpty(extraCss))
                {
                    stylesBuilder.Append($", {extraCss.Trim()}");
                }

                stylesBuilder.Append("},");
            }

            // Add global keyframes animation
            stylesBuilder.Append("\"@keyframes shimmer\":{");
            stylesBuilder.Append("\"0%\":{\"background-color\":\"rgb(170, 220, 170)\"},");
            stylesBuilder.Append("\"50%\":{\"background-color\":\"rgb(0, 209, 224)\"},");
            stylesBuilder.Append("\"100%\":{\"background-color\":\"rgb(193, 220, 170)\"}");
            stylesBuilder.Append("}");

            stylesBuilder.Append("}"); // Close css
            stylesBuilder.Append("}"); // Close data
            stylesBuilder.Append("}"); // Close root

            string styles = stylesBuilder.ToString();
            string jsonData = JsonUtility.ToJson(new MiniMapObjectCollection { data = miniMapObjects });

            // Send both JSON data and styles via WebSocket
            websocket.SendWsMessage(jsonData);
            websocket.SendWsMessage(styles);
        }
    }

    string ColorToRgbString(Color color)
    {
        int r = Mathf.RoundToInt(color.r * 255);
        int g = Mathf.RoundToInt(color.g * 255);
        int b = Mathf.RoundToInt(color.b * 255);
        return $"rgb({r}, {g}, {b})";
    }

    private Vector2 RelativeCoords(Vector3 objectWorldPos, Vector3 planeMin, Vector3 planeMax)
    {
        return new Vector2(
            Mathf.Clamp((objectWorldPos.x - planeMin.x) / (planeMax.x - planeMin.x), 0f, 1f),
            Mathf.Clamp((objectWorldPos.z - planeMin.z) / (planeMax.z - planeMin.z), 0f, 1f)
        );
    }
}

[Serializable]
public class MiniMapObject
{
    public int id;
    public double x, y;
    public string kind;
}

[Serializable]
public class MiniMapObjectCollection
{
    public List<MiniMapObject> data;
}
