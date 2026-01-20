using DG.Tweening;
using Spine.Unity;
using System;
using UnityEngine;

public sealed class CalendarRewardPanel : RewardPanel
{
    public SkeletonGraphic chestBefore, chestBehind, chestSingle;
    public RewardArea chestRewardArea;
    public GameObject effect;

    public override RewardArea CustomRewardArea => chestRewardArea;

    public DateTime Date;
    public Vector3 ChestTweenEndPos;
    public override void OnShow(RewardArea rewardArea, Action onShowComplete)
    {
        transform.SetAsFirstSibling();
        blackBg.OnShow();
        effect.SetActive(false);
        gameObject.SetActive(true);
        chestSingle.gameObject.SetActive(false);
        chestBefore.gameObject.SetActive(false);
        chestBehind.gameObject.SetActive(false);
        if (Date.Month % 2 == 0)
        {
            chestBefore.Initialize(true);

            var rewardLevel = GameManager.DataNode.GetData("CalendarChallengeRewardLevel", 0) + 2;

            chestBefore.AnimationState.SetAnimation(0, $"{rewardLevel:D2}", false).TimeScale = 0;
            chestBefore.gameObject.SetActive(true);
            chestBefore.transform.DOScale(Vector3.one, 0.8f);
            chestBefore.transform.DOLocalJump(ChestTweenEndPos, 800f, 1, 0.8f).onComplete += () =>
            {
                chestBehind.gameObject.SetActive(true);
                chestBefore.Initialize(true);
                chestBehind.Initialize(true);
                chestBefore.AnimationState.SetAnimation(0, $"{rewardLevel:D2}", false).TimeScale = 1;
                chestBehind.AnimationState.SetAnimation(0, $"{rewardLevel:D2}-2", false).TimeScale = 1;
                GameManager.Task.AddDelayTriggerTask(0.35f, () =>
                {
                    effect.SetActive(true);
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
            };
        }
        else
        {
            chestSingle.Initialize(true);
            var rewardLevel = GameManager.DataNode.GetData("CalendarChallengeRewardLevel", 0) + 1;
            chestSingle.AnimationState.SetAnimation(0, $"{rewardLevel:D2}", false).TimeScale = 0;
            chestSingle.gameObject.SetActive(true);
            chestSingle.transform.DOScale(Vector3.one, 0.5f);
            chestSingle.transform.DOLocalJump(ChestTweenEndPos, 800f, 1, 0.5f).onComplete += () =>
            {
                chestSingle.Initialize(true);
                chestSingle.AnimationState.SetAnimation(0, $"{rewardLevel:D2}", false).TimeScale = 1;
                GameManager.Task.AddDelayTriggerTask(1.4f, () =>
                {
                    effect.SetActive(true);

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
            };
        }





    }

    public override void OnHide(bool quickHide, Action onHideComplete)
    {
        chestBefore.gameObject.SetActive(false);
        chestBehind.gameObject.SetActive(false);
        chestSingle.gameObject.SetActive(false);

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
