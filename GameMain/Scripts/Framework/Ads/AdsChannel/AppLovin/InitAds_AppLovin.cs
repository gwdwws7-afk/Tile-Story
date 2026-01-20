#if AmazonStore || UNITY_IOS || UNITY_IPHONE

using System;
using System.Collections;
using System.Collections.Generic;
using AmazonAds;
using Firebase.Analytics;
using UnityEngine;
public static class InitAds_AppLovin
{
    public static void Init(Action<bool> callback = null)
    {
        if(MaxSdk.IsInitialized())
            return;
        MaxSdkCallbacks.OnSdkInitializedEvent += (config) =>
        {
            Debug.Log("MaxSdk has initialized "+ config.IsSuccessfullyInitialized);
            callback?.Invoke(config.IsSuccessfullyInitialized);
        };

#if !UNITY_EDITOR
#if AmazonStore
        Amazon.Initialize("4b07a53e-4924-4649-a9e3-a86d7a641946");
        Amazon.SetAdNetworkInfo(new AdNetworkInfo(DTBAdNetwork.MAX));
#elif UNITY_IOS || UNITY_IPHONE
        Amazon.Initialize("cad9d1e5-445d-4bd7-a596-7239343f5c1d");
        Amazon.SetAdNetworkInfo(new AdNetworkInfo(DTBAdNetwork.MAX));
#endif
#endif

        MaxSdk.SetSdkKey("GAzhLBM7bH_iRX8lBMpe_QRfi0Ti3ZeKkc04oNCOfB55iErubdXa3-Em_iS3G6N_iO4VmHuPS-VADL3pzaN0Zc");
        MaxSdk.InitializeSdk();
        Debug.Log("Start Initialize MaxSdk");

        // Attach callbacks based on the ad format(s) you are using
        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
    }
    
    private static void OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Log.Info($"OnAdRevenuePaidEvent:{adUnitId}:{MaxSdk.GetSdkConfiguration().CountryCode}:{adInfo.Revenue}");
        
        double revenue = adInfo.Revenue;
        if(revenue<0)return;
    
        string countryCode = MaxSdk.GetSdkConfiguration().CountryCode;
        
        AdsTaichiRecordManager.OnAdRevenuePaidEvent(revenue,countryCode,false);
    }

    public static bool IsInit => MaxSdk.IsInitialized();
}
#endif
