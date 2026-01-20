using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 道具接收对象接口
/// </summary>
public interface IItemFlyReceiver
{
    ReceiverType ReceiverType { get; }
    GameObject GetReceiverGameObject();
    void OnFlyHit(TotalItemData type);

    void OnFlyEnd(TotalItemData type);

    Vector3 GetItemTargetPos(TotalItemData type);
}

[Flags]
public enum ReceiverType
{
    None=0,
    Common=1,
    Coin=2,
    Star=4,
    Gold=8,
    Boost1=16,
    Boost2=32,
    Boost3=64,
    Pickaxe=128,
    MergeEnergyBox=256,
}