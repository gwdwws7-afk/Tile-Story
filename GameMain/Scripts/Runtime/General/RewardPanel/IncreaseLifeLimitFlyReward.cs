using Coffee.UIExtensions;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IncreaseLifeLimitFlyReward : FlyReward
{
    public override string Name => "IncreaseLifeLimitFlyReward";

    public Image rewardImage;
    public TextMeshProUGUI numText;
    public UIParticle rewardBgEffect;

    public override void OnInit(TotalItemData rewardType, int rewardNum, int rewardTypeCount, bool autoGetReward, RewardArea rewardArea)
    {
        base.OnInit(rewardType, rewardNum, rewardTypeCount, autoGetReward, rewardArea);

        ShowRewardBgEffect();
    }

    public override void OnShow(Action callback = null)
    {
        rewardImage.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

        gameObject.SetActive(true);

        rewardImage.transform.DOScale(1.1f, 0.16f).onComplete = () =>
        {
            rewardImage.transform.DOScale(0.9f, 0.2f).SetEase(Ease.InQuad).onComplete = () =>
            {
                rewardImage.transform.DOScale(1f, 0.2f).onComplete = () =>
                {
                    callback?.Invoke();
                };
            };
        };
    }

    public override IEnumerator ShowGetRewardAnim(TotalItemData type,Vector3 targetPos)
    {
        HideRewardBgEffect();

        yield return null;

        Vector3 originalScale = cachedTransform.localScale;

        cachedTransform.DOScale(originalScale * 1.1f, 0.1f).onComplete = () =>
        {
            cachedTransform.DOScale(originalScale * 0.7f, 0.1f).onComplete = () =>
            {
                cachedTransform.DOScale(originalScale * 0.8f, 0.1f);
                cachedTransform.DOMove(targetPos, 0.5f).SetEase(Ease.InQuad).onComplete = () =>
                {
                    OnHide();

                    if (RewardManager.Instance.LifeFlyReceiver != null)
                    {
                        RewardManager.Instance.LifeFlyReceiver.OnLifeFlyHit();
                        cachedTransform.localScale = originalScale;

                        GameManager.Task.AddDelayTriggerTask(0.2f, () =>
                        {
                            getRewardAnimFinish = true;
                        });

                        GameManager.Event.Fire(this, LifeNumChangeEventArgs.Create(1, RewardManager.Instance.LifeFlyReceiver));
                        RewardManager.Instance.LifeFlyReceiver.OnLifeFlyEnd();

                        GameManager.Sound.PlayAudio("SFX_itemget");
                    }
                    else
                    {
                        getRewardAnimFinish = true;
                    }
                };
            };
        };
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
}
