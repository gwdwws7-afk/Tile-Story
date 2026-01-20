using GameFramework.Event;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 奖励广告加载完毕事件
/// </summary>
public sealed class RewardAdLoadCompleteEventArgs : GameEventArgs
{
    public static readonly int EventId = typeof(RewardAdLoadCompleteEventArgs).GetHashCode();

    public RewardAdLoadCompleteEventArgs()
    {
    }

    public override int Id
    {
        get
        {
            return EventId;
        }
    }

    public static RewardAdLoadCompleteEventArgs Create()
    {
        RewardAdLoadCompleteEventArgs rewardAdLoadCompleteEventArgs = GameFramework.ReferencePool.Acquire<RewardAdLoadCompleteEventArgs>();
        return rewardAdLoadCompleteEventArgs;
    }

    public override void Clear()
    {
    }
}
