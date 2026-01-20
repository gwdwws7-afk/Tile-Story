using UnityEngine;

/// <summary>
/// 奖励接收对象接口
/// </summary>
public interface IRewardFlyReceiver
{
    Vector3 RewardFlyTargetPos { get; }

    void OnRewardFlyEnd();
}
