using UnityEngine;

/// <summary>
/// 飞行奖励对象
/// </summary>
public sealed class FlyRewardObject : ObjectBase
{
    public override void Release(bool isShutdown)
    {
        UnityUtility.UnloadInstance((GameObject)Target);
    }
}
