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
public sealed class Interstitial_NativeAds_AdMob : AdUnitBase
{
    private NativeAd nativeAd;
    
    private bool isUseFirebaseConfig = false;
    
    private float recordStartLoadTime = 0f;

    public override bool IsLoaded()
    {
        return nativeAd != null && IsLoad;
    }

    public override void LoadAd(string[] ids=null)
    {
        ids = ids == null ? base.AdsIds : ids;
        if (ids == null || ids.Length == 0)
        {
            Log.Debug("Admob Native InterstitialAd adUnitId is invalid!");
            return;
        }

        if (IsLoaded())
        {
            Log.Debug("Admob Native InterstitialAd adUnitId already load:" + ids);
            return;
        }

        base.SetAdsIds(ids);
        base.LoadAd(ids);

        if (this.CurAdsId == null)
        {
            Log.Debug($"Admob Native InterstitialAd Can Load Id is Null! Id :{this.CurAdsId}");
            return;
        }

        Log.Debug($"Admob Native InterstitialAd Start Load...{this.CurAdsId}");

        if(nativeAd!=null)nativeAd.Destroy();

        nativeAd = new NativeAd(this.CurAdsId);

        RegisterEventHandlers(nativeAd);
        
        if(recordStartLoadTime==0)recordStartLoadTime = Time.realtimeSinceStartup;

        nativeAd.LoadAd();
    }

    public override bool Show(object userData)
    {
        if (IsLoaded())
        {
            GameManager.CurState = "ShowSelfNativeInterstitialAds_Admob";
            ShowAdsCanvas(true,nativeAd.Show);
            return true;
        }
        return false;
    }

    public override void Dispose()
    {
        ShowAdsCanvas(false);
        IsLoad = false;
        if (nativeAd != null)
        {
            nativeAd.Destroy();
            nativeAd = null;
        }

        base.Dispose();
    }

    private void RegisterEventHandlers(NativeAd ad)
    {
        ad.OnAdLoaded -= LoadedAction;
        ad.OnAdLoaded += LoadedAction;

        ad.OnAdFailedToLoad -= FailedToLoadAction;
        ad.OnAdFailedToLoad += FailedToLoadAction;
        
        ad.OnAdClosed -= ClosedAction;
        ad.OnAdClosed += ClosedAction;

        ad.OnAdFailedToShow -= FailedToShow;
        ad.OnAdFailedToShow += FailedToShow;

        ad.OnPaidEvent -= HandlePaidEvent;
        ad.OnPaidEvent += HandlePaidEvent;
    }
    
    void LoadedAction(object a, EventArgs b)
    {
        HandleInterstitialAdLoaded();
    }

    void FailedToLoadAction(object a, LoadAdErrorClientEventArgs b)
    {
        HandleAdFailedToLoad(b.LoadAdErrorClient);
    }

    void ClosedAction(object a, EventArgs b)
    {
        HandleAdClosed();
    }

    void FailedToShow(object a, AdErrorClientEventArgs b)
    {
        HandleAdClosed();
    }

    private void HandleInterstitialAdLoaded()
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            //打点记录加载成功时长 和次数
            GameManager.Firebase.RecordMessageByEvent("NativeInterstitialLoadSuccess",
                new Parameter("Time",Time.realtimeSinceStartup-recordStartLoadTime),
                new Parameter("FailCount",AdsIdModel.GetFailCount()));
            recordStartLoadTime = 0;
            
            base.IsLoad = true;
            loadedAdLifeTime = Time.realtimeSinceStartup + 60 * 60 * 2;
            AdsIdModel.LoadSuccess();
            Log.Info($"Admob Native InterstitialAd Loaded...:{this.CurAdsId}");
        }));
    }

    private void HandleAdFailedToLoad(ILoadAdErrorClient e)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            FirebaseAdsJsonUtil.FailRecordAdsPrice(false);

            try
            {
                Log.Debug($"Admob Native InterstitialAd FailedToLoad...{this.CurAdsId}...Error");
            }
            catch (Exception exception)
            {
                Log.Debug($"Admob Native InterstitialAd FailedToLoad...{this.CurAdsId}");
            }
            Dispose();
            AdsIdModel.LoadFail();

            eventQueue.Enqueue(AdsEvent.Create(() => { LoadAd(); }, AdsDelayLoadTime));
        }));
    }

    private void HandleAdClosed()
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            Log.Info("Admob Native InterstitialAd Closed...");

            Dispose();

            eventQueue.Enqueue(AdsEvent.Create(() => { LoadAd(); }, AdsDelayLoadTime));
        }));
    }
    
    private void HandlePaidEvent(object a,AdValue e)
    {
        if (e == null) return;

        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            try
            {
                if (AddLocalPriceLog(e.Precision.ToString(), e.Value, e.CurrencyCode))
                {
                    if (nativeAd != null)
                    {
                        string responseId = nativeAd.GetAdUnitId();
                        string mediationAdapterClassName = nativeAd.GetMediationAdapterClassName();
                        GameManager.Firebase.RecordOnPaidEvent("NativeInterstitialAd", e.Value, e.CurrencyCode,
                            e.Precision.ToString(), responseId, mediationAdapterClassName);
                        Log.Debug($"HandleNativeInterstitialPaidEvent:{mediationAdapterClassName}");
                    }
                    Log.Debug($"HandleInterstitialPaidEvent");
                    GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Interstitial_Ads_Paid,
                        new Parameter("Memory", UnityUtility.GetSystemMemory()),
                        new Parameter("Value", e.Value / 1000000d));
                }
                else
                {
                    Log.Debug("HandlePaidEvent:RecordOnPaidEvent :Fail!");
                }
            }
            catch(Exception except)
            {
                Log.Error($"HandlePaidEvent:{except.Message}");
            }
        }));
    }

    private bool AddLocalPriceLog(string precision, long value, string currencyCode)
    {
        adsPrice = value;
        FirebaseAdsJsonUtil.RecordAdsPrice(value, false, true);
        
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
