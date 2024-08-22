using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        StartCoroutine(UpdateCSS());
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
                MapObject[] worldCombinables = world.GetComponentsInChildren<MapObject>(true).Where(combinable => combinable.gameObject.activeInHierarchy).ToArray();
                mapObjects.AddRange(worldCombinables);
            }
        }
    }

    public void AddCombinable(MapObject mapObj)
    {
        mapObjects.Add(mapObj);
    }

    private IEnumerator UpdateCSS()
    {
        StringBuilder stylesBuilder = new();

        while (websocket != null)
        {
            stylesBuilder.Clear();
            stylesBuilder.Append("{\"data\":{\"css\": \"");
            foreach (var mapObject in mapObjects)
            {
                // Handle color and extra CSS
                string id = "_" + Math.Abs(mapObject.GetInstanceID());
                stylesBuilder.Append($".{id} {{ background-color: {ColorToRgbString(mapObject.mapColor)}");
                string extraCss = mapObject.extraCss;
                if (!string.IsNullOrEmpty(extraCss))
                {
                    stylesBuilder.Append($";{extraCss.Trim()}");
                }

                stylesBuilder.Append("}");
            }
            // Add global keyframes animations, these are available to be added to extra css
            stylesBuilder.Append("@keyframes shimmer {");
            stylesBuilder.Append("0% { background-color: rgb(170, 220, 170)}");
            stylesBuilder.Append("50% { background-color: rgb(0, 209, 224)}");
            stylesBuilder.Append("100% { background-color: rgb(193, 220, 170)}");
            stylesBuilder.Append("}");
            stylesBuilder.Append("@keyframes sizeFluctuation {");
            stylesBuilder.Append("100% { transform: scale(2); }");
            stylesBuilder.Append("}");
            stylesBuilder.Append("@keyframes doRotate {");
            stylesBuilder.Append("0% { transform: rotate(0deg) }");
            stylesBuilder.Append("100% { transform: rotate(360deg) }");
            stylesBuilder.Append("}");
            stylesBuilder.Append("\"} }"); // Close: data, root

            string styles = stylesBuilder.ToString();
            websocket.SendWsMessage(styles);

            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator UpdateLobby()
    {
        Vector3 planeScale = planeTransform.localScale;
        float planeSizeX = 5 * planeScale.x;
        float planeSizeZ = 5 * planeScale.z;
        Vector3 planeCenter = planeTransform.position;
        Vector3 planeMin = planeCenter - new Vector3(planeSizeX, 0f, planeSizeZ);
        Vector3 planeMax = planeCenter + new Vector3(planeSizeX, 0f, planeSizeZ);

        while (websocket != null)
        {
            yield return new WaitForSeconds(0.1f);

            List<MiniMapObject> miniMapObjects = new();

            foreach (var mapObject in mapObjects)
            {
                Vector2 normalizedCoords = RelativeCoords(mapObject.transform.position, planeMin, planeMax);
                string kind = mapObject.name.Split(' ')[0].Replace("(Clone)", "");

                MiniMapObject miniMap = new()
                {
                    id = Math.Abs(mapObject.GetInstanceID()),
                    x = Math.Round(normalizedCoords.x, 3),
                    y = Math.Round(normalizedCoords.y, 3),
                    kind = kind
                };
                miniMapObjects.Add(miniMap);
            }
            string jsonData = JsonUtility.ToJson(new MiniMapObjectCollection { data = miniMapObjects });

            // Send both JSON data and styles via WebSocket
            websocket.SendWsMessage(jsonData);
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
