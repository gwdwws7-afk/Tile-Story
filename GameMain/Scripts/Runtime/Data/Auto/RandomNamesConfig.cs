using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RandomNamesConfig
{
    public List<RobotName> RandomNames;

    public string GetRandomName()
    {
        return RandomNames[UnityEngine.Random.Range(0, RandomNames.Count)].Name;
    }
}

[Serializable]
public class RobotName
{
    public int ID;

    public string Name;
}
