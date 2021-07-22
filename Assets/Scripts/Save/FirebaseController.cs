using Firebase;
using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseController : MonoBehaviour
{
    public static FirebaseController Instance;
    string deviceID;
    DatabaseReference reference;
    public static SaveData currentSaveData;

    private void Awake()
    {
        Instance = this;
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        deviceID = SystemInfo.deviceUniqueIdentifier;
        GetData();
    }

    public void SaveData()
    {
        string json = JsonUtility.ToJson(currentSaveData);

        reference.Child("saves").Child(deviceID).SetRawJsonValueAsync(json);
    }

    public void GetData()
    {
        reference.Child("saves").Child(deviceID).GetValueAsync().ContinueWith(task => {
          if (task.IsFaulted)
          {
                CreateSaveData();
          }
          else if (task.IsCompleted)
          {
                if (task.Result.GetRawJsonValue() != null)
                {
                    currentSaveData = JsonUtility.FromJson<SaveData>(task.Result.GetRawJsonValue());
                } 
                else
                {
                    CreateSaveData();
                }   
            }
        });
    }

    public int XpAmount
    {
        get { return currentSaveData.xpAmount; }
        set { currentSaveData.xpAmount = value; SaveData(); }
    }
    public int NukeCount
    {
        get { return currentSaveData.nukeCount; }
        set { currentSaveData.nukeCount = value; SaveData(); }
    }
    public int TrapCount
    {
        get { return currentSaveData.trapCount; }
        set { currentSaveData.trapCount = value; SaveData(); }
    }
    //settings
    public bool Music
    {
        get { return currentSaveData.music; }
        set { currentSaveData.music = value; SaveData(); }
    }
    public bool Sfx
    {
        get { return currentSaveData.sfx; }
        set { currentSaveData.sfx = value; SaveData(); }
    }
    public int NodeCount
    {
        get { return currentSaveData.nodeCount; }
        set { currentSaveData.nodeCount = value; SaveData(); }
    }
    public int TreasureCount
    {
        get { return currentSaveData.treasureCount; }
        set { currentSaveData.treasureCount = value; SaveData(); }
    }
    public int FirewallCount
    {
        get { return currentSaveData.firewallCount; }
        set { currentSaveData.firewallCount = value; SaveData(); }
    }
    public int SpamCount
    {
        get { return currentSaveData.spamCount; }
        set { currentSaveData.spamCount = value; SaveData(); }
    }
    public float SpamDecrease
    {
        get { return currentSaveData.spamDecrease; }
        set { currentSaveData.spamDecrease = value; SaveData(); }
    }
    public float TrapDelay
    {
        get { return currentSaveData.trapDelay; }
        set { currentSaveData.trapDelay = value; SaveData(); }
    }

    void CreateSaveData()
    {
        currentSaveData = new SaveData();
        SaveData();
    }
}