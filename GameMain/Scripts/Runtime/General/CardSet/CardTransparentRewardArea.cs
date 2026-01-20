using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardTransparentRewardArea : RewardArea
{
    public override void OnShow(Action callback = null)
    {
        foreach (var reward in rewardFlyObjects)
        {
            reward.CachedTransform.localPosition = Vector3.zero;
            reward.OnShow();
        }
        callback?.InvokeSafely();
    }
}
