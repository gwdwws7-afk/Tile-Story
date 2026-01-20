#if AmazonStore||UNITY_IOS||UNITY_IPHONE

using System.Collections;
using System.Collections.Generic;
using Firebase.Analytics;
using UnityEngine;

public class InterstitialAds_AppLovin : AdUnitBase
{
#if UNITY_IOS||UNITY_IPHONE
        private readonly string AmazonAdsId = "49dc799a-705f-4584-bf84-2af760738d2c";
#else
         private readonly string AmazonAdsId = "c7ad3bf9-0dc9-4b6b-8bcd-ffc13d5a8f9e";
#endif
    public override bool IsLoaded()
    {
        if (loadedAdLifeTime==0||Time.realtimeSinceStartup < loadedAdLifeTime)
            return IsLoad&&MaxSdk.IsInterstitialReady(CurAdsId);
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
            Dispose();
            eventQueue.Enqueue(AdsEvent.Create(() =>
            {
                LoadAd(AdsIds);
            }, AdsDelayLoadTime));
            return false;
        }

        if (IsLoaded())
        {
            GameManager.CurState="ShowInterstitialAds_AppLovin";
            ShowAdsCanvas(true,()=>MaxSdk.ShowInterstitial(CurAdsId));

            return true;
        }

        return false;
    }

    public override void LoadAd(string[] ids)
    {
        ids = ids == null ? base.AdsIds : ids;
        if (ids == null || ids.Length == 0)
        {
            Log.Debug("AppLovin InterstitialAd adUnitId is invalid!");
            return;
        }

        if (IsLoaded())
        {
            Log.Debug("AppLovin InterstitialAd adUnitId already load:" + ids);
            return;
        }
        base.SetAdsIds(ids);
        base.LoadAd(ids);
        
        if (base.CurAdsId == null)
        {
            Log.Debug($"AppLovin InterstitialAd Can Load Id is Null! Id :{base.CurAdsId}");
            return;
        }

        Log.Debug($"AppLovin InterstitialAd Start Load...{base.CurAdsId}");
        GameManager.Ads.IsRequestingInterstitialAd = true;
        if (!InitAds_AppLovin.IsInit)
            InitAds_AppLovin.Init();
        SetAdsEvents();

        if (IsFirstLoad)
        {
            IsFirstLoad = false;

            var interstitialVideoAd = new AmazonAds.APSVideoAdRequest(320, 480, AmazonAdsId);
            interstitialVideoAd.onSuccess += (adResponse) =>
            {
                MaxSdk.SetInterstitialLocalExtraParameter(CurAdsId, "amazon_ad_response", adResponse.GetResponse());
                MaxSdk.LoadInterstitial(CurAdsId);
            };
            interstitialVideoAd.onFailedWithError += (adError) =>
            {
                MaxSdk.SetInterstitialLocalExtraParameter(CurAdsId, "amazon_ad_error", adError.GetAdError());
                MaxSdk.LoadInterstitial(CurAdsId);
            };

            interstitialVideoAd.LoadAd();
        }
        else
        {
            MaxSdk.LoadInterstitial(CurAdsId);
        }
    }

    private bool IsFirstLoad = true;

    public override void Dispose()
    {
        ShowAdsCanvas(false);
        IsLoad = false;
        base.Dispose();
    }

    private void SetAdsEvents()
    {
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent-= OnAdLoadedEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent+= OnAdLoadedEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent-= OnAdLoadFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent+= OnAdLoadFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent-= OnAdDisplayFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent+= OnAdDisplayFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent-= OnAdHiddenEvent;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent+= OnAdHiddenEvent;
        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent-= OnAdRevenuePaidEvent;
        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent+= OnAdRevenuePaidEvent;
    }

    private void OnAdRevenuePaidEvent(string adId, MaxSdkBase.AdInfo adInfo)
    {
        if(adInfo==null)
            return;
        if (AddLocalPriceLog(adInfo.RevenuePrecision, adInfo.Revenue, "USD"))
        {
            GameManager.Firebase.RecordOnPaidEvent("InterstitialAd", adInfo.Revenue, "USD", adInfo.RevenuePrecision, adId,
                adInfo.NetworkName);
            Log.Debug($"HandleInterstitialPaidEvent:{adInfo.NetworkName}");
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Interstitial_Ads_Paid,new Parameter("Memory",UnityUtility.GetSystemMemory()),new Parameter("Value",adInfo.Revenue/1000000d));
        }
    }

    private void OnAdHiddenEvent(string adId, MaxSdkBase.AdInfo adInfo)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            Log.Debug("AppLovin InterstitialAd Closed...");
            Dispose();
            eventQueue.Enqueue(AdsEvent.Create(() =>
            {
                LoadAd(AdsIds);
            }, AdsDelayLoadTime));
        }));
    }

    private void OnAdDisplayFailedEvent(string adId, MaxSdkBase.ErrorInfo err, MaxSdkBase.AdInfo adInfo)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            Log.Debug($"AppLovin InterstitialAd OnAdDisplayFailedEvent...{base.CurAdsId}...Error:{err.Message}");
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
            Log.Debug($"AppLovin InterstitialAd FailedToLoad...{base.CurAdsId}...Error:{err.Message}");
            GameManager.Ads.IsRequestingInterstitialAd = false;

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
            GameManager.Ads.IsRequestingInterstitialAd = false;
            loadedAdLifeTime = Time.realtimeSinceStartup + 60 * 60 * 2;
            AdsIdModel.LoadSuccess();
            Log.Debug($"AppLovin InterstitialAd Loaded...:{base.CurAdsId}");
        }));
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
