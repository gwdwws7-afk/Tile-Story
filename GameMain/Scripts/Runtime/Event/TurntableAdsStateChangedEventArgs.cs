using System.Collections;
using System.Collections.Generic;
using GameFramework;
using GameFramework.Event;
using UnityEngine;

public class TurntableAdsStateChangedEventArgs : GameEventArgs
{
    public static int EventId = typeof(TurntableAdsStateChangedEventArgs).GetHashCode();
    public override void Clear()
    {
        
    }

    public override int Id
    {
        get { return EventId; }
    }

    public static TurntableAdsStateChangedEventArgs Create()
    {
        var eventArgs = ReferencePool.Acquire<TurntableAdsStateChangedEventArgs>();
        return eventArgs;
    }
}
