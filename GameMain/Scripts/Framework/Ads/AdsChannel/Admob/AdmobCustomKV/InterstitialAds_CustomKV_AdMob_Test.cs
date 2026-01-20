using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Analytics;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

public sealed class InterstitialAds_CustomKV_AdMob_Test : AdUnitBase
{
    private const int MaxOffsetIndex = 2;
    
    private InterstitialAd interstitialAd;
    private int offsetIndex = MaxOffsetIndex;
    private float recordStartLoadTime = 0f;

    private KVGroupInfo curKVGroup = null;
    protected override string CurAdsId
    {
        get
        {
            if (curKVGroup == null)
            {
                curKVGroup = CustomKVManager.GetCustomKVGroup(false, offsetIndex);
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
        return interstitialAd != null && IsLoad && (interstitialAd.CanShowAd()||GameManager.DataNode.GetData("IsOpenKVTest", false));
    }

    public override void LoadAd(string[] ids = null)
    {
        if (!IsValid)
            return;
        
        if (curKVGroup != null || IsLoaded()) 
        {
            Debug.Log("[KV]InterstitialAds_CustomKV_AdMob adUnitId already loaded");
            return;
        }
        
        if (this.CurAdsId == null)
        {
            IsValid = false;
            Debug.LogError($"[KV]InterstitialAds_CustomKV_AdMob Can Load Id is Null! Ecpm :{CustomKVManager.InterstitialAdsEcpm}");
            return;
        }
        
        Debug.Log($"[KV]InterstitialAds_CustomKV_AdMob Start Load...");

        loadAdsTimeOut = 65;
        //loadAdsTimeOut = (float)GameManager.Firebase.GetDouble(Constant.RemoteConfig.Remote_KVGroup_TimeOut, 20d); //广告请求超时时间
        ClearEventHandlers(interstitialAd);
        if(interstitialAd!=null)interstitialAd.Destroy();
        
        var adRequest = new AdRequest();
        if (curKVGroup.Key != null && curKVGroup.Value != null)
        {
            adRequest.CustomTargeting.Add(curKVGroup.Key,curKVGroup.Value);
            Debug.Log($"[KV]InterstitialAds_CustomKV_AdMob admob_custom_keyvals:{(curKVGroup!=null?curKVGroup.Value:"???")}|{CustomKVManager.InterstitialAdsEcpm}|n+{offsetIndex}");
        }

        if(recordStartLoadTime==0)recordStartLoadTime = Time.realtimeSinceStartup;

        InterstitialAd.Load(this.CurAdsId, adRequest,(ads,error)=>
        {
            AdmobUtils.ConcurrentQueue.Enqueue(() =>
            {
                if (ads == null || error != null)
                {
                    FailedToLoadAction(error);
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

        // interstitialAd = new SelfInterstitial(this.CurAdsId);
        //
        // if(recordStartLoadTime==0)recordStartLoadTime = Time.realtimeSinceStartup;
        //
        // RegisterEventHandlers(interstitialAd);
        //
        // if (curKVGroup.Key != null && curKVGroup.Value != null) 
        // {
        //     GameManager.Firebase.RecordMessageByEvent("KeyValueIntReq",
        //         new Parameter("AdsId",this.CurAdsId),
        //         new Parameter("Key",curKVGroup.Key),
        //         new Parameter("Value",curKVGroup.Value),
        //         new Parameter("Ecpm",CustomKVManager.InterstitialAdsEcpm));
        //     
        //     Debug.Log($"[KV]InterstitialAds_CustomKV_AdMob admob_custom_keyvals:{(curKVGroup!=null?curKVGroup.Value:"???")}|{CustomKVManager.InterstitialAdsEcpm}|n+{offsetIndex}");
        //     interstitialAd.LoadAd("admob_custom_keyvals", curKVGroup.Key, curKVGroup.Value);
        // }
        // else
        // {
        //     interstitialAd.LoadAd(null,null,null);
        // }
    }

    public override bool Show(object userData)
    {
        if (IsLoaded())
        {
            Debug.Log($"[KV]InterstitialAds_CustomKV_AdMob show ads:{(curKVGroup!=null?curKVGroup.Value:"???")}|n+{offsetIndex}");
            GameManager.CurState = "ShowSelfInterstitialAds_Admob";
            ShowAdsCanvas(true,interstitialAd.Show);
            return true;
        }else if (interstitialAd != null && IsLoad && !interstitialAd.CanShowAd())
        {
            GameManager.Firebase.RecordMessageByEvent("interstitialAdNoCanShowAd");
        }

        return false;
    }

    public override void Dispose()
    {
        Debug.Log($"[KV]InterstitialAds_CustomKV_AdMob Dispose:{(curKVGroup!=null?curKVGroup.Value:"???")}|n+{offsetIndex}");
        
        ShowAdsCanvas(false);
        IsLoad = false;
        curKVGroup = null;
        offsetIndex++;

        if (interstitialAd != null)
        {
            ClearEventHandlers(interstitialAd);
            interstitialAd.Destroy();
            interstitialAd = null;
        }
        base.Dispose();
    }

    private void RegisterEventHandlers(InterstitialAd ad)
    {
        if(ad==null)return;
        ad.OnAdPaid -= HandlePaidEvent;
        ad.OnAdPaid += HandlePaidEvent;
        
        ad.OnAdFullScreenContentClosed -= ClosedAction;
        ad.OnAdFullScreenContentClosed += ClosedAction;
        
        ad.OnAdFullScreenContentFailed -= FailedToShow;
        ad.OnAdFullScreenContentFailed += FailedToShow;
    }

    private void ClearEventHandlers(InterstitialAd ad)
    {
        if(ad==null)return;
        
        ad.OnAdPaid -= HandlePaidEvent;
        ad.OnAdFullScreenContentClosed -= ClosedAction;
        ad.OnAdFullScreenContentFailed -= FailedToShow;
    }
    
    void LoadedAction(object a, EventArgs b)
    {
        HandleInterstitialAdLoaded();
    }

    void FailedToLoadAction(LoadAdError e)
    {
        HandleAdFailedToLoad();
    }

    void FailedToShow(AdError e)
    {
        HandleAdClosed();
    }
    
    void ClosedAction()
    {
        HandleAdClosed();
    }
    
    private void HandleInterstitialAdLoaded()
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            Debug.Log($"[KV]InterstitialAds_CustomKV_AdMob Loaded...:{(curKVGroup!=null?curKVGroup.Value:"???")}|n+{offsetIndex}|loadTime{Time.realtimeSinceStartup-recordStartLoadTime}");
            //打点记录加载成功时长 和次数
            // GameManager.Firebase.RecordMessageByEvent("InterstitialLoadSuccess",
            //     new Parameter("Time",Time.realtimeSinceStartup-recordStartLoadTime),
            //     new Parameter("FailCount",AdsIdModel.GetFailCount()));
            recordStartLoadTime = 0;
            
            IsLoad = true;
            //AdsIdModel.LoadSuccess();
            Debug.Log("[KV]GetMediationValueByKey:" + GetMediationValueByKey());
        }));
    }

    private void HandleAdFailedToLoad()
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            Debug.Log($"[KV]InterstitialAds_CustomKV_AdMob FailedToLoad...{(curKVGroup!=null?curKVGroup.Value:"???")}|n+{offsetIndex}");
            Dispose();
            //AdsIdModel.LoadFail();
            recordStartLoadTime = 0;

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
            Debug.Log($"[KV]InterstitialAds_CustomKV_AdMob Closed...{(curKVGroup!=null?curKVGroup.Value:"???")}|n+{offsetIndex}");

            IsValid = false;//直接结束这次请求
            Dispose();
            
            GameManager.Event.Fire(null,AdsEcpmRefreshEventArgs.Create(false));  
            
            //eventQueue.Enqueue(AdsEvent.Create(() => { LoadAd(); }, AdsDelayLoadTime));
        }));
    }

    private string GetMediationValueByKey()
    {
        string result = string.Empty;
        try
        {
            //result = interstitialAd.GetMediationValueByKey("userGroup");
        }
        catch
        {
            return "???";
        }
        
        return result;
    }
    
    private void HandlePaidEvent(AdValue e)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            if (e == null) return;
            Debug.Log($"[KV]InterstitialAds_CustomKV_AdMob HandlePaidEvent...{(curKVGroup!=null?curKVGroup.Value:"???")}|n+{offsetIndex}|Ecpm:"+(e.Value/1000f));

            try
            {
                if (AddLocalPriceLog(e.Precision.ToString(), e.Value, e.CurrencyCode))
                {
                    if (interstitialAd != null)
                    {
                        ResponseInfo info = interstitialAd.GetResponseInfo();
                        string responseId = info.GetResponseId();
                        string mediationAdapterClassName = info.GetMediationAdapterClassName();
                        GameManager.Firebase.RecordOnPaidEvent("InterstitialAd", e.Value, e.CurrencyCode,
                            e.Precision.ToString(), responseId, mediationAdapterClassName);
                        Log.Debug($"HandleInterstitialPaidEvent:{mediationAdapterClassName}");
                        Debug.Log("[KV]GetMediationValueByKey:" + GetMediationValueByKey());
                    }

                    GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Interstitial_Ads_Paid,
                        new Parameter("Memory", UnityUtility.GetSystemMemory()),
                        new Parameter("Value", e.Value / 1000000d));

                    if (curKVGroup != null)
                    {
                        GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Interstitial_KV_Ads_Paid,
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
        FirebaseAdsJsonUtil.RecordAdsPrice(value,false,false);
        
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
