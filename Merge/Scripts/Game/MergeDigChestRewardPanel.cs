using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MergeDigChestRewardPanel : RewardPanel
{
    public GameObject chestObj;
    public SkeletonGraphic chest;
    public SkeletonGraphic chestTop;
    public RewardArea chestRewardArea;
    public GameObject chestOpenEffect;

    public override RewardArea CustomRewardArea => chestRewardArea;

    public override void OnShow(RewardArea rewardArea, Action onShowComplete)
    {
        transform.SetAsFirstSibling();
        blackBg.OnShow(0.6f);
        chestOpenEffect.SetActive(false);
        Transform titleTrans = titleText.transform;
        Transform tipTrans = ReferenceEquals(tipBg, null) ? tipText.transform : tipBg.transform;
        titleTrans.localScale = Vector3.zero;
        tipTrans.localScale = Vector3.zero;

        string chestAnimName = "yellow_open1";
        string chestTopAnimName = "yellow_open2";
        chest.Initialize(true);
        chestTop.Initialize(true);
        chest.AnimationState.SetAnimation(0, chestAnimName, false);
        chestTop.AnimationState.SetAnimation(0, chestTopAnimName, false);
        chest.timeScale = 0;
        chestTop.timeScale = 0;

        chest.gameObject.SetActive(true);
        chestTop.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        chestTop.gameObject.SetActive(true);

        chestTop.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutSine);
        chestObj.transform.DOLocalJump(new Vector3(0, -400f), 700f, 1, 0.6f).SetEase(Ease.InOutQuad).onComplete = () =>
        {
            chest.timeScale = 1;
            chestTop.timeScale = 1;

            GameManager.Task.AddDelayTriggerTask(0.3f, () =>
            {
                chestOpenEffect.SetActive(true);

                titleText.gameObject.SetActive(true);
                titleTrans.DOScale(1.1f, 0.2f).onComplete = () =>
                {
                    try
                    {
                        titleTrans.DOScale(1f, 0.2f);
                    }
                    catch (Exception e)
                    {
                        OnHide(true, null);
                        Debug.LogError(e.Message);
                    }
                };

                rewardArea.OnShow(() =>
                {
                    //if (!autoGetReward)
                    {
                        tipText.gameObject.SetActive(true);
                        if (!ReferenceEquals(tipBg, null))
                        {
                            tipBg.SetActive(true);
                        }
                        tipTrans.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
                    }
                    onShowComplete?.Invoke();
                });

                GameManager.Sound.PlayAudio(SoundType.SFX_magic_pop_open_02.ToString());

                GameManager.Task.AddDelayTriggerTask(0.4f, () =>
                {
                    GameManager.Sound.PlayAudio(SoundType.SFX_shopBuySuccess.ToString());
                });
            });
        };
    }

    public override void OnHide(bool quickHide, Action onHideComplete)
    {
        blackBg.OnHide(quickHide ? 0 : 0.2f);
        chestTop.gameObject.SetActive(false);

        if (!quickHide)
        {
            Transform titleTrans = titleText.transform;
            Transform tipTrans = ReferenceEquals(tipBg, null) ? tipText.transform : tipBg.transform;

            titleTrans.DOScale(new Vector3(1.05f, 1.05f), 0.2f).onComplete += () =>
            {
                titleTrans.DOScale(Vector3.zero, 0.2f).onComplete += () =>
                {
                    try
                    {
                        titleText.gameObject.SetActive(false);
                        titleTrans.localScale = Vector3.one;
                    }
                    catch (Exception e)
                    {
                        OnHide(true, null);
                        Debug.LogError(e.Message);
                    }
                };
            };

            tipTrans.DOScale(new Vector3(1.05f, 1.05f), 0.2f).onComplete += () =>
            {
                tipTrans.DOScale(Vector3.zero, 0.2f).onComplete += () =>
                {
                    try
                    {
                        tipText.gameObject.SetActive(false);
                        if (!ReferenceEquals(tipBg, null))
                        {
                            tipBg.SetActive(false);
                        }
                        tipTrans.localScale = Vector3.one;

                        onHideComplete?.Invoke();
                    }
                    catch (Exception e)
                    {
                        OnHide(true, null);
                        Debug.LogError(e.Message);
                    }
                };
            };
        }
        else
        {
            titleText.gameObject.SetActive(false);
            tipText.gameObject.SetActive(false);
            if (!ReferenceEquals(tipBg, null))
            {
                tipBg.SetActive(false);
            }

            onHideComplete?.Invoke();
        }
    }
}
