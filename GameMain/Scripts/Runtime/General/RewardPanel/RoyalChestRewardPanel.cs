using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoyalChestRewardPanel : RewardPanel
{
    public SkeletonGraphic chest;
    public RewardArea chestRewardArea;

    public override RewardArea CustomRewardArea => chestRewardArea;

    public override void OnShow(RewardArea rewardArea, Action onShowComplete)
    {
        transform.SetAsFirstSibling();
        blackBg.OnShow();

        chest.gameObject.SetActive(true);
        chest.Initialize(false);
        //chest.AnimationState.SetAnimation(0, "01", false);
        chest.AnimationState.SetAnimation(0, "active_fall", false);

        GameManager.Task.AddDelayTriggerTask(1.6f, () =>
        {
            GameManager.Sound.PlayAudio("SFX_Bonus_Bank_Open");
            chest.AnimationState.SetAnimation(0, "active_open", false).Complete += entry =>
            {
                chest.AnimationState.SetAnimation(0, "idle_shine", true);
            };
        });

        GameManager.Task.AddDelayTriggerTask(0.2f, () =>
        {
            Transform titleTrans = titleText.transform;
            Transform tipTrans = tipText.transform;
            titleTrans.localScale = Vector3.zero;
            tipTrans.localScale = Vector3.zero;

            titleText.gameObject.SetActive(true);
            titleTrans.DOScale(1.1f, 0.2f).onComplete = () =>
            {
                rewardArea.OnShow(() =>
                {
                    if (!autoGetReward)
                    {
                        tipText.gameObject.SetActive(true);
                        tipTrans.DOScale(1.1f, 0.2f).onComplete = () =>
                        {
                            tipTrans.DOScale(1f, 0.2f);
                        };
                    }

                    onShowComplete?.InvokeSafely();
                });

                titleTrans.DOScale(1f, 0.2f);
            };
        });
    }

    public override void OnHide(bool quickHide, Action onHideComplete)
    {
        chest.gameObject.SetActive(false);

        blackBg.OnHide(quickHide ? 0 : 0.2f);

        if (!quickHide)
        {
            Transform titleTrans = titleText.transform;
            Transform tipTrans = tipText.transform;

            titleTrans.DOScale(new Vector3(1.05f, 1.05f), 0.2f).onComplete += () =>
            {
                titleTrans.DOScale(Vector3.zero, 0.2f).onComplete += () =>
                {
                    titleText.gameObject.SetActive(false);
                    titleTrans.localScale = Vector3.one;
                };
            };

            tipTrans.DOScale(new Vector3(1.05f, 1.05f), 0.2f).onComplete += () =>
            {
                tipTrans.DOScale(Vector3.zero, 0.2f).onComplete += () =>
                {
                    tipText.gameObject.SetActive(false);
                    tipTrans.localScale = Vector3.one;

                    onHideComplete?.InvokeSafely();
                };
            };
        }
        else
        {
            titleText.gameObject.SetActive(false);
            tipText.gameObject.SetActive(false);

            onHideComplete?.InvokeSafely();
        }
    }
}
