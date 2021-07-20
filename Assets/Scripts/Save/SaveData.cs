using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int xpAmount;
    public int nukeCount;
    public int trapCount;

    //settings
    public bool music;
    public bool sfx;
    public int nodeCount;
    public int treasureCount;
    public int firewallCount;
    public int spamCount;
    public float spamDecrease;
    public float trapDelay;

    public SaveData()
    {
        xpAmount = 0;
        nukeCount = 0;
        trapCount = 0;
        sfx = true;
        music = true;
        nodeCount = 0;
        treasureCount = 0;
        firewallCount = 0;
        spamCount = 0;
        spamDecrease = 0;
        trapDelay = 0;
   
    }

    public SaveData Copy()
    {
        return (SaveData)MemberwiseClone();
    }
}
