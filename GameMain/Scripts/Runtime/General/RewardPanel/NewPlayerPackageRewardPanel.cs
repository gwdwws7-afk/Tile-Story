using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewPlayerPackageRewardPanel : RewardPanel
{
    public SkeletonGraphic chest;
    public RewardArea chestRewardArea;
    public GameObject chestOpenEffect;

    public override RewardArea CustomRewardArea => chestRewardArea;

    public override void OnShow(RewardArea rewardArea, Action onShowComplete)
    {
        transform.SetAsFirstSibling();
        blackBg.OnShow();

        
        chestOpenEffect.SetActive(false);
        chest.Initialize(true);
        chest.gameObject.SetActive(true);
        chest.AnimationState.SetAnimation(0, "yellow", false);

        GameManager.Task.AddDelayTriggerTask(0.43333f, () =>
        {
            Transform tipTrans = tipText.transform;
            tipTrans.localScale = Vector3.zero;

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


        GameManager.Task.AddDelayTriggerTask(0.3666f, () =>
        {
            chestOpenEffect.SetActive(true);
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
