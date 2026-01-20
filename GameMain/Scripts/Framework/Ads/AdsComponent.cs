using UnityEngine;
using System.Collections;
using System;
using System.Globalization;
using Firebase.Analytics;
using System.Collections.Generic;

/// <summary>
/// 广告组件
/// </summary>
public sealed class AdsComponent : GameFrameworkComponent
{
    private IAdsManager adsManager = null;

    private bool isIniting = false;
    public bool IsRemovePopupAds
    {
        get=>GameManager.PlayerData.IsRemoveAds;
        set
        {
            GameManager.PlayerData.IsRemoveAds = value;
            if(adsManager!=null)adsManager.IsRemovePopupAds = value;
        }
    }

    public bool IsRequestingInterstitialAd { get; set; }

    public bool IsRequestingRewardAd { get; set; }

    public bool IsRequestingBannerAd { get; set; }

    private float refreshingBannerAdTimer = 0f;
    public bool IsRefreshingBannerAd
    {
        get => refreshingBannerAdTimer > 0;
        set => refreshingBannerAdTimer = 5f;
    }

    private LinkedList<string> hideBannerRequests = new LinkedList<string>();

    public bool IsBannerAdInHideStatus => hideBannerRequests.Count > 0;

    public bool IsRequestingAd
    {
        get
        {
            return IsRequestingInterstitialAd || IsRequestingRewardAd || IsRequestingBannerAd;
        }
    }

    public bool IsInitComplete
    {
        get
        {
            return adsManager != null && adsManager.IsInitComplete;
        }
    }

    public bool CheckIsUseLowInterstitial()
    {
        if (SystemInfoManager.CheckIsSpecialDevice())
        {
            return GameManager.Firebase.GetBool(Constant.RemoteConfig.Is_Use_Special_Device_Strage_Use_Low_Interstitial, false);
        }

        return false;
    }

    public bool CheckIsUseLowRV()
    {
        if (SystemInfoManager.CheckIsSpecialDevice())
        {
            return GameManager.Firebase.GetBool(Constant.RemoteConfig.Is_Use_Special_Device_Strage_Use_Low_RV, false);
        }

        return false;
    }
    
    private float recordInitTime = 0;

    private void InitAdsManager()
    {
        if(adsManager!=null)return;
        
#if AmazonStore||UNITY_IOS||UNITY_IPHONE
        adsManager = new AdsManager_Max();
#else
        if (AdsCommon.IsAdsUseAdmob)
        {
            adsManager = new AdsManager_Admob();
        }
        else
        {
            adsManager = new AdsManager_YandexMyTargetMix();
        }
#endif
    }

    private void Update()
    {
        if(adsManager!=null)adsManager.Update(Time.deltaTime, Time.unscaledDeltaTime);

        if (refreshingBannerAdTimer > 0)
            refreshingBannerAdTimer -= Time.deltaTime;
    }

    public void Init()
    {
        #if AmazonStore||UNITY_IOS||UNITY_IPHONE
            RequestAds();
        #else
            StartCoroutine(DelayInit(5f));
        #endif
    }

    private IEnumerator DelayInit(float delayTime)
    {
        if (IsInitComplete) yield break;

        if (recordInitTime == 0) recordInitTime = Time.realtimeSinceStartup;
        isIniting = true;
        while (!GameManager.Firebase.IsFetchRemoteConfig && delayTime > 0)
        {
            delayTime -= Time.deltaTime;
            yield return null;
        }
        isIniting = false;

        RequestAds();
    }

    public void RequestAds()
    {
        if(isIniting)return;
        
        InitAdsManager();

        if (adsManager != null)
        {
            adsManager.Init();
            adsManager.RequestAds();
        }
    }

    public void ReInitAds()
    {
        if(adsManager!=null)adsManager.ReInitAds();
    }

    /// <summary>
    /// 检测是否有插屏广告加载完毕
    /// </summary>
    /// <returns>是否加载完毕</returns>
    public bool CheckInterstitialAdIsLoaded()
    {
        return adsManager != null ? adsManager.CheckInterstitialAdIsLoaded() : false;
    }

    /// <summary>
    /// 检测是否有奖励广告加载完毕
    /// </summary>
    /// <returns>是否加载完毕</returns>
    public bool CheckRewardedAdIsLoaded()
    {
        int todayMaxTime = (int)GameManager.Firebase.GetLong(Constant.RemoteConfig.RV_Times_Total, 99);
        if (GameManager.PlayerData.TodayWatchRewardedAdTime >= todayMaxTime)
        {
            //Log.Info("CheckRewardedAdIsLoaded fail - reach max TodayWatchRewardedAdTime {0}", todayMaxTime.ToString());
            return false;
        }

        return adsManager != null ? adsManager.CheckRewardedAdIsLoaded() : false;
    }

    /// <summary>
    /// 检测奖励广告是否可以展示
    /// </summary>
    public bool CheckRewardAdCanShow()
    {
        var coldTime = PlayerPrefs.GetString("RewardAdColdingTime", null);
        if (string.IsNullOrEmpty(coldTime) || (DateTime.TryParse(coldTime, out DateTime time) && DateTime.Now > time))   
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 设置奖励广告冷却时间
    /// </summary>
    public void SetRewardAdColdingTime()
    {
        int coldMinutes = (int)GameManager.Firebase.GetLong(Constant.RemoteConfig.Level_RV_CD, 0);
        if (coldMinutes > 0) 
            PlayerPrefs.SetString("RewardAdColdingTime", DateTime.Now.AddMinutes(coldMinutes).ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// 展示奖励广告
    /// </summary>
    /// <returns>是否展示成功</returns>
    public bool ShowRewardedAd(string rewardType)
    {
        bool isCanShowRV = CheckRewardedAdIsLoaded();
        if (isCanShowRV)
        {
            //展示ads动画
            GameManager.UI.ShowUIForm("AdsCanvasPanel", UIFormType.TopUI);
            
            //延迟一会展示广告
            GameManager.Task.AddDelayTriggerTask(0.5f, () =>
            {
                if (adsManager.ShowRewardedAd(rewardType))
                {
                    GameManager.Task.AddDelayTriggerTask(0.4f, () =>
                    {
                        GameManager.PlayerData.TodayWatchRewardedAdTime += 1;
                        GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Reward_Ads_Show);
                        GameManager.Event.Fire(this,CommonEventArgs.Create(CommonEventType.RewardAdsShownEvent));
                    });
                }   
            });
            GameManager.Task.AddDelayTriggerTask(1.5f, () =>
            {
                GameManager.UI.HideUIForm("AdsCanvasPanel");
            });
        }

        return isCanShowRV;
    }

    /// <summary>
    /// 展示插屏广告
    /// </summary>
    /// <returns>是否展示成功</returns>
    public bool ShowInterstitialAd(Action finishAction=null)
    {
        if (GameManager.PlayerData.NowLevel <= AdsCommon.InterstitialNeedLevel)
        {
            finishAction?.InvokeSafely();
            return false;
        }

        RecordShowInterstitialFail();
        
        if (!IsRemovePopupAds && GameManager.PlayerData.NowLevel > AdsCommon.InterstitialNeedLevel) 
            GameManager.Firebase.Should_show_Int();//应该展示插屏打点

        bool isCanShowInterstitialAd = CheckInterstitialAdIsLoaded();
        if (isCanShowInterstitialAd)
        {
            GameManager.UI.ShowUIForm("AdsCanvasPanel", UIFormType.TopUI);
            GameManager.Task.AddDelayTriggerTask(0.5f, () =>
            {
                if (adsManager.ShowInterstitial())
                {
                    GameManager.Task.AddDelayTriggerTask(0.4f, () =>
                    {
                        GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Show_Interstitial);

                        GameManager.PlayerData.WatchInterstitialAdTimeToday += 1;
                        GameManager.PlayerData.AFShowInterstitial += 1;
                        if (GameManager.PlayerData.AFShowInterstitial % 5 == 0 && GameManager.PlayerData.AFShowInterstitial >= 80 && GameManager.PlayerData.AFShowInterstitial <= 100)
                            GameManager.AppsFlyer.SendShowInterstitialEvent();
                        if (!GameManager.PlayerData.LifeTimeEverShowedNoAdsPanel || GameManager.PlayerData.WatchInterstitialAdTimeToday == 5)
                        {
                            if (!GameManager.PlayerData.TodayEverShowedNoAdsPanel)
                                GameManager.PlayerData.ShowRemoveAdsMenuWhenBackToMap = true;
                        }
                    });
                }
            });
            
            GameManager.Task.AddDelayTriggerTask(1.5f, () =>
            {
                finishAction?.InvokeSafely();
                GameManager.UI.HideUIForm("AdsCanvasPanel");
            });
        }
        else
        {
            finishAction?.InvokeSafely();
        }

        return isCanShowInterstitialAd;
    }
    
    /// <summary>
    /// 设置儿童保护模式
    /// </summary>
    public void SetChildDirectedTreatment(bool open)
    {
        if(adsManager!=null)adsManager.SetChildDirectedTreatment(open);
    }

    public int BannerAdsHeight
    {
        get
        {
            if(adsManager!=null)return adsManager.GetBannerAdsHeight();
            return 0;
        }
    }

    public void DestroyBannerAndReLoad()
    {
        if(adsManager!=null)adsManager.DestroyBannerAndReLoad();
    }

    public void ShowBanner(string request = null, bool clearRequests = false)
    {
        if (!string.IsNullOrEmpty(request) && hideBannerRequests.Contains(request)) 
            hideBannerRequests.Remove(request);

        if (clearRequests)
            hideBannerRequests.Clear();

        if (adsManager != null && hideBannerRequests.Count == 0) adsManager.ShowBanner();
    }

    public void HideBanner(string request = null)
    {
        if (!string.IsNullOrEmpty(request) && !hideBannerRequests.Contains(request)) 
            hideBannerRequests.AddLast(request);

        if (adsManager != null) adsManager.HideBanner();
    }
    
    public void RecordShowInterstitialFail()
    {
        //等级没达到
        bool isCanShowInterstitial = GameManager.PlayerData.NowLevel > AdsCommon.InterstitialNeedLevel;
        if(!isCanShowInterstitial)return;
        //去广告
       if(IsRemovePopupAds)return;
       //初始化未完成
       if(!IsInitComplete&& (Time.realtimeSinceStartup-recordInitTime)<=5*60)return;
        //if (CheckIsForbidInterstitial()) return;

       //网络
       bool isNoNet = Application.internetReachability == NetworkReachability.NotReachable;
       //是否填充
       bool isFill = CheckInterstitialAdIsLoaded();
       
       if (!isFill)
       {
           GameManager.Firebase.RecordMessageByEvent("AdInterShowFail",new Parameter("key","total"));
           
           if (isNoNet)
           {
               GameManager.Firebase.RecordMessageByEvent("AdInterShowFail",new Parameter("key","NoNet"));
           }
           else
           {
               GameManager.Firebase.RecordMessageByEvent("AdInterShowFail",new Parameter("key","NoFill"));
           }
       }
    }
}
