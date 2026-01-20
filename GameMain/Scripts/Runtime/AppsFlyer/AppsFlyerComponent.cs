using AppsFlyerSDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// AppsFlyer组件
    /// </summary>
    public sealed class AppsFlyerComponent : GameFrameworkComponent
    {
        private string devKey = "gAXJCZmwDRExevNM3BpSfd";
        private string appID = "6504067069";

        private bool isInit = false;
        public void Init()
        {
            isInit = true;
            Log.Info("Init AppsFlyer...");
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyer.waitForATTUserAuthorizationWithTimeoutInterval(60);
#endif
            AppsFlyer.initSDK(devKey, appID);
            AppsFlyer.startSDK();
            AppsFlyer.setIsDebug(false);
        }

        public void SendPurchaseEvent(Dictionary<string, string> eventValues)
        {
            if (isInit) AppsFlyer.sendEvent(AFInAppEvents.PURCHASE, eventValues);
        }

        public void SendShowSuccessInterstitialEvent()
        {
            if (isInit) AppsFlyer.sendEvent("af_show_success_interstitial", null);
        }

        public void SendShowFailInterstitialEvent()
        {
            if (isInit) AppsFlyer.sendEvent("af_show_fail_interstitial", null);
        }

        public void SendShowRewardedAdsEvent()
        {
            if (isInit) AppsFlyer.sendEvent("af_show_rewarded_ads", null);
        }

        public void SendGameWinEvent()
        {
            if (isInit) AppsFlyer.sendEvent($"levelsuccess_{GameManager.PlayerData.AFPassLevel}", null);
        }

        public void SendShowInterstitialEvent()
        {
            if (isInit) AppsFlyer.sendEvent($"af_interstitial_{GameManager.PlayerData.AFShowInterstitial}", null);
        }

        public void RecordAdsEvent(double value, string country, string adUnit, string adType, string placement, string ecpmPayload)
        {
            if (isInit)
            {
                //Dictionary<string, string> additionalParams = new Dictionary<string, string>
                //{
                //    { AFAdRevenueEvent.COUNTRY, country },
                //    { AFAdRevenueEvent.AD_UNIT, adUnit },
                //    { AFAdRevenueEvent.AD_TYPE, adType },
                //    { AFAdRevenueEvent.PLACEMENT, placement },
                //    { AFAdRevenueEvent.ECPM_PAYLOAD, ecpmPayload },
                //};

                //AppsFlyerAdRevenue.logAdRevenue(placement,
                //    AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeApplovinMax,
                //    value,
                //    "USD",
                //    additionalParams);
                Dictionary<string, string> additionalParams = new Dictionary<string, string>();
                additionalParams.Add(AdRevenueScheme.COUNTRY, country);
                additionalParams.Add(AdRevenueScheme.AD_UNIT, adUnit);
                additionalParams.Add(AdRevenueScheme.AD_TYPE, adType);
                additionalParams.Add(AdRevenueScheme.PLACEMENT, placement);
                var logRevenue = new AFAdRevenueData(placement, MediationNetwork.ApplovinMax, "USD", value);
                AppsFlyer.logAdRevenue(logRevenue, additionalParams);
            }
        }

        //只在亚马逊上打点
        public void RecordAdsEventMintergral(double value, string country, string adUnit, string adType, string placement, string ecpmPayload)
        {
#if AmazonStore
            if (isInit)
            {
                Dictionary<string, string> additionalParams = new Dictionary<string, string>
                {
                    { AdRevenueScheme.COUNTRY, country },
                    { AdRevenueScheme.AD_UNIT, adUnit },
                    { AdRevenueScheme.AD_TYPE, adType },
                    { AdRevenueScheme.PLACEMENT, placement },
                    { AFInAppEvents.REVENUE, value.ToString(CultureInfo.InvariantCulture) },
                };

                AppsFlyer.sendEvent("ads_revenue", additionalParams);
            }
#endif
        }

        public void RecordAdsEventAdmob(double value, string country, string adUnit, string adType, string placement, string ecpmPayload, string currencyCode)
        {
            if (isInit)
            {
                //Dictionary<string, string> additionalParams = new Dictionary<string, string>
                //{
                //    { AFAdRevenueEvent.COUNTRY, country },
                //    { AFAdRevenueEvent.AD_UNIT, adUnit },
                //    { AFAdRevenueEvent.AD_TYPE, adType },
                //    { AFAdRevenueEvent.PLACEMENT, placement },
                //    { AFAdRevenueEvent.ECPM_PAYLOAD, ecpmPayload },
                //};
                //AppsFlyerAdRevenue.logAdRevenue(placement,
                //    AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeGoogleAdMob,
                //    value,
                //    currencyCode,
                //    additionalParams);
                Dictionary<string, string> additionalParams = new Dictionary<string, string>();
                additionalParams.Add(AdRevenueScheme.COUNTRY, country);
                additionalParams.Add(AdRevenueScheme.AD_UNIT, adUnit);
                additionalParams.Add(AdRevenueScheme.AD_TYPE, adType);
                additionalParams.Add(AdRevenueScheme.PLACEMENT, placement);
                var logRevenue = new AFAdRevenueData(placement, MediationNetwork.GoogleAdMob, currencyCode, value);
                AppsFlyer.logAdRevenue(logRevenue, additionalParams);
            }
        }
    }
}