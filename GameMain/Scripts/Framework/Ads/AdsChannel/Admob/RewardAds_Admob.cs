using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Analytics;
using GoogleMobileAds.Api.Mediation;
using GoogleMobileAds.Common;
using UnityEngine;

/// <summary>
/// Admob������浥Ԫ
/// </summary>
public sealed class RewardAds_Admob : AdUnitBase
{
    private RewardedAd rewardedAd;

    private object userData;
    private bool isUserEarnedReward;

    public override void LoadAd(string[] ids = null)
    {
        ids = ids == null ? base.AdsIds : ids;
        if (ids == null || ids.Length == 0)
        {
            Log.Debug("Admob RewardedAd adUnitId is invalid");
            return;
        }

        if (IsLoaded())
        {
            Log.Debug("Admob RewardedAd adUnitId already load:" + ids);
            return;
        }

        base.SetAdsIds(ids);
        base.LoadAd(ids);

        if (this.CurAdsId == null)
        {
            Log.Debug($"Admob RewardedAd Can Load Id is Null! Id :{this.CurAdsId}");
            return;
        }

        Log.Debug($"Admob RewardedAd Start Load...{this.CurAdsId}");
        GameManager.Ads.IsRequestingRewardAd = true;

        if(rewardedAd!=null)rewardedAd.Destroy();

        var adRequest = new AdRequest();

        RewardedAd.Load(this.CurAdsId,adRequest,(ads,error)=>
        {
            AdmobUtils.ConcurrentQueue.Enqueue(() =>
            {
                if (error != null || ads == null)
                {
                    HandleRewardedAdFailedToLoad(error);
                }
                else
                {
                    rewardedAd = ads;
                    HandleRewardedAdLoaded();
                    RegisterEventHandlers(rewardedAd);
                }
            });
        });
    }

    public override bool IsLoaded()
    {
        if (Time.realtimeSinceStartup < loadedAdLifeTime) return rewardedAd != null && IsLoad;
        Dispose();
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            LoadAd(AdsIds);
        }, AdsDelayLoadTime));
        return false;
    }

    public override bool Show(object userData)
    {
        if (Time.realtimeSinceStartup > loadedAdLifeTime)
        {
            Debug.Log("Admob RewardedAd adUnitId is expired!");
            object tempUserData = userData;
            GameManager.Task.AddDelayTriggerTask(0.4f, () =>
            {
                if (tempUserData != null)
                    GameManager.Event.Fire(this, RewardAdEarnedRewardEventArgs.Create(true, tempUserData));
            });
            
            Dispose();
            eventQueue.Enqueue(AdsEvent.Create(() =>
            {
                LoadAd(AdsIds);
            }, AdsDelayLoadTime));
            return false;
        }
        
        if (IsLoaded()&&rewardedAd.CanShowAd())
        {
            this.userData=userData;
            GameManager.CurState = "ShowRewardAds_Admob";
            ShowAdsCanvas(true,()=> 
            {
                rewardedAd.Show((reward)=> 
                {
                    if(reward!=null&&reward.Amount>0)
                        HandleUserEarnedReward();
                });
            });
            return true;
        }
        return false;
    }

    private void RegisterEventHandlers(RewardedAd ad)
    {
        ad.OnAdPaid -= HandleRewardedAdPaidEvent;
        ad.OnAdPaid += HandleRewardedAdPaidEvent;
        
        ad.OnAdFullScreenContentClosed -= HandleRewardedAdClosed;
        ad.OnAdFullScreenContentClosed += HandleRewardedAdClosed;
        
        ad.OnAdFullScreenContentFailed -= HandleRewardedAdFailedToShow;
        ad.OnAdFullScreenContentFailed += HandleRewardedAdFailedToShow;
    }

    public override void Dispose()
    {
        ShowAdsCanvas(false);
        IsLoad = false;
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
        }
        isUserEarnedReward = false;
        base.Dispose();
    }

    private void HandleRewardedAdLoaded()
    {
        AdmobUtils.ConcurrentQueue.Enqueue(() =>
        {
            Log.Info($"RewardedAd Loaded...:{this.CurAdsId}");
        
            base.IsLoad = true;
            GameManager.Ads.IsRequestingRewardAd = false;
            loadedAdLifeTime = Time.realtimeSinceStartup + 60 * 60*2;
            AdsIdModel.LoadSuccess();
            GameManager.Event.Fire(this, RewardAdLoadCompleteEventArgs.Create());
        });
    }

    private void HandleRewardedAdFailedToLoad(LoadAdError e)
    {
        AdmobUtils.ConcurrentQueue.Enqueue(() =>
        {
            GameManager.Ads.IsRequestingRewardAd = false;

            Log.Debug($"RewardedAd Failed To Load,Reason {this.CurAdsId}...Error:{(e!=null?e.GetMessage():null)}");

            Dispose();

            AdsIdModel.LoadFail();
            eventQueue.Enqueue(AdsEvent.Create(() =>
            {
                LoadAd();
            }, AdsDelayLoadTime));
        });
    }

    private void HandleRewardedAdFailedToShow(AdError e)
    {
        AdmobUtils.ConcurrentQueue.Enqueue(() =>
        {
            Log.Error($"{this.CurAdsId} RewardedAd Failed To Show,Reason ..." + e.GetMessage());

            if (userData != null)
                GameManager.Event.Fire(this, RewardAdEarnedRewardEventArgs.Create(true, userData));
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Reward_Ads_Complete);

            Dispose();

            eventQueue.Enqueue(AdsEvent.Create(() =>
            {
                LoadAd();
            }, AdsDelayLoadTime));
        });
    }

    private void HandleUserEarnedReward()
    {
        AdmobUtils.ConcurrentQueue.Enqueue(() => { isUserEarnedReward = true; });
    }

    private void HandleRewardedAdClosed()
    {
        AdmobUtils.ConcurrentQueue.Enqueue(() =>
        {
            bool isEarnedReward = isUserEarnedReward;
            object tempUserData = userData;
            GameManager.Task.AddDelayTriggerTask(0.4f, () =>
            {
                if (tempUserData != null) 
                    GameManager.Event.Fire(this, RewardAdEarnedRewardEventArgs.Create(true, tempUserData));
            });

            if (isUserEarnedReward)
            {
                isUserEarnedReward = false;
                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Reward_Ads_Complete);
            }
            else
            {
                Log.Warning("User not earned reward!");
            }

            Dispose();

            GameManager.Event.Fire(null,AdsEcpmRefreshEventArgs.Create(true));  
            
            eventQueue.Enqueue(AdsEvent.Create(() =>
            {
                LoadAd();
            }, AdsDelayLoadTime));
        });
    }

    private void HandleRewardedAdPaidEvent(AdValue e)
    {
        AdmobUtils.ConcurrentQueue.Enqueue(() =>
        {
            if (e == null) return;
            try 
            {
                if (AddLocalPriceLog(e.Precision.ToString(), e.Value, e.CurrencyCode))
                {
                    if (rewardedAd != null)
                    {
                        ResponseInfo info = rewardedAd.GetResponseInfo();
                        GameManager.Firebase.RecordOnPaidEvent("RewardedAd", e.Value, e.CurrencyCode, e.Precision.ToString(), info.GetResponseId(), info.GetMediationAdapterClassName());
                        Log.Debug($"HandleRewardedAdPaidEvent:{info.GetMediationAdapterClassName()}");
                    }
                    Log.Debug($"HandleRewardedAdPaidEvent111");
                    GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Rewarded_Ads_Paid,new Parameter("Memory",UnityUtility.GetSystemMemory()),new Parameter("Value",e.Value/1000000d));
                } else
                {
                    Log.Debug("HandlePaidEvent:RecordOnPaidEvent :Fail!");
                }
            }catch(Exception except)
            {
                Log.Error($"HandlePaidEvent:{except.Message}");
            }
        });
    }

    private bool AddLocalPriceLog(string precision, long value, string currencyCode)
    {
        adsPrice = value;

        FirebaseAdsJsonUtil.RecordAdsPrice(value, true, false);
        
        if (string.IsNullOrEmpty(precision) || value <= 0 || string.IsNullOrEmpty(currencyCode))
            return false;

        AdsTaichiRecordManager.OnAdRevenuePaidEvent(value,currencyCode);

        if (currencyCode.Equals("USD") && precision != "Unknown")
        {
            return true;
        }
        return false;
    }
}
