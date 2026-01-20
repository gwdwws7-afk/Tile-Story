using GameFramework.Event;

/// <summary>
/// 奖励广告奖励获取事件
/// </summary>
public sealed class RewardAdEarnedRewardEventArgs : GameEventArgs
{
    public static readonly int EventId = typeof(RewardAdEarnedRewardEventArgs).GetHashCode();

    public RewardAdEarnedRewardEventArgs()
    {
        EarnedReward = false;
        UserData = null;
    }

    public override int Id
    {
        get
        {
            return EventId;
        }
    }

    /// <summary>
    /// 能否获取奖励
    /// </summary>
    public bool EarnedReward
    {
        get;
        private set;
    }

    /// <summary>
    /// 用户自定义数据
    /// </summary>
    public object UserData
    {
        get;
        private set;
    }

    public static RewardAdEarnedRewardEventArgs Create(bool isUserEarnedReward, object userData)
    {
        RewardAdEarnedRewardEventArgs rewardAdEarnedRewardEventArgs = GameFramework.ReferencePool.Acquire<RewardAdEarnedRewardEventArgs>();
        rewardAdEarnedRewardEventArgs.EarnedReward = isUserEarnedReward;
        rewardAdEarnedRewardEventArgs.UserData = userData;
        return rewardAdEarnedRewardEventArgs;
    }

    public override void Clear()
    {
        EarnedReward = false;
        UserData = null;
    }
}
