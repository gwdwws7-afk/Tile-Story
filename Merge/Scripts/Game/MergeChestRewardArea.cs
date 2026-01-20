using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class MergeChestRewardArea : RewardArea
{
    public Transform startPos;
    public float delayTime = 0.4f;
    public float deltaY = 200;

    public override void OnShow(Action callback = null)
    {
        int TileIndex = -1;
        for (int i = 0; i < rewardFlyObjects.Count; i++)
        {
            if (rewardFlyObjects[i].RewardType.TotalItemType == TotalItemType.Item_TileID)
            {
                TileIndex = i;
                break;
            }
        }

        if (TileIndex != -1 && TileIndex != 3 && rewardFlyObjects.Count == 7)
        {
            var flyReward = rewardFlyObjects[3];
            rewardFlyObjects[3] = rewardFlyObjects[TileIndex];
            rewardFlyObjects[TileIndex] = flyReward;
        }

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
            flyObject.CachedTransform.position = startPos.position + new Vector3(0, 0.1f, 0);
            flyObject.gameObject.SetActive(true);

            Vector3 finalPos = GetRewardLocalPosition(i);
            Vector3 direction = (finalPos - flyObject.CachedTransform.localPosition).normalized;

            flyObject.CachedTransform.DOScale(originalScale, 0.15f).onComplete = () => { };

            flyObject.CachedTransform.DOLocalMove(finalPos + direction * 35, 0.2f).onComplete = () =>
            {
                flyObject.CachedTransform.DOLocalMove(finalPos, 0.2f);
            };

            if (i == rewardFlyObjects.Count - 1)
            {
                yield return new WaitForSeconds(0.6f);

                callback?.InvokeSafely();
            }
        }
    }

    protected override Vector3 GetRewardLocalPosition(int index)
    {
        var pos = base.GetRewardLocalPosition(index);
        return new Vector3(pos.x, pos.y + deltaY, pos.z);
    }
}
