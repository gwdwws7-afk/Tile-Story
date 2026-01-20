using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DG.Tweening;
using Firebase.Analytics;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using GoogleMobileAds.Ump.Api;
using UnityEngine;
using ConsentStatus = Firebase.Analytics.ConsentStatus;

/// <summary>
/// 广告管理器
/// </summary>
public sealed class AdsManager_Admob : AdsManagerBase
{
    private string[] Admob_BannerIds => AdsCommon.IsUseKVAds ? new[] { "ca-app-pub-8209077259415431/7705113434" } : new[] { "ca-app-pub-8209077259415431/5355507069" };
    private string[] Admob_InterstitialIds => AdsCommon.IsUseKVAds ? new[] { "ca-app-pub-8209077259415431/3581233265" } : new[] { "ca-app-pub-8209077259415431/6623538467" };
    private string[] Admob_RewardedIds => AdsCommon.IsUseKVAds ? new[] { "ca-app-pub-8209077259415431/9955069920" } : new[] { "ca-app-pub-8209077259415431/2729343725" };

    private string[] Admob_Native_InterstitialIds = { "ca-app-pub-8209077259415431/6938919882" };
    private string[] Admob_Native_RewardIds = { "ca-app-pub-8209077259415431/5212123839" };

    private string[] Admob_Native_InterstitialIds_Change_Normal = {"ca-app-pub-8209077259415431/4104220983"};
    private string[] Admob_Native_RewardIds_Change_Normal ={"ca-app-pub-8209077259415431/3757865929"};
    
    private string[] Low_Admob_BannerIds ={ "ca-app-pub-8209077259415431/5355507069" };
    private string[] Low_Admob_InterstitialIds ={ "ca-app-pub-8209077259415431/4273670623" };
    private string[] Low_Admob_RewardedIds ={ "ca-app-pub-8209077259415431/1739049159" };

    List<string> _testDeviceIds = new List<string>()
    {
        AdRequest.TestDeviceSimulator,
        "12E45B8A41FF25C0139D7E29D3F3420D",
        "B31E697C1DFE1D73230A066CFE6618FC",
        "B97266E352998E5457908DA1430B9E95",
        "3C9977755A4E7C0D3679FFE4C64E4478",
    };

    protected override string[] GetAdsId(AdsType type)
    {
        switch (type)
        {
            case AdsType.Banner:
                return AdsCommon.IsLowPerformanceMachine ? Low_Admob_BannerIds : Admob_BannerIds;
            case AdsType.Interstitial:
                if (AdsCommon.IsInterstitialChangeNative) return Admob_Native_InterstitialIds_Change_Normal;
                return GameManager.Ads.CheckIsUseLowInterstitial()? Low_Admob_InterstitialIds : Admob_InterstitialIds;
            case AdsType.Reward:
                if (AdsCommon.IsRewardChangeNative)return Admob_Native_RewardIds_Change_Normal;
                return GameManager.Ads.CheckIsUseLowRV() ? Low_Admob_RewardedIds : Admob_RewardedIds;
            case AdsType.NativeInterstitial:
                string remoteINTId = AdsCommon.RemoteNativeInterstitialAdId;
                return string.IsNullOrEmpty(remoteINTId)?Admob_Native_InterstitialIds:new []{remoteINTId};
            case AdsType.NativeReward:
                string remoteRWId = AdsCommon.RemoteNativeRewardAdId;
                return string.IsNullOrEmpty(remoteRWId)?Admob_Native_RewardIds:new []{remoteRWId};
        }
        return null;
    }

    protected override AdUnitBase GetBannerAdUnit()
    {
        return new BannerAds_Admob();
    }

    protected override AdUnitBase GetInterstitialAdUnit()
    {
#if UNITY_EDITOR
        return new InterstitialAdsGroup_AdMob();
#endif
        if (AdsCommon.IsInterstitialChangeNative)
        {
            return new Interstitial_NativeAds_AdMob();
        }
        else
        {
            if (GameManager.Firebase.GetBool(Constant.RemoteConfig.Is_Use_KVGroup, false) ||
                PlayerPrefs.GetInt("LocalOpenKV", 0) == 1) 
                return new InterstitialAdsGroup_AdMob();
            else
                return new InterstitialAds_AdMob();
        }
    }

    protected override AdUnitBase GetRewardedAdUnit()
    {
    #if UNITY_EDITOR
        return new RewardAdsGroup_Admob();
    #endif
        if (AdsCommon.IsRewardChangeNative)
        {
            return new Reward_NativeAds_AdMob();
        }
        else
        {
            if (GameManager.Firebase.GetBool(Constant.RemoteConfig.Is_Use_KVGroup, false) ||
                PlayerPrefs.GetInt("LocalOpenKV", 0) == 1) 
                return new RewardAdsGroup_Admob();
            else
                return new RewardAds_Admob();
        }
    }

    protected override AdUnitBase GetNativeInterstitialAdUnit()
    {
#if UNITY_EDITOR
        return null;
#else
        if (AdsCommon.IsOpenNativeInterstitialAds) return new Interstitial_NativeAds_AdMob();
        else return null;
#endif
    }

    protected override AdUnitBase GetNativeRewardAdUnit()
    {
#if UNITY_EDITOR
        return null;
#else
    if (AdsCommon.IsOpenNativeRewardAds) return new Reward_NativeAds_AdMob();
        else return null;
#endif
    }

    public override void Init()
    {
        if(GameManager.PlayerData.NowLevel<AdsCommon.AdsInitLevel)return;

        //if (GameManager.Ads.CheckIsForbidAds()) return;

        if(IsInit)return;
        IsInit = true;

        SetChildDirectedTreatment(GameManager.PlayerData.KidMode);
        //MobileAds.SetiOSAppPauseOnBackground(true);
        MobileAds.RaiseAdEventsOnUnityMainThread = true;
        
        Action initAdmobEvent = () =>
        {
            InitAllAdUnits();

            MobileAds.Initialize(initStatus =>
            {
                AdmobUtils.ConcurrentQueue.Enqueue(() =>
                {
                    Log.Debug($"admob广告初始化结束");
                    IsInitComplete = true;
                    RequestAds();
                });
            });
        };

        try
        {
            Log.Info("Init MobileAds...");
            
            if(AdsCommon.IsOpenUseGprp || GameManager.DataNode.GetData("IsOpenUMPTest", false))
            {
                string testId = PlayerPrefs.GetString("UMPTestDevericeId", "");
                if (!string.IsNullOrEmpty(testId))
                {
                    _testDeviceIds.Add(testId);
                }
                
                //设置测试账号
                var debugSettings = new ConsentDebugSettings
                {
                    DebugGeography = DebugGeography.EEA,
                    TestDeviceHashedIds = _testDeviceIds,
                };

                ConsentRequestParameters request = new ConsentRequestParameters
                {
                    TagForUnderAgeOfConsent = false,
                    ConsentDebugSettings = debugSettings,
                };
                Log.Debug($">>>DeviceIdCount:{_testDeviceIds.Count}");
                ConsentInformation.Update(request, (consentError) =>    
                {
                    Log.Debug(">>> Update-callback");
                    if (consentError != null)
                    {
                        Log.Error($">>> consentError: {consentError.Message}");
                        if(ConsentInformation.CanRequestAds()) initAdmobEvent();
                        return;
                    }

                    ConsentForm.LoadAndShowConsentFormIfRequired((formError) =>
                    {
                        if (formError != null)
                        {
                            Log.Error($">>> formError: {formError.Message}");
                        }

                        if (ConsentInformation.CanRequestAds())
                        {
                            string purposeConsents = ApplicationPreferences.GetString("IABTCF_PurposeConsents");
                            // Purposes are zero-indexed. Index 0 contains information about Purpose 1.
                            if (!string.IsNullOrEmpty(purposeConsents))
                            {
                                int resultLength = purposeConsents.Length;
                                char purposeOneString = purposeConsents[0];
                                bool hasConsentForPurposeOne = purposeOneString == '1';

                                if (resultLength > 1)
                                {
                                    hasConsentForPurposeOne = true;
                                }
                                
                                // 更新用户权限
                                Dictionary<ConsentType, ConsentStatus> dict =
                                    new Dictionary<ConsentType, ConsentStatus>();
                                dict.Add(ConsentType.AnalyticsStorage, hasConsentForPurposeOne ? ConsentStatus.Granted : ConsentStatus.Denied);
                                dict.Add(ConsentType.AdStorage, hasConsentForPurposeOne ? ConsentStatus.Granted : ConsentStatus.Denied);
                                dict.Add(ConsentType.AdUserData, hasConsentForPurposeOne ? ConsentStatus.Granted : ConsentStatus.Denied);
                                dict.Add(ConsentType.AdPersonalization, hasConsentForPurposeOne ? ConsentStatus.Granted : ConsentStatus.Denied);
                                FirebaseAnalytics.SetConsent(dict);
                                
                                // 将分析后的数据上传Firebase
                                GameManager.Firebase.RecordMessageByEvent(hasConsentForPurposeOne ? "GDPR_GRANTED" : "GDPR_DENIED");
                            }
                            
                            Log.Debug(">>> UMP  AdsInit");
                            initAdmobEvent();
                        }
                    });
                });
            }
            else
            {
                Log.Debug(">>> AdsInit");
                initAdmobEvent();
            }
        }
        catch (Exception e)
        {
            Log.Error($"MobileAds.Initialize Error {e.Message}");
        }
    }

    public override void Update(float elapseSeconds, float realElapseSeconds)
    {
        if (AdmobUtils.ConcurrentQueue != null&&AdmobUtils.ConcurrentQueue.Count>0)
        {
            if (AdmobUtils.ConcurrentQueue.TryDequeue(out Action action))
            {
                action?.InvokeSafely();
            }
        }

        base.Update(elapseSeconds, realElapseSeconds);
    }

    /// <summary>
    /// 设置儿童保护模式
    /// </summary>
    public override void SetChildDirectedTreatment(bool open)
    {
        if (open)
        {
            RequestConfiguration requestConfiguration = new RequestConfiguration();
            requestConfiguration.TagForChildDirectedTreatment=TagForChildDirectedTreatment.True;
            requestConfiguration.MaxAdContentRating=MaxAdContentRating.G;
            MobileAds.SetRequestConfiguration(requestConfiguration);
        }
        else
        {
            RequestConfiguration requestConfiguration = new RequestConfiguration();
            requestConfiguration.TagForChildDirectedTreatment=TagForChildDirectedTreatment.Unspecified;
            requestConfiguration.MaxAdContentRating=MaxAdContentRating.Unspecified;
            MobileAds.SetRequestConfiguration(requestConfiguration);
        }
    }
}

