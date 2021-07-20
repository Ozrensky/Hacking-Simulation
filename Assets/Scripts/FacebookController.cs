using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;
using UnityEngine;

public class FacebookController : MonoBehaviour
{
    [Header("Rewards")]
    public int nukeAmount;
    public int xpAmount;
    public int trapAmount;
    [Header("-------------")]
    public GameObject fbButtonObject;

    private void Awake()
    {
        if (!FB.IsInitialized)
        {
            FB.Init(InitCallback);
        }
        else
        {
            FB.ActivateApp();
        }
    }

    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            FB.ActivateApp();
            if (FB.IsLoggedIn)
                fbButtonObject.SetActive(false);
        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }

    public void Login()
    {
        var perms = new List<string>() { "email" };
        FB.LogInWithReadPermissions(perms, AuthCallback);
    }

    private void AuthCallback(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            // AccessToken class will have session details
            var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
            // Print current access token's User ID
            Debug.Log(aToken.UserId);
            Debug.Log("Facebook login successful");
            SaveController.currentSaveData.trapCount += trapAmount;
            SaveController.currentSaveData.nukeCount += nukeAmount;
            SaveController.currentSaveData.xpAmount += xpAmount;
            SaveController.WriteSaveData();
            fbButtonObject.SetActive(false);
            UIManager.Instance.OpenFbRewardPanel(xpAmount, nukeAmount, trapAmount);
        }
        else
        {
            Debug.Log("User cancelled login");
        }
    }
}
