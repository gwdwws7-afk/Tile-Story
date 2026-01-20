using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class HarvestKitchenChestRewardPanel : RewardPanel
{
    public SkeletonGraphic chest;
    public RewardArea chestRewardArea;

    public override RewardArea CustomRewardArea => chestRewardArea;

    public override void OnShow(RewardArea rewardArea, Action onShowComplete)
    {
        transform.SetAsFirstSibling();
        blackBg.OnShow(0.2f);

        chest.gameObject.SetActive(true);
        chest.Initialize(true);
        int chestId = HarvestKitchenManager.Instance.ChestId - 1;
        string skinName = "Box_Green";
        switch (chestId)
        {
            case 0:
                skinName = "Box_wood";
                break;
            case 1:
                skinName = "Box_Green";
                break;
            case 2:
                skinName = "Box_blue";
                break;
            case 3:
                skinName = "Box_Red";
                break;
            case 4:
                skinName = "Box_Purple";
                break;
        }
        chest.Skeleton.SetSkin(skinName);
        chest.AnimationState.SetAnimation(0, "OpenBox", false);

        GameManager.Task.AddDelayTriggerTask(0.6f, () =>
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
            UnityUtil.EVibatorType.Medium.PlayerVibrator();
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
