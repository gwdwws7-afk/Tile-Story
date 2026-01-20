using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 金币接收对象接口
/// </summary>
public interface ICoinFlyReceiver
{
    Vector3 CoinFlyTargetPos { get; }

    GameObject GetReceiverGameObject();

    int ShownNum { get; set; }

    void Show();

    void OnCoinFlyHit();

    void OnCoinFlyEnd();
}
