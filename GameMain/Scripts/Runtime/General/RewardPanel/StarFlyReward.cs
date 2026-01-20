using Coffee.UIExtensions;
using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public sealed class StarFlyReward : FlyReward
{
    public override string Name => "StarFlyReward";

    public override int SortPriority => 9;

    public Transform[] stars;
    public UIParticle rewardBgEffect;

    public override void OnInit(TotalItemData rewardType, int rewardNum, int rewardTypeCount, bool autoGetReward, RewardArea rewardArea)
    {
        base.OnInit(rewardType, rewardNum, rewardTypeCount, autoGetReward, rewardArea);

        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].localPosition = new Vector3(-50 * i, 0, 0);
            stars[i].localScale = Vector3.one;
            stars[i].localRotation = Quaternion.Euler(Vector3.zero);
            stars[i].gameObject.SetActive(i < rewardNum);
        }
    }

    public override void OnShow(Action callback = null)
    {
        Transform trans = body;
        float originalScale = trans.localScale.x;

        trans.localScale = new Vector3(originalScale * 0.4f, originalScale * 0.4f, originalScale * 0.4f);

        gameObject.SetActive(true);

        trans.DOScale(originalScale * 1.1f, 0.15f).onComplete = () =>
        {
            trans.DOScale(originalScale * 0.95f, 0.15f).SetEase(Ease.InQuad).onComplete = () =>
            {
                trans.DOScale(originalScale, 0.15f).onComplete = () =>
                {
                    try
                    {
                        callback?.Invoke();
                    }
                    catch (Exception e)
                    {
                        OnHide();
                        Log.Error("StarFlyReward OnShow error - {0}", e.Message);
                    }
                };
            };
        };
    }

    public override void OnRelease()
    {
        base.OnRelease();

        StopAllCoroutines();
        gameObject.SetActive(false);
    }

    public override void ShowRewardBgEffect()
    {
        if (rewardBgEffect != null)
        {
            rewardBgEffect.gameObject.SetActive(true);
        }
    }

    public override void HideRewardBgEffect()
    {
        if (rewardBgEffect != null)
        {
            rewardBgEffect.gameObject.SetActive(false);
        }
    }

    public override IEnumerator ShowGetRewardAnim(TotalItemData type, Vector3 targetPos)
    {
        HideRewardBgEffect();

        yield return new WaitForSeconds(0.25f);

        GameManager.Task.AddDelayTriggerTask(0.54f, () =>
        {
            getRewardAnimFinish = true;
        });

        if (rewardNum > stars.Length)
            rewardNum = stars.Length;
        float delayTime = 0f;
        for (int i = 0; i < rewardNum; i++)
        {
            int index = i;
            Transform cachedTrans = stars[index];
            cachedTrans.gameObject.SetActive(true);
            Vector3 startPos = new Vector3(cachedTrans.position.x, cachedTrans.position.y, 0);
            Vector3 backPos = startPos + (startPos - targetPos).normalized * 0.2f;
            delayTime = i * 0.2f;

            cachedTrans.DOBlendableLocalRotateBy(new Vector3(0, 0, -700), 0.6f, RotateMode.FastBeyond360).SetEase(Ease.Linear);
            cachedTrans.DOMove(backPos, 0.1f).SetDelay(delayTime).onComplete = () =>
            {
                cachedTrans.DOMove(targetPos, 0.36f).SetEase(Ease.InOutQuad);
                cachedTrans.DOScale(0.5f, 0.33f).SetEase(Ease.InOutQuad).onComplete = () =>
                {
                    cachedTrans.gameObject.SetActive(false);
                    if (index != rewardNum - 1)
                    {
                        var receiver = RewardManager.Instance.GetReceiverByItemType(type);
                        receiver?.OnFlyHit(type);
                    }
                };
            };

            if (index == rewardNum - 1)
            {
                GameManager.Task.AddDelayTriggerTask(0.45f + delayTime, () =>
                 {
                     try
                     {
                         OnHide();
                         getRewardAnimFinish = true;
                         var receiver = RewardManager.Instance.GetReceiverByItemType(type);
                         receiver?.OnFlyEnd(type);
                         GameManager.Sound.PlayAudio("SFX_itemget");
                     }
                     catch (Exception e)
                     {
                         OnHide();
                         Log.Error("StarFlyReward ShowGetRewardAnim error - {0}", e.Message);
                     }
                 });
            }
        }
    }
}
