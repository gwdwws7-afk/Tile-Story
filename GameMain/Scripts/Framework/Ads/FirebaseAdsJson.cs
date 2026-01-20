using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using Random = System.Random;

public class FirebaseAdsJson
{
    public List<AdsGroup> Rewarded=null;
    public List<AdsGroup> Interstitial=null;
}

public class AdsGroup
{
    public List<float> Range=null;
    public string Key;
    public string Value;
    public string AdsId;
}

public static class FirebaseAdsJsonUtil
{
    private static FirebaseAdsJson recordJson = null;
    public static FirebaseAdsJson Json
    {
        get
        {
            if (recordJson == null)
            {
                //远程获取 有的话用远程的
                // var remoteJson = GameManager.Firebase.GetString(Constant.RemoteConfig.Json_KV_Ads_Ids, string.Empty);
                // if (!string.IsNullOrEmpty(remoteJson))
                // {
                //     Log.Info($"remoteJson:{remoteJson}");
                //     var jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<FirebaseAdsJson>(remoteJson);
                //     if(jsonData!=null&& (jsonData.Rewarded!=null||jsonData.Interstitial!=null))
                //     {
                //         recordJson = jsonData;
                //         return recordJson;
                //     }
                // }

                //本地拿数据
                recordJson = new FirebaseAdsJson();
                
                var rvList= new List<AdsGroup>();
                
                rvList.Add(new AdsGroup()
                {
                    Range =new List<float>{ 10000, 40000 },
                    Key = "userGroup",
                    Value = "RVGroup0",
                    AdsId = "ca-app-pub-8209077259415431/7327671465"
                });
                rvList.Add(new AdsGroup()
                {
                    Range =new List<float>{ 30, 10000 },
                    Key = "userGroup",
                    Value = "RVGroup1",
                    AdsId = "ca-app-pub-8209077259415431/7327671465"
                });
                rvList.Add(new AdsGroup()
                {
                    Range =new List<float> { 15, 30 },
                    Key = "userGroup",
                    Value = "RVGroup2",
                    AdsId = "ca-app-pub-8209077259415431/7327671465"
                });
                rvList.Add(new AdsGroup()
                {
                    Range =new List<float> {6, 15 },
                    Key = "userGroup",
                    Value = "RVGroup3",
                    AdsId = "ca-app-pub-8209077259415431/7327671465"
                });
                rvList.Add(new AdsGroup()
                {
                    Range =new List<float> { 3, 6 },
                    Key = "userGroup",
                    Value = "RVGroup4",
                    AdsId = "ca-app-pub-8209077259415431/7327671465"
                });
                rvList.Add(new AdsGroup()
                {
                    Range =new List<float> { 1, 3 },
                    Key = "userGroup",
                    Value = "RVGroup5",
                    AdsId = "ca-app-pub-8209077259415431/7327671465"
                });
                rvList.Add(new AdsGroup()
                {
                    Range =new List<float> { 0, 1 },
                    Key = "userGroup",
                    Value = "RVGroup6",
                    AdsId = "ca-app-pub-8209077259415431/7327671465"
                });
                recordJson.Rewarded = rvList;

                // var list = new List<AdsGroup>();
                //
                // list.Add(new AdsGroup()
                // {
                //     Range =new List<float>{ 10000, 40000 },
                //     Key = "userGroup",
                //     Value = "INTGroup0",
                //     AdsId = "ca-app-pub-8209077259415431/7098032798"
                // });
                // list.Add(new AdsGroup()
                // {
                //     Range =new List<float>{ 30, 10000 },
                //     Key = "userGroup",
                //     Value = "INTGroup1",
                //     AdsId = "ca-app-pub-8209077259415431/7098032798"
                // });
                // list.Add(new AdsGroup()
                // {
                //     Range =new List<float> { 15, 30 },
                //     Key = "userGroup",
                //     Value = "INTGroup2",
                //     AdsId = "ca-app-pub-8209077259415431/7098032798"
                // });
                // list.Add(new AdsGroup()
                // {
                //     Range =new List<float> { 6, 15 },
                //     Key = "userGroup",
                //     Value = "INTGroup3",
                //     AdsId = "ca-app-pub-8209077259415431/7098032798"
                // });
                // list.Add(new AdsGroup()
                // {
                //     Range =new List<float> { 3, 6 },
                //     Key = "userGroup",
                //     Value = "INTGroup4",
                //     AdsId = "ca-app-pub-8209077259415431/7098032798"
                // });
                // list.Add(new AdsGroup()
                // {
                //     Range =new List<float> { 1, 3 },
                //     Key = "userGroup",
                //     Value = "INTGroup5",
                //     AdsId = "ca-app-pub-8209077259415431/7098032798"
                // });
                // list.Add(new AdsGroup()
                // {
                //     Range =new List<float> { 0, 1 },
                //     Key = "userGroup",
                //     Value = "INTGroup6",
                //     AdsId = "ca-app-pub-8209077259415431/7098032798"
                // });
                // recordJson.Interstitial = list;
                #if UNITY_EDITOR
                var str= Newtonsoft.Json.JsonConvert.SerializeObject(recordJson,  Newtonsoft.Json.Formatting.Indented);
                Log.Info($"Json:{str}");
                #endif
            }
            return recordJson;
        }
    }
    
    private static string[] rewardedList = null;
    private static string[] interstitialList = null;

    public static float rvAdsPrice = 20000;
    public static float interAdsPrice = 20000;

    private static bool IsOpenFirebaseAdsIds => false;
    
    private static bool isDebugger=false;
    public static bool IsDebugger
    {
        get
        {
            return PlayerPrefs.GetInt("FirebaseAdsJson_Debugger",0)==1; 
        }
        set
        {
             PlayerPrefs.SetInt("FirebaseAdsJson_Debugger",value?1:0);
             PlayerPrefs.Save();
             Log.Info($"FirebaseAdsJson_Debugger:{value}:重启生效");
        }
    }
    
    /// <summary>
    /// 获取对应广告list
    /// </summary>
    /// <param name="isRV"></param>
    /// <returns></returns>
    public static string[] GetAdsId(bool isRV)
    {
        if (SystemInfoManager.IsSuperLowMemorySize) return null;
        if (!IsOpenFirebaseAdsIds) return null;
        if (isRV)
        {
            if (Json!=null&&Json.Rewarded!=null&&rewardedList == null)
                rewardedList = Json.Rewarded.Select(a => a.AdsId).ToArray();
            return rewardedList;
        }
        else
        {
            if (Json!=null&&Json.Interstitial!=null&&interstitialList == null)
                interstitialList = Json.Interstitial.Select(a => a.AdsId).ToArray();
            return interstitialList;
        }
    }

    /// <summary>
    /// 根据前一个广告价格获取新的广告id
    /// </summary>
    /// <param name="adsPrice"></param>
    /// <param name="isRv"></param>
    /// <returns></returns>
    public static string GetNewAdsId(bool isRv)
    {
        if (SystemInfoManager.IsSuperLowMemorySize) return null;
        if (!IsOpenFirebaseAdsIds) return null;
        
        var list = isRv ? Json.Rewarded : Json.Interstitial;
        if (list != null)
        {
            var adsPrice = isRv ? rvAdsPrice : interAdsPrice;
            foreach (var item in list)
            {
                if (item.Range.First() <= adsPrice && item.Range.Last() > adsPrice) return item.AdsId;
            }
            return list.First().AdsId;
        }
        return null;
    }
    
    public static (string,string) GetAdsKeyValue(bool isRv)
    {
        if (SystemInfoManager.IsSuperLowMemorySize) return (null,null);
        if (!IsOpenFirebaseAdsIds) return (null,null);
        
        var list = isRv ? Json.Rewarded : Json.Interstitial;
        if (list != null)
        {
            var adsPrice = isRv ? rvAdsPrice : interAdsPrice;
            foreach (var item in list)
            {
                if (item.Range.First() <= adsPrice && item.Range.Last() > adsPrice) return (item.Key,item.Value);
            }
            return (list.First().Key,list.First().Value);
        }
        return (null,null);
    }

    public static void RecordAdsPrice(float adsPrice,bool isRV,bool isNative)
    {
        if (!isNative)
        {
            if (isRV)
            {
                CustomKVManager.RewardedAdsEcpm = adsPrice / (float)1000;
                CustomKVManager.ShowRewardedTime++;
            }
            else
            {
                CustomKVManager.InterstitialAdsEcpm = adsPrice / (float)1000;
                CustomKVManager.ShowInterstitialTime++;
            }
        }
        
        if (SystemInfoManager.IsSuperLowMemorySize) return;
        if (!IsOpenFirebaseAdsIds) return;
        
        if (isRV)
        {
            rvAdsPrice = adsPrice/1000;
            if (rvAdsPrice <= 0) rvAdsPrice = 0;
            if (rvAdsPrice >= 10000) rvAdsPrice = 9999;

            Log.Info($"RecordAdsPrice:{rvAdsPrice}");
        }
        else
        {
            interAdsPrice = adsPrice/1000;
            if (interAdsPrice <= 0) interAdsPrice = 0;
            if (interAdsPrice >= 10000) interAdsPrice = 9999;
            
            Log.Info($"RecordAdsPrice:{interAdsPrice}");

            if (IsDebugger)
            {
                int random = UnityEngine.Random.Range(0, Json.Interstitial.Count);

                var inter= Json.Interstitial[random];
                interAdsPrice = (inter.Range.First() + inter.Range.Last())/2;
                Log.Info($"debug开启 随机设置价格 RecordAdsPrice:{interAdsPrice}");
            }
        }
    }
    
    private static bool IsOpenFailAdsPriceChange =>
        GameManager.Firebase.GetBool(Constant.RemoteConfig.Is_Open_Fail_Ads_Price_Change, false);
    public static void FailRecordAdsPrice(bool isRV)
    {
        if(!IsOpenFailAdsPriceChange)return;
        
        float newPrice = isRV ? rvAdsPrice*1000 / 2 : interAdsPrice*1000 / 2;
        RecordAdsPrice(newPrice,isRV,true);
    }
}

