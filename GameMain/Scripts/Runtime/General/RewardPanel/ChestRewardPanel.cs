using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ChestRewardPanel : RewardPanel
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
        chest.AnimationState.SetAnimation(0, "red", false);

        GameManager.Task.AddDelayTriggerTask(0.42f, () =>
        {
            //Transform titleTrans = titleText.transform;
            Transform tipTrans = tipText.transform;
            //titleTrans.localScale = Vector3.zero;
            tipTrans.localScale = Vector3.zero;

            //titleText.gameObject.SetActive(true);
            //titleTrans.DOScale(1.1f, 0.2f).onComplete = () =>
            //{
            //    rewardArea.OnShow(() =>
            //    {
            //        if (!autoGetReward)
            //        {
            //            tipText.gameObject.SetActive(true);
            //            tipTrans.DOScale(1.1f, 0.2f).onComplete = () =>
            //            {
            //                tipTrans.DOScale(1f, 0.2f);
            //            };
            //        }

            //        onShowComplete?.Invoke();
            //    });

            //    titleTrans.DOScale(1f, 0.2f);
            //};

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

                onShowComplete?.Invoke();
            });
        });
    }

    public override void OnHide(bool quickHide, Action onHideComplete)
    {
        chest.gameObject.SetActive(false);

        blackBg.OnHide(quickHide ? 0 : 0.2f);

        if (!quickHide)
        {
            Transform tipTrans = tipText.transform;

            tipTrans.DOScale(new Vector3(1.05f, 1.05f), 0.2f).onComplete += () =>
            {
                tipTrans.DOScale(Vector3.zero, 0.2f).onComplete += () =>
                {
                    tipText.gameObject.SetActive(false);
                    tipTrans.localScale = Vector3.one;

                    onHideComplete?.Invoke();
                };
            };
        }
        else
        {
            tipText.gameObject.SetActive(false);

            onHideComplete?.Invoke();
        }
    }
}
