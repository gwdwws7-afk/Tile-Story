using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using Spine;
using UnityEngine;
using UnityEngine.UI;

public class ClimbBeanstalkLevelChestRewardPanel : RewardPanel
{
    public SkeletonGraphic chest;
    public SkeletonGraphic hammer;
    public RewardArea chestRewardArea;
    public Transform chestTweenEndTrans;
    public GameObject chestOpenEffect;
    public GameObject[] eggOpenEffect;
    public Button claimBtn;

    public override RewardArea CustomRewardArea => chestRewardArea;

    private int effectIndex = 0;
    private string chestAnimName = "idle1";
    private Action OnShowCallback;

    public override void OnShow(RewardArea rewardArea, Action onShowComplete)
    {
        transform.SetAsFirstSibling();
        blackBg.OnShow(1.0f);
        chestOpenEffect.SetActive(false);

        chest.gameObject.SetActive(true);
        chest.Initialize(true);
        chest.AnimationState.SetAnimation(0, chestAnimName, true);

        chest.transform.DOJump(chestTweenEndTrans.position, 0.2f, 1, 0.5f);
        chest.transform.DOScale(Vector3.one, 1.0f);

        OnShowCallback?.Invoke();
        OnShowCallback = null;

        GameManager.Task.AddDelayTriggerTask(0.2f, () =>
        {
            Transform titleTrans = titleText.transform;
            Transform tipTrans = tipText.transform;
            titleTrans.localScale = Vector3.zero;
            tipTrans.localScale = Vector3.zero;

            titleText.gameObject.SetActive(true);
            tipTrans.gameObject.SetActive(true);
            tipTrans.DOScale(1.1f, 0.2f);
            titleTrans.DOScale(1.1f, 0.2f).onComplete = () =>
            {
                claimBtn.gameObject.SetActive(true);
                claimBtn.SetBtnEvent(() =>
                {
                    claimBtn.gameObject.SetActive(false);
                    hammer.gameObject.SetActive(true);
                    hammer.AnimationState.SetAnimation(0, "idle", false);
                    //延时等待锤子动画
                    GameManager.Task.AddDelayTriggerTask(0.5f, () =>
                    {
                        chest.gameObject.SetActive(false);
                        //播放特效
                        chestOpenEffect.SetActive(true);
                        GameManager.Sound.PlayAudio("SFX_Easter_Egg");
                        eggOpenEffect[effectIndex].SetActive(true);
                        chestOpenEffect.transform.parent.GetComponent<UIParticle>().RefreshParticles();
                        chestOpenEffect.transform.parent.GetComponent<UIParticle>().Play();
                        //展示奖励
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
                    });
                });
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

    public void SetChestTypeAndPosition(int index, string inputAnimName, Vector3 inputGlobalPosition)
    {
        effectIndex = index;
        chestAnimName = inputAnimName;
        chest.transform.position = inputGlobalPosition;
        chest.transform.localScale = Vector3.one * 0.6f;
    }

    public void SetOnShowCallback(Action inputCallback)
    {
        OnShowCallback = inputCallback;
    }
}
