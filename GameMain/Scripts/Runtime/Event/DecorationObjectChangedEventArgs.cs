using GameFramework.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationObjectChangedEventArgs : GameEventArgs
{
    public static readonly int EventId = typeof(DecorationObjectChangedEventArgs).GetHashCode();

    public DecorationObjectChangedEventArgs()
    {
        AreaID = 0;
        PositionID = 0;
        AnimationType = 0;
        AnimationTime = 0;
        IsEnd = false;
    }

    public override int Id
    {
        get
        {
            return EventId;
        }
    }

    public int AreaID
    {
        get;
        private set;
    }

    public int PositionID
    {
        get;
        private set;
    }

    public int AnimationType
    {
        get;
        private set;
    }

    public float AnimationTime
    {
        get;
        private set;
    }

    public bool IsEnd
    {
        get;
        private set;
    }

    public static DecorationObjectChangedEventArgs Create(int areaId, int positionId, int type, bool isEnd, float animationTime)
    {
        DecorationObjectChangedEventArgs decorationObjectChangedEventArgs = GameFramework.ReferencePool.Acquire<DecorationObjectChangedEventArgs>();
        decorationObjectChangedEventArgs.AreaID = areaId;
        decorationObjectChangedEventArgs.PositionID = positionId;
        decorationObjectChangedEventArgs.AnimationType = type;
        decorationObjectChangedEventArgs.IsEnd = isEnd;
        decorationObjectChangedEventArgs.AnimationTime = animationTime;
        return decorationObjectChangedEventArgs;
    }

    public override void Clear()
    {
        AreaID = 0;
        PositionID = 0;
        AnimationType = 0;
        IsEnd = false;
    }

}
