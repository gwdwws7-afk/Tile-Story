using Firebase.Analytics;
using System;
using System.Globalization;

public sealed partial class FirebaseManager : GameFrameworkModule, IFirebaseManager
{
    /// <summary>
    /// 根据事件记录信息
    /// </summary>
    /// <summary>
    /// 根据SelectContent记录信息
    /// </summary>
    public void RecordMessageByEventSelectContent(string contentType, string ItemIds = null)
    {
        if (isInitFirebaseApp)
        {
            int size = string.IsNullOrEmpty(ItemIds) ? 1 : 2;
            var paramters = new Parameter[size];
            paramters[0] = new Parameter(FirebaseAnalytics.ParameterContentType, contentType);
            if (size == 2) paramters[1] = new Parameter(AnalyticsConstants.ParamItemId, ItemIds);
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventSelectContent, paramters);
        }
        else
        {
            Log.Warning("{0} record failed as Firebase has not been initialized.", contentType);
        }
    }

    /// <summary>
    /// 根据Event记录多重Parameter信息(selectConnect)
    /// </summary>
    public void RecordMessageByEvent(string recordName, params Parameter[] parameters)
    {
        Log.Info($"打点RecordMessageByEvent：{recordName}；{GetParameterContent(parameters)}");
        if (isInitFirebaseApp)
        {
            if (parameters == null || parameters.Length == 0)
            {
              FirebaseAnalytics.LogEvent(recordName);
            }else
              FirebaseAnalytics.LogEvent(recordName, parameters);
        }
        else
        {
            Log.Warning("{0} record failed as Firebase has not been initialized.", recordName);
        }
    }

    public void RecordOnPaidEvent(string source, long value, string currencyCode, string precisionType, string adsUnitId, string mediationAdapterClassName)
    {
      try
      {
        GameManager.AppsFlyer.RecordAdsEventAdmob(value / 1000000d, RegionInfo.CurrentRegion.TwoLetterISORegionName, adsUnitId, source, mediationAdapterClassName, null, currencyCode);
      }
      catch (Exception e)
      {
       Log.Error($"RecordOnPaidEvent:{e.Message}");
      }
        if (isInitFirebaseApp)
        {
            double nowPrice = 0;
#if UNITY_IOS || UNITY_IPHONE || AmazonStore
             nowPrice = value;
#else
            nowPrice = value / 1000000d;
#endif
            FirebaseAnalytics.LogEvent("OnPaidEvent", new Parameter[] {
                                              new Parameter(
                                                FirebaseAnalytics.ParameterContentType, source),
                                              new Parameter(
                                                AnalyticsConstants.ParamItemId, precisionType),
                                              new Parameter(
                                                FirebaseAnalytics.ParameterCurrency, currencyCode),
                                              new Parameter(
                                                FirebaseAnalytics.ParameterValue, nowPrice),
                                              new Parameter(
                                                "adsUnitId",adsUnitId ),
                                              new Parameter(
                                                "mediationAdapterClassName",mediationAdapterClassName ),
                                                });

            FirebaseAnalytics.LogEvent("ad_impression", new Parameter[] {
                                              new Parameter(
                                                FirebaseAnalytics.ParameterContentType, source),
                                              new Parameter(
                                                AnalyticsConstants.ParamItemId, precisionType),
                                              new Parameter(
                                                FirebaseAnalytics.ParameterCurrency, currencyCode),
                                              new Parameter(
                                                FirebaseAnalytics.ParameterValue, nowPrice),
                                              new Parameter(
                                                "adsUnitId",adsUnitId ),
                                              new Parameter(
                                                "mediationAdapterClassName",mediationAdapterClassName ),
                                                });

            if (source == "InterstitialAd"||source == "NativeInterstitialAd")
            {
                FirebaseAnalytics.LogEvent("ad_impression_int", new Parameter[] {
                                              new Parameter(
                                                FirebaseAnalytics.ParameterContentType, source),
                                              new Parameter(
                                                AnalyticsConstants.ParamItemId, precisionType),
                                              new Parameter(
                                                FirebaseAnalytics.ParameterCurrency, currencyCode),
                                              new Parameter(
                                                FirebaseAnalytics.ParameterValue, nowPrice),
                                              new Parameter(
                                                "adsUnitId",adsUnitId ),
                                              new Parameter(
                                                "mediationAdapterClassName",mediationAdapterClassName ),
                                                });
            }
            else if (source == "RewardedAd"||source == "NativeRewardedAd")
            {
                FirebaseAnalytics.LogEvent("ad_impression_rv", new Parameter[] {
                                              new Parameter(
                                                FirebaseAnalytics.ParameterContentType, source),
                                              new Parameter(
                                                AnalyticsConstants.ParamItemId, precisionType),
                                              new Parameter(
                                                FirebaseAnalytics.ParameterCurrency, currencyCode),
                                              new Parameter(
                                                FirebaseAnalytics.ParameterValue, nowPrice),
                                              new Parameter(
                                                "adsUnitId",adsUnitId ),
                                              new Parameter(
                                                "mediationAdapterClassName",mediationAdapterClassName ),
                                                });
            }
        }
        else
        {
            Log.Warning("[RecordOnPaidEvent] record failed as Firebase has not been initialized.");
        }
    }
    
     public void RecordOnPaidEvent(string source, double value, string currencyCode, string precisionType, string adsUnitId, string mediationAdapterClassName)
    {
      try
      {
        GameManager.AppsFlyer.RecordAdsEvent(value, RegionInfo.CurrentRegion.TwoLetterISORegionName, adsUnitId, source, mediationAdapterClassName, null);
      }
      catch (Exception e)
      {
        Log.Error($"RecordOnPaidEvent:{e.Message}");
      }
        if (isInitFirebaseApp)
        {
            double nowPrice = 0;
#if UNITY_IOS || UNITY_IPHONE || AmazonStore
             nowPrice = value;
#else
            nowPrice = value / 1000000d;
#endif
            FirebaseAnalytics.LogEvent("OnPaidEvent", new Parameter[] {
                                              new Parameter(
                                                FirebaseAnalytics.ParameterContentType, source),
                                              new Parameter(
                                                AnalyticsConstants.ParamItemId, precisionType),
                                              new Parameter(
                                                FirebaseAnalytics.ParameterCurrency, currencyCode),
                                              new Parameter(
                                                FirebaseAnalytics.ParameterValue, nowPrice),
                                              new Parameter(
                                                "adsUnitId",adsUnitId ),
                                              new Parameter(
                                                "mediationAdapterClassName",mediationAdapterClassName ),
                                                });

            FirebaseAnalytics.LogEvent("ad_impression", new Parameter[] {
                                              new Parameter(
                                                FirebaseAnalytics.ParameterContentType, source),
                                              new Parameter(
                                                AnalyticsConstants.ParamItemId, precisionType),
                                              new Parameter(
                                                FirebaseAnalytics.ParameterCurrency, currencyCode),
                                              new Parameter(
                                                FirebaseAnalytics.ParameterValue, nowPrice),
                                              new Parameter(
                                                "adsUnitId",adsUnitId ),
                                              new Parameter(
                                                "mediationAdapterClassName",mediationAdapterClassName ),
                                                });

            if (source == "InterstitialAd")
            {
                FirebaseAnalytics.LogEvent("ad_impression_int", new Parameter[] {
                                              new Parameter(
                                                FirebaseAnalytics.ParameterContentType, source),
                                              new Parameter(
                                                AnalyticsConstants.ParamItemId, precisionType),
                                              new Parameter(
                                                FirebaseAnalytics.ParameterCurrency, currencyCode),
                                              new Parameter(
                                                FirebaseAnalytics.ParameterValue, nowPrice),
                                              new Parameter(
                                                "adsUnitId",adsUnitId ),
                                              new Parameter(
                                                "mediationAdapterClassName",mediationAdapterClassName ),
                                                });
            }
            else if (source == "RewardedAd")
            {
                FirebaseAnalytics.LogEvent("ad_impression_rv", new Parameter[] {
                                              new Parameter(
                                                FirebaseAnalytics.ParameterContentType, source),
                                              new Parameter(
                                                AnalyticsConstants.ParamItemId, precisionType),
                                              new Parameter(
                                                FirebaseAnalytics.ParameterCurrency, currencyCode),
                                              new Parameter(
                                                FirebaseAnalytics.ParameterValue, nowPrice),
                                              new Parameter(
                                                "adsUnitId",adsUnitId ),
                                              new Parameter(
                                                "mediationAdapterClassName",mediationAdapterClassName ),
                                                });
            }

        }
        else
        {
            Log.Warning("[RecordOnPaidEvent] record failed as Firebase has not been initialized.");
        }
    }

    private string GetParameterContent(Parameter[] parameters)
    {
        string content = String.Empty;
        if (parameters != null && parameters.Length > 0)
        {
            foreach (var data in parameters)
            {
                content += $"{data.ToString()}    ";
            }
        }
        return content;
    }
}
