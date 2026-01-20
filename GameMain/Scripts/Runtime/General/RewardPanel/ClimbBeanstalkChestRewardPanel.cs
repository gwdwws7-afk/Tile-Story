using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbBeanstalkChestRewardPanel : RewardPanel
{
    public SkeletonGraphic chest;
    public RewardArea chestRewardArea;
    public Transform chestTweenEndTrans;
    public GameObject chestOpenEffect;

    public override RewardArea CustomRewardArea => chestRewardArea;

    private string chestAnimName = "01";
    private float jumpPower = 0.2f;
    private Action OnShowCallback;

    public override void OnShow(RewardArea rewardArea, Action onShowComplete)
    {
        transform.SetAsFirstSibling();
        blackBg.OnShow(1.0f);
        chestOpenEffect.SetActive(false);

        chest.gameObject.SetActive(true);
        chest.Initialize(true);
        chest.AnimationState.SetAnimation(0, chestAnimName, false);

        chest.transform.DOJump(chestTweenEndTrans.position, jumpPower, 1, 0.5f);
        chest.transform.DOScale(Vector3.one, 0.5f);

        OnShowCallback?.Invoke();
        OnShowCallback = null;

        GameManager.Task.AddDelayTriggerTask(0.2f, () =>
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

        GameManager.Task.AddDelayTriggerTask(1.3f, () =>
        {
            chestOpenEffect.SetActive(true);

            GameManager.Sound.PlayAudio(SoundType.SFX_magic_pop_open_02.ToString());
        });

        GameManager.Task.AddDelayTriggerTask(1.7f, () =>
        {
            GameManager.Sound.PlayAudio(SoundType.SFX_shopBuySuccess.ToString());
        });

        GameManager.Sound.PlayAudio(SoundType.SFX_DigTreasure_Active_Reward_Show.ToString());
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

    public void SetChestTypeAndPosition(string inputAnimName, Vector3 inputGlobalPosition, float scaleRatio = 0.6f, float jumpPower = 0.2f)
    {
        chestAnimName = inputAnimName;
        chest.transform.position = inputGlobalPosition;
        chest.transform.localScale = Vector3.one * scaleRatio;
        this.jumpPower = jumpPower;
    }

    public void SetOnShowCallback(Action inputCallback)
    {
        OnShowCallback = inputCallback;
    }
}
