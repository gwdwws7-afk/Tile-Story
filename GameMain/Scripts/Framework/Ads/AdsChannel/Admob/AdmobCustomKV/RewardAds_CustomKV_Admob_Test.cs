using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Analytics;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

public sealed class RewardAds_CustomKV_Admob_Test : AdUnitBase
{
    private const int MaxOffsetIndex = 2;
    
    private RewardedAd rewardedAd;
    private object userData;
    private int offsetIndex = MaxOffsetIndex;
    private float recordStartLoadTime = 0f;

    private KVGroupInfo curKVGroup = null;
    protected override string CurAdsId
    {
        get
        {
            if (curKVGroup == null)
            {
                curKVGroup = CustomKVManager.GetCustomKVGroup(true, offsetIndex);
            }

            return curKVGroup != null ? curKVGroup.AdsId : null;
        }
    }
    
    public bool IsValid
    {
        get
        {
            return offsetIndex < MaxOffsetIndex;
        }
        set
        {
            if (value)
                offsetIndex = 0;
            else
                offsetIndex = MaxOffsetIndex;
        }
    }

    public override bool IsLoaded()
    {
        return rewardedAd != null && IsLoad && rewardedAd.CanShowAd();
    }

    public override void LoadAd(string[] ids = null)
    {
        if (!IsValid)
            return;
        
        if (curKVGroup != null || IsLoaded()) 
        {
            Debug.Log("[KV]RewardAds_CustomKV_Admob adUnitId already loaded");
            return;
        }
        
        if (this.CurAdsId == null)
        {
            IsValid = false;
            Debug.LogError($"[KV]RewardAds_CustomKV_Admob Can Load Id is Null! Ecpm :{CustomKVManager.RewardedAdsEcpm}");
            return;
        }
        
        Debug.Log($"[KV]RewardAds_CustomKV_Admob Start Load...");
        
        loadAdsTimeOut = 65;
        //loadAdsTimeOut = (float)GameManager.Firebase.GetDouble(Constant.RemoteConfig.Remote_KVGroup_TimeOut, 20d);//广告请求超时时间
        ClearEventHandlers(rewardedAd);
        
        if(rewardedAd!=null)rewardedAd.Destroy();

        if(recordStartLoadTime==0)recordStartLoadTime = Time.realtimeSinceStartup;
        
        var adRequest = new AdRequest();
        
        if (curKVGroup.Key != null && curKVGroup.Value != null)
        {
            adRequest.CustomTargeting.Add(curKVGroup.Key,curKVGroup.Value);
            Debug.Log($"[KV]RewardAds_CustomKV_Admob admob_custom_keyvals:{(curKVGroup!=null?curKVGroup.Value:"???")}|{CustomKVManager.RewardedAdsEcpm}|n+{offsetIndex}");
        }

        RewardedAd.Load(this.CurAdsId,adRequest,(ads,error)=>
        {
            AdmobUtils.ConcurrentQueue.Enqueue(() =>
            {
                if (error != null || ads == null)
                {
                    FailedToLoadAction(error);
                }
                else
                {
                    Debug.Log($"[KV]RewardAds_CustomKV_Admob Loaded...:{(curKVGroup!=null?curKVGroup.Value:"???")}|n+{offsetIndex}|loadTime{Time.realtimeSinceStartup-recordStartLoadTime}");
                    rewardedAd = ads;
                    HandleRewardedAdLoaded();
                    RegisterEventHandlers(rewardedAd);
                }
            });
        });
        // if(rewardedAd!=null)rewardedAd.Destroy();
        //
        // rewardedAd = new SelfRewarded(this.CurAdsId);
        //
        // if(recordStartLoadTime==0)recordStartLoadTime = Time.realtimeSinceStartup;
        //
        // RegisterEventHandlers(rewardedAd);
        //
        // if (curKVGroup.Key != null && curKVGroup.Value != null) 
        // {
        //     GameManager.Firebase.RecordMessageByEvent("KeyValueRVReq",
        //         new Parameter("AdsId",this.CurAdsId),
        //         new Parameter("Key",curKVGroup.Key),
        //         new Parameter("Value",curKVGroup.Value),
        //         new Parameter("Ecpm",CustomKVManager.RewardedAdsEcpm));
        //     
        //     Debug.Log($"[KV]RewardAds_CustomKV_Admob admob_custom_keyvals:{curKVGroup.Key}|{curKVGroup.Value}|{CustomKVManager.RewardedAdsEcpm}|n+{offsetIndex}");
        //     rewardedAd.LoadAd("admob_custom_keyvals", curKVGroup.Key, curKVGroup.Value);
        // }
        // else
        // {
        //     rewardedAd.LoadAd(null,null,null);
        // }
    }

    public override bool Show(object userData)
    {
        if (IsLoaded()&&rewardedAd.CanShowAd())
        {
            Debug.Log($"[KV]RewardAds_CustomKV_Admob show ads:{(curKVGroup!=null?curKVGroup.Value:"???")}|n+{offsetIndex}");
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
    
    private void HandleUserEarnedReward()
    {
        AdmobUtils.ConcurrentQueue.Enqueue(() => {  });
    }
    public override void Dispose()
    {
        Debug.Log($"[KV]RewardAds_CustomKV_Admob Dispose:{(curKVGroup!=null?curKVGroup.Value:"???")}|n+{offsetIndex}");
        
        ShowAdsCanvas(false);
        IsLoad = false;
        curKVGroup = null;
        offsetIndex++;

        if (rewardedAd != null)
        {
            ClearEventHandlers(rewardedAd);
            rewardedAd.Destroy();
            rewardedAd = null;
        }
        base.Dispose();
    }
    
    private void HandleRewardedAdLoaded()
    {
        AdmobUtils.ConcurrentQueue.Enqueue(() =>
        {
            //Debug.Log($"[KV]RewardAds_CustomKV_Admob Loaded...:{(curKVGroup!=null?curKVGroup.Value:"???")}|n+{offsetIndex}|loadTime{Time.realtimeSinceStartup-recordStartLoadTime}");
            recordStartLoadTime = 0;
            
            base.IsLoad = true;
            if (AdsIdModel != null) AdsIdModel.LoadSuccess();
            
            GameManager.Event.Fire(this, RewardAdLoadCompleteEventArgs.Create());
        });
    }

    private void RegisterEventHandlers(RewardedAd ad)
    {
        ad.OnAdPaid -= HandlePaidEvent;
        ad.OnAdPaid += HandlePaidEvent;
        
        ad.OnAdFullScreenContentClosed -= ClosedAction;
        ad.OnAdFullScreenContentClosed += ClosedAction;
        
        ad.OnAdFullScreenContentFailed -= FailedToShow;
        ad.OnAdFullScreenContentFailed += FailedToShow;
    }

    private void ClearEventHandlers(RewardedAd ad)
    {
        if(ad==null)return;
        
        ad.OnAdPaid -= HandlePaidEvent;
        ad.OnAdFullScreenContentClosed -= ClosedAction;
        ad.OnAdFullScreenContentFailed -= FailedToShow;
    }
    
    //obsolete
    void LoadedAction(object a, EventArgs b)
    {
        HandleAdLoaded();
    }

    void FailedToLoadAction(LoadAdError b)
    {
        HandleAdFailedToLoad(null);
    }

    void FailedToShow(AdError adError)
    {
        HandleAdClosed();
    }
    
    void ClosedAction()
    {
        HandleAdClosed();
    }
    
    private void HandleAdLoaded()
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            Debug.Log($"[KV]RewardAds_CustomKV_Admob Loaded...:{(curKVGroup!=null?curKVGroup.Value:"???")}|n+{offsetIndex}|loadTime{Time.realtimeSinceStartup-recordStartLoadTime}");
            recordStartLoadTime = 0;
            
            base.IsLoad = true;
            if (AdsIdModel != null) AdsIdModel.LoadSuccess();
            
            GameManager.Event.Fire(this, RewardAdLoadCompleteEventArgs.Create());
        }));
    }

    private void HandleAdFailedToLoad(ILoadAdErrorClient e)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            Debug.Log($"[KV]RewardAds_CustomKV_Admob FailedToLoad...{(curKVGroup!=null?curKVGroup.Value:"???")}|n+{offsetIndex}");
            recordStartLoadTime = 0;
            Dispose();
            if (AdsIdModel != null) AdsIdModel.LoadFail();

            eventQueue.Enqueue(AdsEvent.Create(() =>
            {
                LoadAd();
            }, AdsDelayLoadTime));
        }));
    }

    private void HandleAdClosed()
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            GameManager.Task.AddDelayTriggerTask(0.5f,()=>
            {
                GameManager.Event.Fire(this, RewardAdEarnedRewardEventArgs.Create(true, userData));
            });
            
            Debug.Log($"[KV]RewardAds_CustomKV_Admob Closed...{(curKVGroup!=null?curKVGroup.Value:"???")}|n+{offsetIndex}");

            IsValid = false;//直接结束这次请求
            Dispose();
            
            GameManager.Event.Fire(null,AdsEcpmRefreshEventArgs.Create(true));  
            
            //eventQueue.Enqueue(AdsEvent.Create(() => { LoadAd(); }, AdsDelayLoadTime));
        }));
    }
    
    private void HandlePaidEvent(AdValue e)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            if (e == null) return;
            Debug.Log($"[KV]RewardAds_CustomKV_Admob HandlePaidEvent...{(curKVGroup!=null?curKVGroup.Value:"???")}|n+{offsetIndex}|Ecpm:"+(e.Value/1000f));

            try
            {
                if (AddLocalPriceLog(e.Precision.ToString(), e.Value, e.CurrencyCode))
                {
                    if (rewardedAd != null)
                    {
                        ResponseInfo info = rewardedAd.GetResponseInfo();
                        string responseId = info.GetResponseId();
                        string mediationAdapterClassName = info.GetMediationAdapterClassName();
                        GameManager.Firebase.RecordOnPaidEvent("RewardedAd", e.Value, e.CurrencyCode,
                            e.Precision.ToString(), responseId, mediationAdapterClassName);
                        Log.Debug($"HandleRewardedAdPaidEvent:{mediationAdapterClassName}");
                    }

                    GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Rewarded_Ads_Paid,
                        new Parameter("Memory", UnityUtility.GetSystemMemory()),
                        new Parameter("Value", e.Value / 1000000d));
                    
                    if (curKVGroup != null)
                    {
                        GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Rewarded_KV_Ads_Paid,
                            new Parameter("Index", curKVGroup.Index),
                            new Parameter("Value", e.Value / 1000000d));   
                    }
                }
            }
            catch
            {
            }
        }));
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
