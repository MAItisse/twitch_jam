using System;
using TwitchSDK;
using TwitchSDK.Interop;
using UnityEngine;
using UnityEngine.UI;

public class TwitchAuth : MonoBehaviour
{
    private bool alreadyLoggedIn = false;
    GameTask<AuthenticationInfo> AuthInfoTask;
    GameTask<AuthState> curAuthState;

    //This example uses all scopes, we suggest you only request the scopes you actively need.
    string scopes = TwitchOAuthScope.Bits.Read.Scope + " " + TwitchOAuthScope.Channel.ManageBroadcast.Scope + " " + TwitchOAuthScope.Channel.ManagePolls.Scope + " " + TwitchOAuthScope.Channel.ManagePredictions.Scope + " " + TwitchOAuthScope.Channel.ManageRedemptions.Scope + " " + TwitchOAuthScope.Channel.ReadHypeTrain.Scope + " " + TwitchOAuthScope.Clips.Edit.Scope + " " + TwitchOAuthScope.User.ReadSubscriptions.Scope;
    // string scopes = TwitchOAuthScope.Channel.ManageRedemptions.Scope + " " + TwitchOAuthScope.Channel.ManagePredictions.Scope + " " + TwitchOAuthScope.Channel.ManagePolls.Scope;
    public static event Action<bool> OnPluginConnected;
    private bool isAuthenticating = false;

    void Start()
    {
        InvokeRepeating("GetAuthInformation", 0, 2);
    }

    public void UpdateAuthState()
    {
        curAuthState = Twitch.API.GetAuthState();
        if (curAuthState.MaybeResult.Status == AuthStatus.LoggedIn)
        {
            //Plugin logged in
            Debug.Log("Plugin logged in");
            isAuthenticating = false;
            if (!alreadyLoggedIn)
            {
                alreadyLoggedIn = true;
                OnPluginConnected?.Invoke(true);
                CancelInvoke("GetAuthInformation");
            }
        }
        else if (curAuthState.MaybeResult.Status == AuthStatus.LoggedOut)
        {
            //Plugin logged out, do something
            //In this example you could also call GetAuthInformation() to retrigger login
            Debug.Log("Plugin logged out");
            AuthInfoTask = null;
        }
        else if (curAuthState.MaybeResult.Status == AuthStatus.WaitingForCode)
        {
            if (isAuthenticating)
            {
                Debug.Log("Plugin waiting for code");
                return;
            }
            //Waiting for code
            TwitchOAuthScope tscopes = new TwitchOAuthScope(scopes);
            var UserAuthInfo = Twitch.API.GetAuthenticationInfo(tscopes).MaybeResult;
            if (UserAuthInfo == null)
            {
                //Plugin still loading
                Debug.Log("Plugin still loading");
                return;
            }
            //We have reached the state where we can ask the user to login
            Application.OpenURL($"{UserAuthInfo.Uri}");
            isAuthenticating = true;
            //Debug.Log(UserAuthInfo.UserCode);
            Debug.Log("Plugin waiting for code");
        }
        else
        {
            Debug.Log(curAuthState.MaybeResult.Status);
        }
    }

    public void GetAuthInformation()
    {
        if (AuthInfoTask == null)
        {
            TwitchOAuthScope tscopes = new TwitchOAuthScope(scopes);
            AuthInfoTask = Twitch.API.GetAuthenticationInfo(tscopes);
        }
        else
        {
            UpdateAuthState();
        }
    }
}