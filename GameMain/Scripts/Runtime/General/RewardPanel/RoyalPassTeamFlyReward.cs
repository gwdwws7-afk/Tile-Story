using Coffee.UIExtensions;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoyalPassTeamFlyReward : FlyReward
{

    public Image rewardImage;
    public UIParticle rewardBgEffect;

    public override string Name => "RoyalPassTeamFlyReward";
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

        Vector3 startPos = new Vector3(cachedTransform.position.x, cachedTransform.position.y, 0);
        Vector3 backPos = startPos + (startPos - targetPos).normalized * 0.1f;
        Vector3 startScale = cachedTransform.localScale;

        cachedTransform.DOMove(backPos, 0.2f).SetEase(Ease.OutSine).onComplete = () =>
        {
            cachedTransform.DOMove(targetPos, 0.36f).SetEase(Ease.InCubic);
            cachedTransform.DOScale(startScale * 0.7f, 0.33f).SetEase(Ease.InCubic).onComplete = () =>
              {
                  OnHide();

                  getRewardAnimFinish = true;

                  var receiver = RewardManager.Instance.GetReceiverByItemType(type);
                  receiver?.OnFlyEnd(type);

                  GameManager.Sound.PlayAudio("SFX_itemget");
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
