using UnityEngine;

/// <summary>
/// 旋转金币对象
/// </summary>
public sealed class RotateCoinObject : ObjectBase
{
    public override void Release(bool isShutdown)
    {
        UnityUtility.UnloadInstance((GameObject)Target);
    }
}
