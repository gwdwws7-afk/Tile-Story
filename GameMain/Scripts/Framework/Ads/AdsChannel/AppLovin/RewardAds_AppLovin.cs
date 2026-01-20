#if AmazonStore||UNITY_IOS||UNITY_IPHONE

using System.Collections;
using System.Collections.Generic;
using Firebase.Analytics;
using UnityEngine;

public class RewardAds_AppLovin : AdUnitBase
{
    private object userData;
    private bool isUserEarnedReward;
    private bool _isFirstLoad = true;

#if UNITY_IOS||UNITY_IPHONE
        private readonly string AmazonAdsId = "37fa7b25-ad9e-467a-bf07-00cae73a96d5";
#else
        private readonly string AmazonAdsId = "f44b462e-df90-444f-b741-a1d4069b35d7";
#endif

    public override bool IsLoaded()
    {
        if (Time.realtimeSinceStartup < loadedAdLifeTime) 
            return IsLoad&&MaxSdk.IsRewardedAdReady(CurAdsId);
        Dispose();
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            LoadAd(AdsIds);
        }, AdsDelayLoadTime));
        return false;
    }

    public override bool Show(object userData)
    {
        if(Time.realtimeSinceStartup > loadedAdLifeTime)
        {
            Debug.Log("AppLovin RewardedAd adUnitId is expired!");
            GameManager.Event.Fire(this, RewardAdEarnedRewardEventArgs.Create(true, userData));
            
            Dispose();
            eventQueue.Enqueue(AdsEvent.Create(() =>
            {
                LoadAd(AdsIds);
            }, AdsDelayLoadTime));
            return false;
        }

        if (IsLoaded())
        {
            this.userData=userData;
            GameManager.CurState="ShowRewardAds_AppLovin";
            ShowAdsCanvas(true,()=>MaxSdk.ShowRewardedAd(CurAdsId));

            return true;
        }

        return false;
    }

    public override void Dispose()
    {
        ShowAdsCanvas(false);
        IsLoad = false;
        userData = null;
        isUserEarnedReward = false;
        base.Dispose();
    }

    public override void LoadAd(string[] ids)
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
        if (base.CurAdsId == null)
        {
            Log.Debug($"Admob RewardedAd Can Load Id is Null! Id :{base.CurAdsId}");
            return;
        }

        Log.Debug($"Admob RewardedAd Start Load...{base.CurAdsId}");
        GameManager.Ads.IsRequestingRewardAd = true;
        if (!InitAds_AppLovin.IsInit)
            InitAds_AppLovin.Init();
        SetAdsEvents();

        if (_isFirstLoad)
        {
            _isFirstLoad = false;

            var rewardedVideoAd = new AmazonAds.APSVideoAdRequest(320, 480, AmazonAdsId);
            rewardedVideoAd.onSuccess += (adResponse) =>
            {
                MaxSdk.SetRewardedAdLocalExtraParameter(CurAdsId, "amazon_ad_response", adResponse.GetResponse());
                Debug.Log("onSuccess [AdManager] AppLovin SDK is Loading RV" + CurAdsId);
                MaxSdk.LoadRewardedAd(CurAdsId);
            };
            rewardedVideoAd.onFailedWithError += (adError) =>
            {
                MaxSdk.SetRewardedAdLocalExtraParameter(CurAdsId, "amazon_ad_error", adError.GetAdError());
                Debug.Log("onFailedWithError [AdManager] AppLovin SDK is Loading RV " + CurAdsId +adError.GetAdError());
                MaxSdk.LoadRewardedAd(CurAdsId);
            };

            rewardedVideoAd.LoadAd();
			
        }
        else
        {
            Debug.Log("[AdManager] AppLovin SDK is Loading RV" + CurAdsId);
            MaxSdk.LoadRewardedAd(CurAdsId);
        }
    }

    private void SetAdsEvents()
    {
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent -= OnAdLoadedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnAdLoadedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent -= OnAdLoadFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnAdLoadFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent -= OnAdDisplayFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnAdDisplayFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent -= OnAdRevenuePaidEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent -= OnAdHiddenEvent;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnAdHiddenEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent -= OnAdReceivedRewardEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnAdReceivedRewardEvent;
    }

    private void OnAdReceivedRewardEvent(string adId, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo adInfo)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            if(reward.Amount>0)
                isUserEarnedReward = true;
        }));
    }

    private void OnAdHiddenEvent(string adId, MaxSdkBase.AdInfo adInfo)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            GameManager.Event.Fire(this, RewardAdEarnedRewardEventArgs.Create(isUserEarnedReward, userData));
            Log.Debug("AppLovin RewardAd Closed...");
            Dispose();
            eventQueue.Enqueue(AdsEvent.Create(() =>
            {
                LoadAd(AdsIds);
            }, AdsDelayLoadTime));
        }));
    }

    private void OnAdRevenuePaidEvent(string adId, MaxSdkBase.AdInfo adInfo)
    {
        if(adInfo==null)
            return;
        if (AddLocalPriceLog(adInfo.RevenuePrecision, adInfo.Revenue, "USD"))
        {
            GameManager.Firebase.RecordOnPaidEvent("RewardedAd", adInfo.Revenue, "USD", adInfo.RevenuePrecision, adId,
                adInfo.NetworkName);
            Log.Debug($"HandleInterstitialPaidEvent:{adInfo.NetworkName}");
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Rewarded_Ads_Paid,new Parameter("Memory",UnityUtility.GetSystemMemory()),new Parameter("Value",adInfo.Revenue/1000000d));
        }
    }

    private void OnAdDisplayFailedEvent(string adId, MaxSdkBase.ErrorInfo err, MaxSdkBase.AdInfo adInfo)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            Log.Debug($"AppLovin RewardAd OnAdDisplayFailedEvent...{base.CurAdsId}...Error:{err.Message}");
            GameManager.Event.Fire(this, RewardAdEarnedRewardEventArgs.Create(true, userData));
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Reward_Ads_Complete);
            Dispose();
            eventQueue.Enqueue(AdsEvent.Create(() =>
            {
                LoadAd(AdsIds);
            }, AdsDelayLoadTime));
        }));
    }

    private void OnAdLoadFailedEvent(string adId, MaxSdkBase.ErrorInfo err)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            Log.Debug($"AppLovin RewardAd FailedToLoad...{base.CurAdsId}...Error:{err.Message}");
            GameManager.Ads.IsRequestingRewardAd = false;

            Dispose();
            AdsIdModel.LoadFail();
            eventQueue.Enqueue(AdsEvent.Create(() =>
            {
                LoadAd(AdsIds);
            }, AdsDelayLoadTime));
        }));
    }

    private void OnAdLoadedEvent(string adId, MaxSdkBase.AdInfo adInfo)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            base.IsLoad = true;
            GameManager.Ads.IsRequestingRewardAd = false;
            loadedAdLifeTime = Time.realtimeSinceStartup + 60 * 60 * 2;
            AdsIdModel.LoadSuccess();
            Log.Debug($"AppLovin RewardAd Loaded...:{base.CurAdsId}");
            GameManager.Event.Fire(this, RewardAdLoadCompleteEventArgs.Create());
        }, AdsDelayLoadTime));
    }
    
    private bool AddLocalPriceLog(string precision, double value, string currencyCode)
    {
        return true;
        if (string.IsNullOrEmpty(precision) || value <= 0 || string.IsNullOrEmpty(currencyCode))
            return false;

        return true;
        if (currencyCode.Equals("USD") && precision != "Unknown")
        {
            return true;
        }
        return false;
    }
}
#endif
