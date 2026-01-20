
using System;
using System.Collections.Concurrent;
using Firebase.Analytics;
using UnityEngine;

public class AdmobUtils
{
    public static ConcurrentQueue<Action> ConcurrentQueue=new ConcurrentQueue<Action>();
}
