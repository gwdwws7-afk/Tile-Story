using DG.Tweening;
using Spine.Unity;
using System;
using UnityEngine;

public class MergeChestRewardPanel : RewardPanel
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
        chest.AnimationState.SetAnimation(0, "open", false);

        GameManager.Task.AddDelayTriggerTask(0.4f, () =>
        {
            GameManager.Sound.PlayAudio(SoundType.SFX_magic_pop_open_02.ToString());
        });

        titleText.gameObject.SetActive(true);
        GameManager.Task.AddDelayTriggerTask(0.62f, () =>
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

                onShowComplete?.InvokeSafely();
            });

            //GameManager.Sound.PlayAudio(SoundType.SFX_shopBuySuccess.ToString());
        });
    }

    public override void OnHide(bool quickHide, Action onHideComplete)
    {
        chest.gameObject.SetActive(false);
        titleText.gameObject.SetActive(false);

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

                    onHideComplete?.InvokeSafely();
                };
            };
        }
        else
        {
            tipText.gameObject.SetActive(false);

            onHideComplete?.InvokeSafely();
        }
    }
}
