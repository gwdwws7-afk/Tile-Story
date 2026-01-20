using Coffee.UIExtensions;
using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 生命飞行奖励
/// </summary>
public sealed class LifeFlyReward : FlyReward
{
    public override string Name => "LifeFlyReward";

    public override int SortPriority => 8;

    public GameObject normalImage;
    public GameObject infiniteImage;
    public TextMeshProUGUI numText;
    public UIParticle rewardBgEffect;
    public GameObject numberChangeEffect;

    public override void OnInit(TotalItemData rewardType, int rewardNum, int rewardTypeCount, bool autoGetReward, RewardArea rewardArea)
    {
        base.OnInit(rewardType, rewardNum, rewardTypeCount, autoGetReward, rewardArea);

        ShowRewardBgEffect();

        cachedTransform.localScale = Vector3.one;
        normalImage.SetActive(rewardType == TotalItemData.Life);
        infiniteImage.SetActive(rewardType == TotalItemData.InfiniteLifeTime);
    }

    public override void OnReset()
    {
        numberChangeEffect.SetActive(false);

        base.OnReset();
    }

    public override void OnShow(Action callback = null)
    {
        Transform trans = body;
        float originalScale = trans.localScale.x;

        trans.transform.localScale = new Vector3(originalScale * 0.4f, originalScale * 0.4f, originalScale * 0.4f);

        gameObject.SetActive(true);

        trans.DOScale(originalScale * 1.05f, 0.16f).onComplete = () =>
        {
            trans.DOScale(originalScale, 0.2f);
        };

        GameManager.Task.AddDelayTriggerTask(0.36f, () =>
        {
            callback?.Invoke();
        });
    }

    public override void RefreshAmountText()
    {
        if (rewardType == TotalItemData.Life) 
        {
            numText.SetText("x " + rewardNum);
        }
        else if (rewardType == TotalItemData.InfiniteLifeTime)
        {
            if (rewardNum < 60)
                numText.SetText(rewardNum + " m");
            else
                numText.SetText(rewardNum / 60f + " h");
        }
        normalImage.SetActive(rewardType == TotalItemData.Life);
        infiniteImage.SetActive(rewardType == TotalItemData.InfiniteLifeTime);
    }

    public override IEnumerator ShowGetRewardAnim(TotalItemData type,Vector3 targetPos)
    {
        HideRewardBgEffect();

        GameManager.Task.AddDelayTriggerTask(0.95f, () =>
        {
            getRewardAnimFinish = true;
        });

        yield return null;

        Vector3 originalScale = cachedTransform.localScale;

        cachedTransform.DOScale(originalScale * 1.05f, 0.1f).onComplete = () =>
        {
            cachedTransform.DOScale(originalScale * 0.62f, 0.1f).SetEase(Ease.InQuad).onComplete = () =>
            {
                cachedTransform.DOScale(originalScale * 0.7f, 0.1f);
                cachedTransform.DOMove(targetPos, 0.5f).SetEase(Ease.InCubic).onComplete = () =>
                {
                    OnHide();

                    if (RewardManager.Instance.LifeFlyReceiver != null)
                    {
                        RewardManager.Instance.LifeFlyReceiver.OnLifeFlyHit();
                        cachedTransform.localScale = originalScale;

                        //GameManager.Task.AddDelayTriggerTask(0.2f, () =>
                        //{
                        //    getRewardAnimFinish = true;
                        //});

                        GameManager.Event.Fire(this, LifeNumChangeEventArgs.Create(1, RewardManager.Instance.LifeFlyReceiver));
                        RewardManager.Instance.LifeFlyReceiver.OnLifeFlyEnd();

                        GameManager.Sound.PlayAudio("SFX_itemget");
                        UnityUtil.EVibatorType.VeryShort.PlayerVibrator();
                    }
                    else
                    {
                        getRewardAnimFinish = true;
                    }
                };
            };
        };
    }

    public override void DoubleRefreshAmountText()
    {
        numText.transform.DOScale(1.6f, 0.2f).SetDelay(0.2f).onComplete = () =>
        {
            numText.SetItemText(rewardNum, rewardType, false);

            numText.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        };

        numberChangeEffect.SetActive(true);
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
