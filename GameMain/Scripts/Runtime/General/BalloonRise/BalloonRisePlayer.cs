using System;
using UnityEngine;
using Newtonsoft.Json;

public class BalloonRisePlayerBase
{
    public string SerialNumber;
    public string Name;
    public int Avatar;
    public DateTime UpdateTime;
    public int Rank;
    protected float RealScore;
    [JsonIgnore]
    public int Score
    {
        get => Mathf.Clamp(Mathf.RoundToInt(RealScore), 0, GameManager.Task.BalloonRiseManager.StageTarget);
        set => RealScore = value;
    }
    public int RecordScore;
    public virtual bool IsRobot { get; }
    public virtual bool IsSelf { get; }
}

public class BalloonRisePlayer : BalloonRisePlayerBase
{
    public override bool IsRobot => false;
    public override bool IsSelf => true;
}


// public class BalloonRiseRobotPlayer : BalloonRisePlayerBase
// {
//     
// }

// public class BalloonRiseRobot: IRobotPlayerBase
// {
//     public BalloonRiseRobotPlayer data;
//     public BalloonRiseRobot()
//     {
//         data = new BalloonRiseRobotPlayer();
//     }
//
//     public void UpdateByTime(float elapseSeconds, float realElapseSeconds)
//     {
//         data.Score = 1;
//     }
// }
