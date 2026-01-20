using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class GiftRewardPanel : RewardPanel
{
    public SkeletonGraphic chest;
    public RewardArea chestRewardArea;
    public Transform chestTweenEndTrans;

    public override RewardArea CustomRewardArea => chestRewardArea;

    private string chestAnimName = "red";
    private float jumpPower = 0.2f;

    public override void OnShow(RewardArea rewardArea, Action onShowComplete)
    {
        transform.SetAsFirstSibling();
        blackBg.OnShow(1.0f);

        chest.transform.localScale = new Vector3(0.42f, 0.42f, 1f);
        var panel = GameManager.UI.GetUIForm("GameWellDonePanel") as GameWellDonePanel;
        if (panel != null)
        {
            chest.transform.position = panel.RewardBtnPos;
        }

        chest.gameObject.SetActive(true);
        chest.AnimationState.SetAnimation(0, chestAnimName, false);
        chest.freeze = true;

        chest.transform.DOJump(chestTweenEndTrans.position, jumpPower, 1, 0.5f);
        chest.transform.DOScale(Vector3.one, 0.5f);

        GameManager.Task.AddDelayTriggerTask(0.5f, () =>
        {
            chest.transform.position = chestTweenEndTrans.position;
            chest.transform.DOScale(new Vector3(1.1f, 0.9f, 1f), 0.13f).onComplete = () =>
            {
                chest.transform.DOScale(new Vector3(0.95f, 1.05f, 1f), 0.13f).onComplete = () =>
                {
                    chest.transform.DOScale(1f, 0.13f);
                };
            };
        });

        GameManager.Task.AddDelayTriggerTask(0.9f, () =>
        {
            chest.AnimationState.SetAnimation(0, chestAnimName, false);
            chest.freeze = false;
        });

        GameManager.Task.AddDelayTriggerTask(1.25f, () =>
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

        //GameManager.Task.AddDelayTriggerTask(1.3f, () =>
        //{
        //    GameManager.Sound.PlayAudio(SoundType.SFX_magic_pop_open_02.ToString());
        //});

        //GameManager.Task.AddDelayTriggerTask(1.7f, () =>
        //{
        //    GameManager.Sound.PlayAudio(SoundType.SFX_shopBuySuccess.ToString());
        //});

        //GameManager.Sound.PlayAudio(SoundType.SFX_DigTreasure_Active_Reward_Show.ToString());
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
