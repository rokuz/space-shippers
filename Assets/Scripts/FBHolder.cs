using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using Facebook.Unity;

public class FBHolder : MonoBehaviour
{
  public MenuPanel menuPanel;

  private static bool facebookInitializeCalled = false;
  private static bool facebookInitialized = false;
  private bool loginInProgress = false;

  private string facebookId = "";

  public delegate void OnLoginFinished(bool success);
  private OnLoginFinished loginCallback;

  public bool FacebookInitialized
  {
    get { return facebookInitialized; }
  }

  public void Awake()
  {
    if (!facebookInitializeCalled)
    {
      FB.Init(OnInitComplete);
      facebookInitializeCalled = true;
    }
  }

  public void Login(OnLoginFinished callback)
  {
    if (FB.IsLoggedIn && callback != null)
    {
      callback(true);
      return;
    }

    if (!FB.IsLoggedIn && !loginInProgress)
    {
      loginCallback = callback;
      loginInProgress = true;
      FB.LogInWithReadPermissions(new List<string>(){ "public_profile", "user_friends" }, AuthCallback);
    }
  }

  public void Logout()
  {
    if (FB.IsLoggedIn)
      FB.LogOut();
  }

  public bool FacebookLoggedIn
  {
    get { return FB.IsLoggedIn; }
  }

  public string FacebookID
  {
    get { return facebookId; }
  }

  public bool FacebookLoginInProgress
  {
    get { return loginInProgress; }
  }

  private void AuthCallback(ILoginResult result)
  {
    if (!FB.IsLoggedIn)
    {
      if (result.Error != null && result.Error.Length != 0)
        Debug.Log(result.Error);
      Debug.Log("User didn't log into Facebook");
      if (loginCallback != null)
        loginCallback(false);
    }
    else
    {
      Debug.Log("User logged into Facebook");
      if (loginCallback != null)
        loginCallback(true);
    }
    loginInProgress = false;
    loginCallback = null;
  }

  private void OnInitComplete()
  {
    Debug.Log("Facebook Initialized");
    facebookInitialized = true;
  }

  private void InviteFriends()
  {
    string FriendSelectorMessage = "Invite friends to play Space Shippers";
    string[] FriendSelectorFilters = new string[]{"app_non_users"};
    FB.AppRequest(FriendSelectorMessage, null, FriendSelectorFilters, null, 5, "", "", InviteCallback);
  }

  private void InviteCallback(IAppRequestResult result)
  {
    if (result.Error != null && result.Error.Length != 0)
    {
      Analytics.CustomEvent("InvitationFailed");
      Debug.Log(result.Error);
      Debug.Log("Friends were not invited");
    }
    else if (result.To.ToCommaSeparateList().Length == 0 || result.Cancelled)
    {
      Analytics.CustomEvent("InvitationCancelled");
      Debug.Log("Request was cancelled");
    }
    else
    {
      Analytics.CustomEvent("InvitationCompleted");
      Debug.Log("Friends were invited");
    }
  }

  public void OnShare()
  {
    Login((bool success) => {
      if (success)
        InviteFriends();
    });
  }
}
