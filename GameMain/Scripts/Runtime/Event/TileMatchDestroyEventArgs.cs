using System.Collections;
using System.Collections.Generic;
using GameFramework;
using GameFramework.Event;
using UnityEngine;

public class TileMatchDestroyEventArgs : GameEventArgs
{
    public static readonly int EventId = typeof(TileMatchDestroyEventArgs).GetHashCode();

    public override void Clear()
    {
        
    }

    public override int Id => EventId;
    
    public static TileMatchDestroyEventArgs Create()
    {
        var tileMatchDestroyEventArgs = ReferencePool.Acquire<TileMatchDestroyEventArgs>();
        return tileMatchDestroyEventArgs;
    }
}
