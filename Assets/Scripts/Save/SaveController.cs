using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    public static SaveController Instance;

    public static SaveData currentSaveData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }

    public static void CreateSaveData()
    {
        currentSaveData = new SaveData();
        WriteSaveData();
    }

    public static void WriteSaveData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/hackingSim.vg");
        bf.Serialize(file, currentSaveData);
        file.Close();
    }

    public static void LoadSaveData()
    {
        if (File.Exists(Application.persistentDataPath + "/hackingSim.vg"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/hackingSim.vg", FileMode.Open);
            currentSaveData = (SaveData)bf.Deserialize(file);
            file.Close();
        }
        else
        {
            currentSaveData = new SaveData();
            WriteSaveData();
        }
    }
}
