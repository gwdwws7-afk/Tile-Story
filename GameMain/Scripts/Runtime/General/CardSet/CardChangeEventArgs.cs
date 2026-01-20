using System.Collections;
using System.Collections.Generic;
using GameFramework.Event;
using UnityEngine;

public class CardChangeEventArgs : GameEventArgs
{
    public static readonly int EventId = typeof(CardChangeEventArgs).GetHashCode();

    public override int Id => EventId;

    public static CardChangeEventArgs Create()
    {
        CardChangeEventArgs cardChangeEventArgs = GameFramework.ReferencePool.Acquire<CardChangeEventArgs>();
        return cardChangeEventArgs;
    }
    
    public override void Clear()
    {
    }
}
