using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class DecorationChestRewardPanel : RewardPanel
{
    public SkeletonGraphic chest;
    public RewardArea chestRewardArea;
    public GameObject chestOpenEffect;

    public override RewardArea CustomRewardArea => chestRewardArea;

    public override void OnShow(RewardArea rewardArea, Action onShowComplete)
    {
        chestOpenEffect.SetActive(false);
        transform.SetAsFirstSibling();
        blackBg.OnShow();

        chest.gameObject.SetActive(true);
        chest.Initialize(true);
        chest.AnimationState.SetAnimation(0, $"active", false);

        GameManager.Task.AddDelayTriggerTask(0.8f, () =>
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

                    onShowComplete?.Invoke();
                });

                titleTrans.DOScale(1f, 0.2f);
            };
        });


        GameManager.Task.AddDelayTriggerTask(2f, () =>
        {
            GameManager.Sound.PlayAudio(SoundType.SFX_magic_pop_open_02.ToString());
            chestOpenEffect.SetActive(true);
            chest.transform.DOLocalMoveY(chest.transform.localPosition.y - 200, 0.2f);
        });


    }
    public override void OnHide(bool quickHide, Action onHideComplete)
    {
        chest.gameObject.SetActive(false);
        chest.transform.localPosition += new Vector3(0, 200);

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
