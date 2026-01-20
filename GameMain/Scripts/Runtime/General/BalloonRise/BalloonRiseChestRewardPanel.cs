using DG.Tweening;
using Spine.Unity;
using System;
using UnityEngine;

public class BalloonRiseChestRewardPanel : RewardPanel
{
    public SkeletonGraphic chest;
    public RewardArea chestRewardArea;

    public override RewardArea CustomRewardArea => chestRewardArea;

    public override void OnShow(RewardArea rewardArea, Action onShowComplete)
    {
        transform.SetAsFirstSibling();
        blackBg.OnShow(0.2f);

        chest.Initialize(false);
        int stage = GameManager.Task.BalloonRiseManager.Stage;
        string skinName = "Box_wood";
        switch (stage)
        {
            case 1:
                skinName = "Box_wood";
                break;
            case 2:
                skinName = "Box_blue";
                break;
            case 3:
                skinName = "Box_Red";
                break;
        }
        chest.Skeleton.SetSkin(skinName);
        chest.AnimationState.SetAnimation(0, "OpenBox", false).AnimationStart = 0.02f;
        chest.freeze = true;

        Transform chestTrans = chest.transform.parent;
        chestTrans.position = GameManager.DataNode.GetData<Vector3>("BalloonChestPos", Vector3.zero);
        chestTrans.localScale = Vector3.one;
        chest.gameObject.SetActive(true);
        
        chestTrans.DOScale(1.2f, 0.4f).SetEase(Ease.InOutQuad);
        chestTrans.DOLocalJump(new Vector3(0, -350, 0), 500f, 1, 0.55f).SetEase(Ease.InOutQuad);
        GameManager.UI.HideUIForm("BalloonRiseMainMenu");

        GameManager.Task.AddDelayTriggerTask(0.5f, () =>
        {
            chest.freeze = false;
        });

        GameManager.Task.AddDelayTriggerTask(0.9f, () =>
        {
            Transform titleTrans = titleText.transform;
            Transform tipTrans = ReferenceEquals(tipBg, null) ? tipText.transform : tipBg.transform;
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

                    onShowComplete?.Invoke();
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

                    onHideComplete?.Invoke();
                };
            };
        }
        else
        {
            titleText.gameObject.SetActive(false);
            tipText.gameObject.SetActive(false);

            onHideComplete?.Invoke();
        }
    }
}


