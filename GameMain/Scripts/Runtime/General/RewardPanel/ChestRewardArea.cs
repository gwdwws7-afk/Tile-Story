using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestRewardArea : RewardArea
{
    public Transform startPos;
    public float delayTime = 0.4f;
    public float deltaY = 200;

    public override void OnShow(Action callback = null)
    {
        SortRewardFlyObjects(rewardFlyObjects);

        StartCoroutine(ShowChestRewardAnim(callback));
    }

    IEnumerator ShowChestRewardAnim(Action callback)
    {
        yield return new WaitForSeconds(delayTime);

        for (int i = 0; i < rewardFlyObjects.Count; i++)
        {
            FlyReward flyObject = rewardFlyObjects[i];

            Vector3 originalScale = flyObject.CachedTransform.localScale;
            flyObject.CachedTransform.localScale = new Vector3(originalScale.x * 0.5f, originalScale.y * 0.5f);
            flyObject.CachedTransform.position = startPos.position;
            flyObject.gameObject.SetActive(true);

            Vector3 finalPos = GetRewardLocalPosition(i);
            Vector3 direction = (finalPos - flyObject.CachedTransform.localPosition).normalized;

            flyObject.CachedTransform.DOScale(originalScale, 0.15f).onComplete = () =>{};

            flyObject.CachedTransform.DOLocalMove(finalPos + direction * 35, 0.2f).onComplete = () =>
            {
                flyObject.CachedTransform.DOLocalMove(finalPos, 0.2f);
            };

            if (i == rewardFlyObjects.Count - 1)
            {
                yield return new WaitForSeconds(0.6f);

                callback?.Invoke();
            }
        }
    }

    protected override Vector3 GetRewardLocalPosition(int index)
    {
        var pos = base.GetRewardLocalPosition(index);
        return new Vector3(pos.x, pos.y + deltaY, pos.z);
    }
}
