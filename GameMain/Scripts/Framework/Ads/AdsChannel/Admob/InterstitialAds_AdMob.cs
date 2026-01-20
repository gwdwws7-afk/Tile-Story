using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Analytics;
using GoogleMobileAds.Common;
using UnityEngine;

/// <summary>
/// Admob插屏广告单元
/// </summary>
public sealed class InterstitialAds_AdMob : AdUnitBase
{
    private InterstitialAd interstitialAd;

    private float recordStartLoadTime = 0f;
    
    public override bool IsLoaded()
    {
        if (loadedAdLifeTime==0||Time.realtimeSinceStartup < loadedAdLifeTime) return interstitialAd != null && IsLoad;
        Dispose();
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            LoadAd(AdsIds);
        }, AdsDelayLoadTime));
        return false;
    }

    public override void LoadAd(string[] ids=null)
    {
        ids = ids == null ? base.AdsIds : ids;
        if (ids == null || ids.Length == 0)
        {
            Log.Debug("Admob InterstitialAd adUnitId is invalid!");
            return;
        }

        if (IsLoaded())
        {
            Log.Debug("Admob InterstitialAd adUnitId already load:" + ids);
            return;
        }

        base.SetAdsIds(ids);
        base.LoadAd(ids);

        if (this.CurAdsId == null)
        {
            Log.Debug($"Admob InterstitialAd Can Load Id is Null! Id :{this.CurAdsId}");
            return;
        }

        Log.Debug($"Admob InterstitialAd Start Load...{this.CurAdsId}");
        GameManager.Ads.IsRequestingInterstitialAd = true;

        if (interstitialAd!=null)interstitialAd.Destroy();
        
        var adRequest = new AdRequest();

        if(recordStartLoadTime==0)recordStartLoadTime = Time.realtimeSinceStartup;

        InterstitialAd.Load(this.CurAdsId, adRequest,(ads,error)=>
        {
            AdmobUtils.ConcurrentQueue.Enqueue(() =>
            {
                if (ads == null || error != null)
                {
                    HandleInterstitialAdFailedToLoad(error);
                }
                else
                {
                    interstitialAd = ads;
                    base.IsLoad = true;
                    HandleInterstitialAdLoaded();
                    RegisterEventHandlers(interstitialAd);
                }
            });
        });
    }

    public override bool Show(object userData)
    {
        if(Time.realtimeSinceStartup > loadedAdLifeTime)
        {
            Debug.Log("Admob RewardedAd adUnitId is expired!");
            Dispose();
            eventQueue.Enqueue(AdsEvent.Create(() =>
            {
                LoadAd(AdsIds);
            }, AdsDelayLoadTime));
            return false;
        }
        
        if (IsLoaded()&&interstitialAd.CanShowAd())
        {
            GameManager.CurState = "ShowInterstitialAds_Admob";
            ShowAdsCanvas(true,interstitialAd.Show);
            return true;
        }
        return false;
    }

    public override void Dispose()
    {
        ShowAdsCanvas(false);
        IsLoad = false;
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
        }

        base.Dispose();
    }

    private void RegisterEventHandlers(InterstitialAd ad)
    {
        ad.OnAdPaid -= HandleInterstitialPaidEvent;
        ad.OnAdPaid += HandleInterstitialPaidEvent;
        
        ad.OnAdFullScreenContentClosed -= HandleInterstitialAdClosed;
        ad.OnAdFullScreenContentClosed += HandleInterstitialAdClosed;
        
        ad.OnAdFullScreenContentFailed -= HandleInterstitialAdFailedToShow;
        ad.OnAdFullScreenContentFailed += HandleInterstitialAdFailedToShow;
    }

    private void HandleInterstitialAdLoaded()
    {
        AdmobUtils.ConcurrentQueue.Enqueue(() =>
        {
            //打点记录加载成功时长 和次数
            GameManager.Firebase.RecordMessageByEvent("InterstitialLoadSuccess",
                new Parameter("Time",Time.realtimeSinceStartup-recordStartLoadTime),
                new Parameter("FailCount",AdsIdModel.GetFailCount()));
            recordStartLoadTime = 0;
            
            base.IsLoad = true;
            GameManager.Ads.IsRequestingInterstitialAd = false;
            loadedAdLifeTime = Time.realtimeSinceStartup + 60 * 60*2;
            AdsIdModel.LoadSuccess();
            GameManager.Event.Fire(null, InterstitialAdLoadCompleteEventArgs.Create());
            
            Log.Info($"Admob InterstitialAd Loaded...:{this.CurAdsId}");
        });
    }

    private void HandleInterstitialAdFailedToLoad(LoadAdError e)
    {
        AdmobUtils.ConcurrentQueue.Enqueue(() =>
        {
            GameManager.Ads.IsRequestingInterstitialAd = false;
            Log.Debug($"Admob InterstitialAd FailedToLoad...{this.CurAdsId}...Error:{(e!=null?e.GetMessage():null)}");
            
            Dispose();
            AdsIdModel.LoadFail();

            eventQueue.Enqueue(AdsEvent.Create(() =>
            {
                LoadAd();
            }, AdsDelayLoadTime));
        });
    }

    private void HandleInterstitialAdFailedToShow(AdError e)
    {
        AdmobUtils.ConcurrentQueue.Enqueue(() =>
        {
            Log.Error("Admob InterstitialAd Failed To Show..." + e.GetMessage());

            Dispose();

            eventQueue.Enqueue(AdsEvent.Create(() =>
            {
                LoadAd();
            }, AdsDelayLoadTime));
        });
    }

    private void HandleInterstitialAdClosed()
    {
        AdmobUtils.ConcurrentQueue.Enqueue(() =>
        {
            Log.Info("Admob InterstitialAd Closed...");

            Dispose();

            GameManager.Event.Fire(null,AdsEcpmRefreshEventArgs.Create(false));  
            
            eventQueue.Enqueue(AdsEvent.Create(() =>
            {
                LoadAd();
            }, AdsDelayLoadTime));
        });
    }

    private void HandleInterstitialPaidEvent( AdValue e)
    {
        AdmobUtils.ConcurrentQueue.Enqueue(() =>
        {
            if (e == null) return;

            try
            {
                if (AddLocalPriceLog(e.Precision.ToString(), e.Value, e.CurrencyCode))
                {
                    if (interstitialAd != null)
                    {
                        ResponseInfo info = interstitialAd.GetResponseInfo();
                        GameManager.Firebase.RecordOnPaidEvent("InterstitialAd", e.Value, e.CurrencyCode, e.Precision.ToString(), info.GetResponseId(), info.GetMediationAdapterClassName());
                        Log.Debug($"HandleInterstitialPaidEvent:{info.GetMediationAdapterClassName()}");
                    }
                    Log.Debug($"HandleInterstitialPaidEvent");
                    GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Interstitial_Ads_Paid,new Parameter("Memory",UnityUtility.GetSystemMemory()),new Parameter("Value",e.Value/1000000d));
                } else
                {
                    Log.Debug("HandlePaidEvent:RecordOnPaidEvent :Fail!");
                }
            }
            catch(Exception except)
            {
                Log.Error($"HandlePaidEvent:{except.Message}");
            }
        });
    }

    private bool AddLocalPriceLog(string precision, long value, string currencyCode)
    {
        adsPrice = value;

        FirebaseAdsJsonUtil.RecordAdsPrice(value, false, false);
        
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
