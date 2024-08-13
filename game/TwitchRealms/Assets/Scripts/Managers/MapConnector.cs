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
    public Color SphereColor, CubeColor, PlayerColor, SuzanneColor, BozzColor, DoorColor, ShimmerStartColor, ShimmerMidColor, ShimmerEndColor;

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
        websocket.SendWsMessage(updateMinimap.Replace('\'', '"'));

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
            websocket.SendWsMessage(jsonData.Replace('\'', '"'));

            string stylesTemplate = @"{
                ""data"":{
                    ""css"": {
                        "".Sphere"": {
                            ""background-color"": ""{SphereColor};"",
                            ""border-radius"": ""var(--size);""
                        },
                        "".Cube"": {
                            ""background-color"": ""{CubeColor};""
                        },
                        "".Player"": {
                            ""--size"": ""40px;"",
                            ""opacity"": ""0.3;"",
                            ""background-color"": ""{PlayerColor};"",
                            ""border-radius"": ""var(--size);""
                        },
                        "".Suzanne"": {
                            ""background-color"": ""{SuzanneColor};""
                        },
                        "".Bozz"": {
                            ""background-color"": ""{BozzColor};""
                        },
                        "".Door"": {
                            ""background-color"": ""{DoorColor};"",
                            ""animation"": ""shimmer 2s infinite;""
                        },
                        ""@keyframes shimmer"": {
                            ""0%"": {
                                ""background-color"": ""{ShimmerStartColor};""
                            },
                            ""50%"": {
                                ""background-color"": ""{ShimmerMidColor};""
                            },
                            ""100%"": {
                                ""background-color"": ""{ShimmerEndColor};""
                            }
                        }
                    }
                }
            }";
            

            string styles = stylesTemplate
                .Replace("{SphereColor}", ColorToRgbString(SphereColor))
                .Replace("{CubeColor}", ColorToRgbString(CubeColor))
                .Replace("{PlayerColor}", ColorToRgbString(PlayerColor))
                .Replace("{SuzanneColor}", ColorToRgbString(SuzanneColor))
                .Replace("{BozzColor}", ColorToRgbString(BozzColor))
                .Replace("{DoorColor}", ColorToRgbString(DoorColor))
                .Replace("{ShimmerStartColor}", ColorToRgbString(ShimmerStartColor))
                .Replace("{ShimmerMidColor}", ColorToRgbString(ShimmerMidColor))
                .Replace("{ShimmerEndColor}", ColorToRgbString(ShimmerEndColor));

            // Now 'styles' contains your JSON string with the dynamic color values.


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
