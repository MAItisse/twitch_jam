#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
[CustomEditor(typeof(TwitchSDKSettings))]
public class TwitchSDKSettingsEditor : Editor
{
    private void SetDirtyIfNeeded<T>(ref T field, T value)
    {
        if (!System.Object.Equals(field, value))
        {
            field = value;
            EditorUtility.SetDirty(target);
        }
    }


    public override void OnInspectorGUI()
    {
        var inst = TwitchSDKSettings.Instance;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Twitch Client ID:");
        SetDirtyIfNeeded(ref inst.ClientId, EditorGUILayout.TextField(inst.ClientId));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("Status: " + this.CredentialStatus);
        if (GUILayout.Button("Go to dev.twitch.tv", EditorStyles.linkLabel))
        {
            System.Diagnostics.Process.Start("https://dev.twitch.tv");
        }

        UpdateCredentialStatus(inst.ClientId);

        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Use EventSubProxy:", "Do not enable this in releases. This option instructs the plugin to connect to a local EventSubProxy instead of directly to Twitch."));
        SetDirtyIfNeeded(ref inst.UseEventSubProxy, EditorGUILayout.Toggle(inst.UseEventSubProxy));
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Installed Plugin Core Library:");
        EditorGUILayout.LabelField(TwitchSDK.TwitchSDKApi.Version);
        EditorGUILayout.EndHorizontal();
    }

    [MenuItem("Twitch/Edit Settings")]
    public static void Edit()
    {
        if (TwitchSDKSettings.NullableInstance == null)
        {
            var instance = ScriptableObject.CreateInstance<TwitchSDKSettings>();
            string path = Path.Combine(Application.dataPath, "Plugins", "Resources");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string str = Path.Combine(Path.Combine("Assets", "Plugins", "Resources"), $"{nameof(TwitchSDKSettings)}.asset");
            AssetDatabase.CreateAsset(instance, str);
        }
        Selection.activeObject = TwitchSDKSettings.Instance;
    }


    HttpClient Http = new HttpClient();
    string CredentialStatus = "";
    CancellationTokenSource CurrentCts = null;
    string LastCheckedClientId = "";

    public TwitchSDKSettingsEditor()
    {
        Http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Twitch-Route-66", "0.1"));
        Http.Timeout = TimeSpan.FromSeconds(5);
    }

    public async void UpdateCredentialStatus(string clientId)
    {
        try
        {
            if (clientId == LastCheckedClientId)
            {
                return;
            }

            LastCheckedClientId = clientId;

            CurrentCts?.Cancel();
            CurrentCts = new CancellationTokenSource();
            this.CredentialStatus = "Checking ClientId ...";
            try
            {
                this.CredentialStatus = await GetCredentialStatus(clientId);
            }
            catch (TaskCanceledException)
            {
            }
            this.Repaint();
        }
        catch (Exception e)
        {
            Debug.LogWarning("Error updating credential status.");
            Debug.LogException(e);
        }
    }

    public async Task<string> GetCredentialStatus(string clientId)
    {
        if (clientId.Length == 0 || clientId == TwitchSDKSettings.InitialClientId)
            return "Please enter a valid ClientId!";

        clientId = Uri.EscapeDataString(clientId);

        try
        {
            var res = await Http.PostAsync(
                $"https://id.twitch.tv/oauth2/device?client_id={clientId}",
                new StringContent(""),
                CurrentCts.Token);
            var text = await res.Content.ReadAsStringAsync();

            if (res.IsSuccessStatusCode)
                return "ClientId is valid!";

            if (res.StatusCode == System.Net.HttpStatusCode.BadRequest)
                return "Please enter a valid ClientId!";
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

        return "Unable to check if the ClientId is valid.";
    }
}

#endif