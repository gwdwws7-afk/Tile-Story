using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Event;
using Spine.Unity;
using UnityEngine;

public class FrogJumpEntranceBtn : EntranceUIForm
{
    [SerializeField] private GameObject redPoint;
    [SerializeField] private CountdownTimer timer;
    [SerializeField] private GameObject loadingBanner, timerBanner;
    private bool _adCoolDown;

    private void OnEnable()
    {
        GameManager.Event.Subscribe(RewardAdLoadCompleteEventArgs.EventId, RewardAdLoad);
        GameManager.Event.Subscribe(RewardAdEarnedRewardEventArgs.EventId, RewardAdEarnedReward);
        // GameManager.Event.Subscribe(CommonEventArgs.EventId, OnCommonEvent);
        timer.OnReset();
        timer.StartCountdown(DateTime.Today.AddDays(1));
        timer.CountdownOver += OnCountdownOver;
        if (DateTime.Now > GameManager.PlayerData.NextFrogAdReadyTime)
        {
            redPoint.SetActive(GameManager.Ads.CheckRewardedAdIsLoaded());
            _adCoolDown = false;
        }
        else
        {
            redPoint.SetActive(false);
            _adCoolDown = true;
        }

        RefreshBanner();
    }

    private void RewardAdEarnedReward(object sender, GameEventArgs e)
    {
        if (e is not RewardAdEarnedRewardEventArgs ne) return;
        
        if (ne.UserData.ToString() != "FrogJump")
        {
            redPoint.SetActive(GameManager.Ads.CheckRewardedAdIsLoaded());
            return;
        }
        _adCoolDown = true;
        redPoint.SetActive(false);
    }


    private void OnDisable()
    {
        _adCoolDown = false;
        GameManager.Event.Unsubscribe(RewardAdLoadCompleteEventArgs.EventId, RewardAdLoad);
        GameManager.Event.Unsubscribe(RewardAdEarnedRewardEventArgs.EventId, RewardAdEarnedReward);
        // GameManager.Event.Unsubscribe(CommonEventArgs.EventId, OnCommonEvent);
    }

    public override void OnButtonClick()
    {
        GameManager.UI.ShowUIForm("FrogJumpMenu");
    }

    private void RewardAdLoad(object sender, GameEventArgs e)
    {
        if (DateTime.Now > GameManager.PlayerData.NextFrogAdReadyTime)
        {
            redPoint.SetActive(true);
        }
        else
        {
            redPoint.SetActive(false);
        }

        RefreshBanner();
    }

    private void OnCommonEvent(object sender, GameEventArgs e)
    {
        if (e is CommonEventArgs ne)
        {
            try
            {
                if (ne.Type == CommonEventType.FrogAdStateChange)
                {
                    var flag = DateTime.Now < GameManager.PlayerData.NextFrogAdReadyTime;
                    _adCoolDown = flag;
                    redPoint.SetActive(!flag);
                }
            }
            catch (Exception e1)
            {
                Log.Info($"FrogAdStateChange:{e1.Message}");
            }
        }
    }

    private void OnCountdownOver(object sender, CountdownOverEventArgs e)
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_adCoolDown && Time.frameCount % 20 == 0)
        {
            if (DateTime.Now > GameManager.PlayerData.NextFrogAdReadyTime)
            {
                _adCoolDown = false;
                redPoint.SetActive(GameManager.Ads.CheckRewardedAdIsLoaded());

                RefreshBanner();
            }
        }

        timer.OnUpdate(Time.deltaTime, Time.fixedDeltaTime);
    }

    private void RefreshBanner()
    {
        if (DateTime.Now > GameManager.PlayerData.NextFrogAdReadyTime && GameManager.Ads.CheckRewardedAdIsLoaded()) 
        {
            loadingBanner.SetActive(false);
            timerBanner.SetActive(true);
        }
        else
        {
            loadingBanner.SetActive(true);
            timerBanner.SetActive(false);
        }
    }
}