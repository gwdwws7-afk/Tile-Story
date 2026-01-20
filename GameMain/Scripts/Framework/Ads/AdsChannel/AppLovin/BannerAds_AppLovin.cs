#if AmazonStore || UNITY_IOS || UNITY_IPHONE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BannerAds_AppLovin : AdUnitBase
{
#if UNITY_IOS || UNITY_IPHONE
        private readonly string AmazonAdsId = "c2af5e1e-a972-47bd-926e-10ba7ff1ac01";
#else
        private readonly string AmazonAdsId = "397f7b75-75e9-466d-a182-7875e0abe791";
#endif

    private bool IsFirstLoad = true;
    private bool isShow = true;

    public override bool IsLoaded()
    {
        return IsLoad;
    }

    public override bool Show(object userData)
    {
        if (IsLoaded())
        {
            MaxSdk.ShowBanner(CurAdsId);
            return true;
        }

        return false;
    }

    public override void Dispose()
    {
        if (IsLoaded())
        {
            IsLoad = false;
            MaxSdk.DestroyBanner(CurAdsId);
        }
        
        base.Dispose();
    }

    public override void LoadAd(string[] ids=null)
    {
        ids = ids == null ? base.AdsIds : ids;
        
        if (ids == null || ids.Length == 0)
        {
            Log.Debug("AppLovin BannerAd adUnitId is invalid");
            return;
        }

        if (IsLoaded())
        {
            Log.Debug($"AppLovin BannerAd is has load");
            return;
        }
        
        base.SetAdsIds(ids);
        base.LoadAd(ids);
        
        if (string.IsNullOrEmpty(CurAdsId))
        {
            Log.Debug($"AppLovin BannerAd Can Load Id is Null! Id :{base.CurAdsId}");
            return;
        }

        Log.Debug($"AppLovin BannerAd Start Load...:{base.CurAdsId}");
        GameManager.Ads.IsRequestingBannerAd = true;
        if (!InitAds_AppLovin.IsInit)
            InitAds_AppLovin.Init();
        SetAdsEvents();
        int width;
        int height;
        if (MaxSdkUtils.IsTablet())
        {
            width = 768;
            height = 90;
        }
        else
        {
            width = 320;
            height = 50;
        }

        if (IsFirstLoad)
        {
            IsFirstLoad = false;
            var apsBanner = new AmazonAds.APSBannerAdRequest(width, height, AmazonAdsId);
            apsBanner.onSuccess += (adResponse) =>
            {
                MaxSdk.SetBannerLocalExtraParameter(CurAdsId, "amazon_ad_response", adResponse.GetResponse());
                CreateMaxBannerAd();
            };
            apsBanner.onFailedWithError += (adError) =>
            {
                MaxSdk.SetBannerLocalExtraParameter(CurAdsId, "amazon_ad_error", adError.GetAdError());
                CreateMaxBannerAd();
            };

            apsBanner.LoadAd();
        }
        else
        {
            CreateMaxBannerAd();
        }
    }
    
    private void CreateMaxBannerAd()
    {
        MaxSdk.CreateBanner(CurAdsId, MaxSdkBase.BannerPosition.BottomCenter);
        MaxSdk.SetBannerPlacement(CurAdsId, "BannerPosition.BottomCenter");
        // MaxSdk.SetBannerBackgroundColor(CurAdsId, Color.black);
    }

    private void SetAdsEvents()
    {
        MaxSdkCallbacks.Banner.OnAdLoadedEvent-=HandleBannerAdLoaded;
        MaxSdkCallbacks.Banner.OnAdLoadedEvent+=HandleBannerAdLoaded;
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent-=HandleBannerAdFailedToLoad;
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent+=HandleBannerAdFailedToLoad;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent-=HandleBannerAdRevenuePaidEvent;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent+=HandleBannerAdRevenuePaidEvent;
    }

    private void HandleBannerAdRevenuePaidEvent(string adId, MaxSdkBase.AdInfo adInfo)
    {
        if(adInfo==null)
            return;
        if (AddLocalPriceLog(adInfo.RevenuePrecision, adInfo.Revenue, "USD"))
        {
            GameManager.Firebase.RecordOnPaidEvent("BannerAd", adInfo.Revenue, "USD", adInfo.RevenuePrecision, adId,
                adInfo.NetworkName);
            Log.Debug($"HandleBannerAdPaidEvent:{adInfo.NetworkName}");
        }
    }

    private void HandleBannerAdFailedToLoad(string adId, MaxSdkBase.ErrorInfo err)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            GameManager.Ads.IsRequestingBannerAd = false;
            Debug.Log($"Applovin BannerAd Failed To Load,Reason {base.CurAdsId}...Error:{err.Message}");

            if (!IsLoad && !string.IsNullOrEmpty(CurAdsId))
            {
                Dispose();
                AdsIdModel.LoadFail();
                eventQueue.Enqueue(AdsEvent.Create(() =>
                {
                    LoadAd();
                },AdsDelayLoadTime));
            }
        }));
    }

    private void HandleBannerAdLoaded(string adId, MaxSdkBase.AdInfo adInfo)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            AdsIdModel.LoadSuccess();
            base.IsLoad = true;
            GameManager.Ads.IsRequestingBannerAd = false;
            MaxSdk.ShowBanner(adId);
            Log.Debug("AppLovin BannerAd Loaded...");

            if (GameManager.Ads.IsBannerAdInHideStatus)
				GameManager.Ads.HideBanner();
        }));
    }

    public override int GetHeight()
    {
        return Mathf.RoundToInt(MaxSdkUtils.GetAdaptiveBannerHeight());
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

    public override void ShowBanner()
    {
        if(IsLoaded())
        {
            isShow = true;
            MaxSdk.ShowBanner(base.CurAdsId);
        }
    }

    public override void HideBanner()
    {
        if(IsLoaded())
        {
            isShow = false;
            MaxSdk.HideBanner(base.CurAdsId);
        }
    }
}
#endif
