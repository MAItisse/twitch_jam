using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TwitchSDK;
using TwitchSDK.Interop;
using UnityEngine.Networking;
using System;
using WebSocketSharp;
using System.Threading;

public class WebSocketManager : MonoBehaviour
{
    bool sent;
    string url = "https://websocket.matissetec.dev/lobby/new?user=";
    private WebSocket ws;
    private bool isRunning = false;
    [SerializeField]
    private float keepAliveInterval = 30.0f; // Increased keep-alive frequency
    [SerializeField]
    int channelId = 0;
    private int reconnectionAttempts = 0;
    private const int maxReconnectionAttempts = 5;
    private float reconnectionDelay = 2.0f;
    private static WebSocketManager instance;
    private BubbleGenerator _bubbleGenerator;
    private ItemGenerator _itemGenerator;

    private bool showClick = false;
    private string _messageEventData;

    public MapObject cube, sphere;
    public GameObject ground;
    public GameObject parentOfClicks;



    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } 
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // this is so we can keep the keepalive running while the service is not focused
        Application.runInBackground = true;
        _bubbleGenerator = FindObjectOfType<BubbleGenerator>();
        _itemGenerator = FindObjectOfType<ItemGenerator>();
    }

    private void Update()
    {
        if (!sent && Twitch.API.GetMyStreamInfo().IsCompleted)
        {
            sent = true;
            channelId = int.Parse(Twitch.API.GetMyUserInfo().MaybeResult.ChannelId);
            StartCoroutine(ReconnectWithDelay());
        }

        if (showClick)
        {
            try
            {
                BubbleData data = JsonUtility.FromJson<BubbleData>(_messageEventData);
                if (data.userId != null)
                {
                    float t = 5 * 5;
                    _bubbleGenerator.SpawnBubble(data.bubbleColor, data.bubbleSize, new Vector2(Remap(data.x, 0, 1, -t, t), Remap(data.y, 0, 1, -t, t)));

                    MapObject toGenerate;
                    Debug.Log(data.itemType);
                    Debug.Log(_messageEventData);
                    if (data.itemType == "Cube")
                    {
                        toGenerate = cube.gameObject.GetComponent<MapObject>();
                    }
                    else if (data.itemType == "Sphere")
                    {
                        toGenerate = sphere.gameObject.GetComponent<MapObject>();
                    }
                    else
                    {
                        // Pick randomly between cube and sphere
                        if (UnityEngine.Random.value < 0.5f)
                        {
                            toGenerate = cube.gameObject.GetComponent<MapObject>();
                        }
                        else
                        {
                            toGenerate = sphere.gameObject.GetComponent<MapObject>();
                        }
                    }
                    
                    _itemGenerator.GenerateGameObject(toGenerate, new Vector3(Remap(data.x, 0, 1, -t, t), 0, Remap(data.y, 0, 1, -t, t)), HexToColor(data.bubbleColor));
                    //cube, sphere
                    //go.transform.position = );
                }
            }
            catch (Exception ex)
            {
                showClick = false;
            }
            showClick = false;
        }
    }

    float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
    }

    Color HexToColor(string hex)
    {
        if (hex.StartsWith("#"))
        {
            hex = hex.Substring(1);
        }

        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        byte a = 255; // Default to fully opaque

        if (hex.Length == 8)
        {
            a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        }

        return new Color32(r, g, b, a);
    }

    IEnumerator CreateRoom()
    {
        Debug.Log(url + channelId);

        // Assuming the API might need a JSON payload
        string jsonPayload = "{}"; // Adjust the payload as needed

        UnityWebRequest request = new(url + channelId, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + request.error + " " + channelId);
        }
        else
        {
            // Get the response text
            string responseText = request.downloadHandler.text;
            Debug.Log("Response: " + responseText);

            ws = new WebSocket($"wss://websocket.matissetec.dev/lobby/connect/streamer?user={channelId}&key={responseText}");

            // Attach event handlers
            ws.OnMessage += OnMessageReceived;
            ws.OnOpen += OnOpen;
            ws.OnClose += OnClose;
            ws.OnError += OnError;

            // Connect to the WebSocket server
            ws.Connect();
            isRunning = true;
            reconnectionAttempts = 0; // Reset reconnection attempts on successful connection
            StartCoroutine(SendKeepAlive());
        }
    }

    private void OnMessageReceived(object sender, MessageEventArgs e)
    {
        Debug.Log("Message received: " + e.Data);
        showClick = true;
        _messageEventData = e.Data;
    }

    private void OnOpen(object sender, System.EventArgs e)
    {
        Debug.Log("WebSocket connection opened");
    }

    private void OnClose(object sender, CloseEventArgs e)
    {
        Debug.Log("WebSocket connection closed with reason: " + e.Reason);
        isRunning = false;
        if (reconnectionAttempts < maxReconnectionAttempts)
        {
            reconnectionAttempts++;
            if (this.gameObject.activeInHierarchy)
                StartCoroutine(ReconnectWithDelay());
            else
                Debug.Log("Program ended, stopping heartbeat");

            isRunning = true;
        }
        else
        {
            Debug.LogError("Max reconnection attempts reached. Unable to reconnect to WebSocket.");
        }
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        Debug.Log("WebSocket error: " + e.Message);
        StartCoroutine(ReconnectWithDelay());
    }

    public void SendWsMessage(string message)
    {
        if (ws != null && ws.IsAlive)
        {
            ws.Send(message);
        }
    }

    private IEnumerator SendKeepAlive()
    {
        while (isRunning)
        {
            if (ws != null && ws.IsAlive)
            {
                ws.Ping();
                Debug.Log("sending keep alive");
            }
            else
            {
                StartCoroutine(ReconnectWithDelay());
            }
            yield return new WaitForSecondsRealtime(keepAliveInterval);
        }
    }

    private IEnumerator ReconnectWithDelay()
    {
        yield return new WaitForSeconds(reconnectionDelay);
        StartCoroutine(CreateRoom());
        StartCoroutine(SendKeepAlive());
    }

    void OnDestroy()
    {
        // Clean up the WebSocket connection
        if (ws != null)
        {
            ws.Close();
            ws = null;
        }
    }
}
