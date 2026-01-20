using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;

public class CardFinalChestRewardPanel : RewardPanel
{
    public SkeletonGraphic chest;
    public ChestRewardArea rewardArea;

    public override RewardArea CustomRewardArea => rewardArea;
    
    public override void OnInit(bool autoGetReward)
    {
        base.OnInit(autoGetReward);
        
        titleText.transform.localScale = Vector3.zero;
        tipText.transform.localScale = Vector3.zero;
        chest.transform.localScale = Vector3.zero;
        chest.transform.localPosition = Vector3.zero;
        
        chest.AnimationState.TimeScale = 0;
        chest.AnimationState.ClearTracks();
        
        UnityUtil.EVibatorType.Success.PlayerVibrator();
        chest.AnimationState.SetAnimation(0, "open2", false);
        chest.Update(0);
    }

    public override void OnShow(RewardArea rewardArea, Action onShowComplete)
    {
        transform.SetAsFirstSibling();
        blackBg.OnShow(0.2f, false);
        
        titleText.transform.DOScale(1.1f, 0.2f).onComplete = () =>
        {
            titleText.transform.DOScale(1f, 0.2f);
            GameManager.Sound.PlayAudio("Card_Collection_Final_Box_Reward_Show");
            chest.transform.DOScale(1.3f, 0.2f);
            chest.transform.DOLocalMoveY(-484, 0.2f).SetDelay(0.1f).onComplete = () =>
            {
                GameManager.Sound.PlayAudio("SFX_magic_pop_open_02");
                chest.AnimationState.TimeScale = 1;
                rewardArea.OnShow(() =>
                {
                    GameManager.Sound.PlayAudio("SFX_shopBuySuccess");
                    tipText.transform.DOScale(1.1f, 0.2f).onComplete = () =>
                    {
                        tipText.transform.DOScale(1f, 0.2f);
                        blackBg.clickButton.interactable = true;
                    };
                    
                    onShowComplete?.InvokeSafely();
                });
            };
        };
    }

    public override void OnHide(bool quickHide, Action onHideComplete)
    {
        blackBg.OnHide(quickHide ? 0f : 0.2f);
        titleText.transform.DOScale(0f, quickHide ? 0f : 0.2f);
        tipText.transform.DOScale(0f, quickHide ? 0f : 0.2f);
        chest.transform.DOScale(0f, quickHide ? 0f : 0.2f).onComplete += () => onHideComplete?.InvokeSafely();
    }
}
