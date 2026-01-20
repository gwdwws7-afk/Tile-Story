using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GameFramework.Event;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using AnimationState = Spine.AnimationState;

public class TurntableRewardPanel : RewardPanel
{
    public DelayButton claimButton, adsButton;
    public Transform title;

    public SkeletonGraphic spine;
    // public CoinBarManager coinBar;
    private RewardArea _rewardArea;
    private bool isCoinSpin;
    [SerializeField]private GameObject chestOpenEffect;

    public override void OnInit(bool autoGetReward)
    {
        chestOpenEffect.SetActive(false);
        autoGetReward = false;
        spine.gameObject.SetActive(false);
        isCoinSpin = GameManager.DataNode.GetData<bool>("NormalTurntableRewardIsCoin", false);
        claimButton.SetBtnEvent(OnClaimButtonClick);
        adsButton.SetBtnEvent(OnAdsButtonClick);
        // coinBar.OnInit(null, null,null);
        GameManager.Event.Subscribe(RewardAdLoadCompleteEventArgs.EventId, OnRewardAdLoadComplete);
        GameManager.Event.Subscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarnedReward);
        base.OnInit(autoGetReward);
    }

    public override void OnRelease()
    {
        // coinBar.OnRelease();
        base.OnRelease();
    }

    public override void OnShow(RewardArea rewardArea, Action onShowComplete)
    {
        chestOpenEffect.SetActive(false);
        
        _rewardArea = rewardArea;
        transform.SetAsFirstSibling();
        blackBg.OnShow();
        if (RewardManager.Instance.ForceHideRewardPanelBg)
        {
            blackBg.GetComponent<Image>().color = new Color(1, 1, 1, 0f);
        }

        // rewardArea.OnShow(() =>
        // {
        //     SetAdsButtonState(false);
        //
        //     onShowComplete?.Invoke();
        // });

        // var hasCoin = rewardArea.RewardsDic.ContainsKey(TotalItemData.Coin);
        title.localScale = Vector3.zero;
        claimButton.gameObject.SetActive(false);
        adsButton.gameObject.SetActive(false);
        title.gameObject.SetActive(true);
        
        title.DOScale(1.1f, 0.2f).onComplete = () =>
        {
            try
            {
                title.DOScale(1f, 0.2f);
                ShowJackpot(rewardArea, onShowComplete);
                // SetAdsBuelsttonState();
            }
            catch (Exception e)
            {
                OnHide(true, null);
                Debug.LogError(e.Message);
            }
        };
    }

    private void ShowJackpot(RewardArea rewardArea, Action onShowComplete)
    {
        if (rewardArea.RewardTypeCount > 1)
        {
            spine.transform.localScale = Vector3.one;
            spine.Initialize(true);
            spine.gameObject.SetActive(true);
            var anim = spine.AnimationState.SetAnimation(0, "active", false);
            anim.TimeScale = 1f;
            anim.Complete += (t) =>
            {
                spine.transform.DOScale(Vector3.zero,0.1f).onComplete += () =>
                {
                    spine.gameObject.SetActive(false);
                    rewardArea.OnShow(() =>
                    {
                        SetAdsButtonState(false);

                        onShowComplete?.Invoke();
                    });
                };
            };
            GameManager.Task.AddDelayTriggerTask(1.5f, () =>
            {
                chestOpenEffect.SetActive(true);
                // ShowEffect();
            });
        }
        else
        {
            rewardArea.OnShow(() =>
            {
                SetAdsButtonState(false);

                onShowComplete?.Invoke();
            });
        }
    }
    
    private void ShowEffect()
    {
        GameManager.ObjectPool.SpawnWithRecycle<EffectObject>(
            "TileWellDone",
            "TileItemDestroyEffectPool",
            2.2f,
            transform.position,
            transform.rotation,
            transform, (t) =>
            {
                var effect = t?.Target as GameObject;
                if (effect != null)
                {
                    var skeleton = effect.transform.GetComponent<SkeletonGraphic>();
                    if (skeleton != null)
                    {
                        skeleton.AnimationState.ClearTracks();
                        skeleton.Skeleton.SetToSetupPose();
                        skeleton.AnimationState.SetAnimation(0, "active", false);
                    }
                }
            });
    }

    public override void OnHide(bool quickHide, Action onHideComplete)
    {
        blackBg.OnHide(quickHide ? 0 : 0.4f);

        if (!quickHide)
        {
            title.DOScale(new Vector3(1.05f, 1.05f), 0.2f).onComplete += () =>
            {
                title.DOScale(Vector3.zero, 0.2f).onComplete += () =>
                {
                    try
                    {
                        title.gameObject.SetActive(false);
                        title.localScale = Vector3.one;
                        onHideComplete?.Invoke();
                    }
                    catch (Exception e)
                    {
                        OnHide(true, null);
                        Debug.LogError(e.Message);
                    }
                };
                claimButton.transform.DOScale(Vector3.zero, 0.2f).onComplete += () =>
                {
                    claimButton.gameObject.SetActive(false);
                };
                adsButton.transform.DOScale(Vector3.zero, 0.2f).onComplete += () =>
                {
                    adsButton.gameObject.SetActive(false);
                };
                chestOpenEffect.SetActive(false);
                // coinBar.transform.DOScale(Vector3.zero, 0.2f).onComplete += () =>
                // {
                //     coinBar.gameObject.SetActive(false);
                // };
            };
        }
        else
        {
            title.gameObject.SetActive(false);
            claimButton.gameObject.SetActive(false);
            adsButton.gameObject.SetActive(false);
            chestOpenEffect.SetActive(false);
            // coinBar.gameObject.SetActive(false);

            onHideComplete?.Invoke();
        }
    }

    private void SetAdsButtonState(bool isQuick = true)
    {
        claimButton.transform.localScale = Vector3.zero;
        adsButton.transform.localScale = Vector3.zero;
        claimButton.gameObject.SetActive(true);
        if (GameManager.Ads.CheckRewardedAdIsLoaded() /*&& !isCoinSpin*/)
        {
            claimButton.transform.localPosition = new Vector3(-250, -540, 0);
            adsButton.gameObject.SetActive(true);
            adsButton.transform.localPosition = new Vector3(250, -540, 0);
        }
        else
        {
            claimButton.transform.localPosition = new Vector3(0, -540, 0);
            adsButton.gameObject.SetActive(false);
        }

        if (isQuick)
        {
            claimButton.transform.localScale = Vector3.one;
            adsButton.transform.localScale = Vector3.one;
        }
        else
        {
            claimButton.transform.DOScale(1.1f, 0.2f).onComplete = () => { claimButton.transform.DOScale(1f, 0.2f); };
            adsButton.transform.DOScale(1.1f, 0.2f).onComplete = () => { adsButton.transform.DOScale(1f, 0.2f); };
        }
    }

    public override void OnReset()
    {
        claimButton.OnReset();
        adsButton.OnReset();
        GameManager.Event.Unsubscribe(RewardAdLoadCompleteEventArgs.EventId, OnRewardAdLoadComplete);
        GameManager.Event.Unsubscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarnedReward);
        base.OnReset();
    }

    private UnityAction _onClickEvent;

    public override void SetOnClickEvent(UnityAction onClick)
    {
        _onClickEvent = onClick;
    }


    private void OnRewardAdEarnedReward(object sender, GameEventArgs e)
    {
        RewardAdEarnedRewardEventArgs ne = (RewardAdEarnedRewardEventArgs)e;
        if (ne.UserData.ToString() != "DoubleTurntableReward")
        {
            return;
        }

        bool isDouble = true;//ne.EarnedReward;
        if (isDouble)
        {
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Spin_Double);
            try
            {
                OnResume();
            }
            catch (Exception exception)
            {
                Log.Error("TurntableRewardPanel OnRewardAdEarned Error {0}", exception.Message);
            }

            var rewardDic = _rewardArea.RewardsDic;
            if (rewardDic == null)
            {
                _onClickEvent?.Invoke();
                return;
            }

            foreach (var reward in rewardDic)
            {
                GameManager.PlayerData.AddItemNum(reward.Key, reward.Value);
            }

            _rewardArea.DoubleNumberText(false, () => { _onClickEvent?.Invoke(); });
        }
        else
        {
            OnResume();
            SetAdsButtonState();
        }
    }

    private void OnRewardAdLoadComplete(object sender, GameEventArgs e)
    {
    }

    private void OnAdsButtonClick()
    {
        OnPause();
        if (!GameManager.Ads.ShowRewardedAd("DoubleTurntableReward"))
        {
            OnResume();
        }
    }

    private void OnClaimButtonClick()
    {
        _onClickEvent?.Invoke();
    }

    private void OnPause()
    {
        claimButton.interactable = false;
        adsButton.interactable = false;
        // coinBar.addButton.interactable = false;
    }

    private void OnResume()
    {
        claimButton.interactable = true;
        adsButton.interactable = true;
        // coinBar.addButton.interactable = true;

    }
}