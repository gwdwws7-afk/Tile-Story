using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Analytics;
using UnityEngine;

//taichi 3.0
public class AdsTaichiRecordManager
{
   public static void OnAdRevenuePaidEvent(double revenue,string currencyCode,bool isAdmob=true)
   {
      Log.Info($"OnAdRevenuePaidEvent:{currencyCode}:{revenue}");
        
      if(revenue<=0)return;

      revenue =isAdmob?(revenue / 1000000d):revenue;//admob 需要除以一百万，applovin不变

      if (isAdmob)
      {
          LogPaidEventAdmob(currencyCode,revenue);
      }
      else
      {
          LogPaidEventApplovin(currencyCode,revenue);
      }

      //taichi2.5
      AdsPriceRecord.Instance.RecordDailyAdsValue(revenue);

      //taichi3.0
      LogTaichiFirebaseAdRevenueEvent(revenue);
   }

   public static Dictionary<string, float> TotalAdsRevenueDictionary = new Dictionary<string, float>()
   {
       {"Total_Ads_Revenue_001",0.01f},
       {"Total_Ads_Revenue_002",0.02f},
       {"Total_Ads_Revenue_003",0.03f},
       {"Total_Ads_Revenue_005",0.05f},
       {"Total_Ads_Revenue_01",0.1f},
   };
   
   private static void LogTaichiFirebaseAdRevenueEvent(double revenue)
   {
       foreach (var data in TotalAdsRevenueDictionary)
       {
           string cacheKeyName = $"{data.Key}_Cache";
           float newTotal = (float)(PlayerPrefs.GetFloat(cacheKeyName, 0) + revenue); 

           //超过之后打点并清除 缓存
           if (newTotal >= data.Value)
           {
               LogTaichiTroasFirebaseAdRevenueEvent(data.Key,newTotal);
               PlayerPrefs.SetFloat(cacheKeyName, 0);
           }else
               PlayerPrefs.SetFloat(cacheKeyName,newTotal);
           
           Log.Info($"Taichi：：LogTaichiFirebaseAdRevenueEvent:{data.Key}:newTotal:{newTotal}+：：cacheKeyName：{cacheKeyName}");
       }
       PlayerPrefs.Save();
   }

   private static void LogTaichiTroasFirebaseAdRevenueEvent(string eventName,float value)
   {
       Log.Info($"Taichi：：RecordMessageByEvent:：eventName:{eventName}:newTotal:{value}");
      GameManager.Firebase.RecordMessageByEvent(eventName,
         new Parameter(FirebaseAnalytics.ParameterValue,value),
         new Parameter(FirebaseAnalytics.ParameterCurrency,"USD"));
   }
   
   private static void LogPaidEventAdmob(string countryCode,double value)
   {
       GameManager.Firebase.RecordMessageByEvent("AdmobAdsPaidEvent",
           new Parameter("CountryCode",countryCode),
           new Parameter("Revenue",value));
   }
   
   private static void LogPaidEventApplovin(string countryCode,double value)
   {
       GameManager.Firebase.RecordMessageByEvent("ApplovinAdsPaidEvent",
           new Parameter("CountryCode",countryCode),
           new Parameter("Revenue",value));
   }
}

public class AdsPriceRecord
{
    private static AdsPriceRecord instance;
    public static AdsPriceRecord Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new AdsPriceRecord();
            }
            return instance;
        }
    }
    private const string RecordAdsDataKey = "RecordAdsDataKey";
    public RecordAdsData AdsData
    {
        get
        {
            if (!PlayerPrefs.HasKey(RecordAdsDataKey))
            {
                var data = new RecordAdsData();
                data.DayNum = DateTime.Now.GetDay();
                data.IsSendFirebase = new bool[5];

                PlayerPrefs.SetString(RecordAdsDataKey, JsonUtility.ToJson(data));
                PlayerPrefs.Save();
                return data;
            }
            var value = JsonUtility.FromJson<RecordAdsData>(PlayerPrefs.GetString(RecordAdsDataKey));
            if (value.DayNum != DateTime.Now.GetDay())
            {
                PlayerPrefs.DeleteKey(RecordAdsDataKey);
                PlayerPrefs.Save();
                return AdsData;
            }
            return value;
        }
        set
        {
            PlayerPrefs.SetString(RecordAdsDataKey, JsonUtility.ToJson(value));
            PlayerPrefs.Save();
        }
    }


    private List<double> adsConfig;
    private List<double> GetFirebaseAdsConfig
    {
        get
        {
            if (adsConfig != null) return adsConfig;

            adsConfig = new List<double>();
            adsConfig.Add(GameManager.Firebase.GetDouble(Constant.RemoteConfig.TaiChi_Top50Percent));
            adsConfig.Add(GameManager.Firebase.GetDouble(Constant.RemoteConfig.TaiChi_Top40Percent));
            adsConfig.Add( GameManager.Firebase.GetDouble(Constant.RemoteConfig.TaiChi_Top30Percent));
            adsConfig.Add( GameManager.Firebase.GetDouble(Constant.RemoteConfig.TaiChi_Top20Percent));
            adsConfig.Add( GameManager.Firebase.GetDouble(Constant.RemoteConfig.TaiChi_Top10Percent));
            return adsConfig;
        }
    }

    public void RecordDailyAdsValue(double adsPrice)
    {
        var adsData = AdsData;

        adsData.TotalAdsValue += adsPrice;
        adsData.RecordPriceCount++;
        adsData.RecordAllPriceStr += $"{adsData.RecordPriceCount}:{adsPrice}|";

        int index = GetIndex(adsData.TotalAdsValue);

        if (index > 0 && !adsData.IsSendFirebase[index - 1])
        {
            for (int i = 0; i < index; i++)
            {
                if (!adsData.IsSendFirebase[i])
                {
                    RecordDataByEvent(i);
                    adsData.IsSendFirebase[i] = true;
                }
            }
        }
        AdsData = adsData;
    }

    private void RecordDataByEvent(int index)
    {
        switch (index)
        {
            case 0:
                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.AdLTV_OneDay_Top50Percent);
                break;
            case 1:
                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.AdLTV_OneDay_Top40Percent);
                break;
            case 2:
                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.AdLTV_OneDay_Top30Percent);
                break;
            case 3:
                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.AdLTV_OneDay_Top20Percent);
                break;
            default:
                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.AdLTV_OneDay_Top10Percent);
                break;
        }
    }

    private int GetIndex(double totalPrice)
    {
        int index = 0;
        if (totalPrice >= GetFirebaseAdsConfig[0] && totalPrice < GetFirebaseAdsConfig[1])
        {
            index = 1;
        }
        else if (totalPrice >= GetFirebaseAdsConfig[1] && totalPrice < GetFirebaseAdsConfig[2])
        {
            index = 2;
        }
        else if (totalPrice >= GetFirebaseAdsConfig[2] && totalPrice < GetFirebaseAdsConfig[3])
        {
            index = 3;
        }
        else if (totalPrice >= GetFirebaseAdsConfig[3] && totalPrice < GetFirebaseAdsConfig[4])
        {
            index = 4;
        }
        else if (totalPrice >= GetFirebaseAdsConfig[4])
        {
            index = 5;
        }
        return index;
    }
    [System.Serializable]
    public class RecordAdsData
    {
        public int DayNum;//记录天数
        public double TotalAdsValue;//当天全部广告价值
        public int RecordPriceCount;//记录次数
        public string RecordAllPriceStr;//记录所有广告情况
        public bool[] IsSendFirebase = new bool[5];//记录是否发送过
    }
}
public partial class UnityUtility
{
    public static DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    /// <summary>
    /// 获取零点时间
    /// </summary>
    public static int GetDay(this DateTime datetime)
    {
        return (int)(datetime - epoch).Days;
    }
}
