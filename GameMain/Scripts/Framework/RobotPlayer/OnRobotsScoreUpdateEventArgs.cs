using System.Collections;
using System.Collections.Generic;
using GameFramework;
using GameFramework.Event;
using UnityEngine;

public class OnRobotsScoreUpdateEventArgs : GameEventArgs
{
    public static readonly int eventId = typeof(OnRobotsScoreUpdateEventArgs).GetHashCode();
    

    public override int Id => eventId;
    
    public RobotType RobotType { get; private set; }
    
    public static OnRobotsScoreUpdateEventArgs Create(RobotType robotType)
    {
        OnRobotsScoreUpdateEventArgs args = ReferencePool.Acquire<OnRobotsScoreUpdateEventArgs>();
        args.RobotType = robotType;
        return args;
    }
    public override void Clear()
    {
        
    }
}
