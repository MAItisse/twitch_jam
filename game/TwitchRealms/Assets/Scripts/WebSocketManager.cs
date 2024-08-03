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
    private float keepAliveInterval = 30.0f; // Increased keep-alive frequency
    int channelId = 0;
    private int reconnectionAttempts = 0;
    private const int maxReconnectionAttempts = 5;
    private float reconnectionDelay = 2.0f;

    private void Start()
    {
        // this is so we can keep the keepalive running while the service is not focused
        Application.runInBackground = true;
    }

    private void Update()
    {
        if (!sent && Twitch.API.GetMyStreamInfo().IsCompleted)
        {
            sent = true;
            channelId = int.Parse(Twitch.API.GetMyUserInfo().MaybeResult.ChannelId);
            StartCoroutine(ReconnectWithDelay());
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            ws.Close();
            Debug.Log("Closing ws");
        }
    }

    IEnumerator CreateRoom()
    {
        Debug.Log(url + channelId);

        // Assuming the API might need a JSON payload
        string jsonPayload = "{}"; // Adjust the payload as needed

        UnityWebRequest request = new UnityWebRequest(url + channelId, "POST");
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
            StartCoroutine(ReconnectWithDelay());
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

    public void SendMessage(string message)
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
