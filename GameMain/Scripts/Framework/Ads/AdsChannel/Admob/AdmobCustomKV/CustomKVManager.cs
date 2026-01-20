using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class KVGroupInfo
{
    public int Index;
    public string Key;
    public string Value;
    public string AdsId;

    public KVGroupInfo(int index, string key, string value, string adsId)
    {
        Index = index;
        Key = key;
        Value = value;
        AdsId = adsId;
    }
}

public static class CustomKVManager
{
    private class CustomKVGroup
    {
        public float TargetEcpm;
        public string Key;
        public string Value;
        public string AdsId;
    }
    
    private static List<CustomKVGroup> interstitialAds_KVGroups = null;
    private static List<CustomKVGroup> rewardedAds_KVGroups = null;

    private static List<CustomKVGroup> InterstitialAds_KVGroups
    {
        get
        {
            if (interstitialAds_KVGroups == null)
            {
                interstitialAds_KVGroups = new List<CustomKVGroup>();
                
                InitIntTargetEcpmDict();

                foreach (var pair in IntTargetEcpmDict)
                {
                    interstitialAds_KVGroups.Add(new CustomKVGroup()
                    {
                        TargetEcpm=pair.Value,
                        Key = "userGroup",
                        Value = pair.Key,
                        AdsId = "ca-app-pub-8209077259415431/8263011704"
                    });  
                }
                
                interstitialAds_KVGroups.Sort((a, b) => a.TargetEcpm > b.TargetEcpm ? -1 : 1);
            }

            return interstitialAds_KVGroups;
        }
    }

    private static List<CustomKVGroup> RewardedAds_KVGroups
    {
        get
        {
            if (rewardedAds_KVGroups == null)
            {
                rewardedAds_KVGroups = new List<CustomKVGroup>();

                InitRVTargetEcpmDict();

                foreach (var pair in RVTargetEcpmDict)
                {
                    rewardedAds_KVGroups.Add(new CustomKVGroup()
                    {
                        TargetEcpm=pair.Value,
                        Key = "userGroup",
                        Value = pair.Key,
                        AdsId = "ca-app-pub-8209077259415431/9332442995"
                    });  
                }

                rewardedAds_KVGroups.Sort((a, b) => a.TargetEcpm > b.TargetEcpm ? -1 : 1);
            }

            return rewardedAds_KVGroups;
        }
    }

    public static float InterstitialAdsEcpm = -1;//上一次的插屏的ecpm
    public static float RewardedAdsEcpm = -1;//上一次的RV的ecpm
    public static int ShowInterstitialTime = 0;//插屏展示次数
    public static int ShowRewardedTime = 0;//RV展示次数

    public static KVGroupInfo GetCustomKVGroup(bool isRV, int indexOffset)
    {
        if (isRV)
        {
            float RvEcpm = RewardedAdsEcpm * (1 - RewardedAdsEcpmDecay(ShowRewardedTime));
            Debug.Log($"[KV]GetCustomKVGroup RV: ShowTime:{ShowRewardedTime} SavedEcpm:{RewardedAdsEcpm} CalculatedEcpm:{RvEcpm} Offset:{indexOffset}");
            return GetCustomKVGroup(RvEcpm, RewardedAds_KVGroups, indexOffset);
        }
        else
        {
            float IntEcpm = InterstitialAdsEcpm * (1 - InterstitialEcpmDecay(ShowInterstitialTime));
            Debug.Log($"[KV]GetCustomKVGroup Int: ShowTime:{ShowInterstitialTime} SavedEcpm:{InterstitialAdsEcpm} CalculatedEcpm:{IntEcpm} Offset:{indexOffset}");
            return GetCustomKVGroup(IntEcpm, InterstitialAds_KVGroups, indexOffset);
        }
    }

    private static KVGroupInfo GetCustomKVGroup(float ecpm, List<CustomKVGroup> kvGroups, int indexOffset)
    {
        if (kvGroups != null)
        {
            int index = kvGroups.Count - 1;
            for (int i = 0; i < kvGroups.Count; i++)
            {
                if (ecpm > kvGroups[i].TargetEcpm)
                {
                    index = i - 1;
                    break;
                }
            }

            index = index < 0 ? 0 : index;
            index = Mathf.Min(index + indexOffset, kvGroups.Count - 1);
            return new KVGroupInfo(index, kvGroups[index].Key, kvGroups[index].Value, kvGroups[index].AdsId); 
        }

        return null;
    }
    
    /// <summary>
    /// 奖励广告ecpm衰减（rn = 1: 0.236811 rn = 2: 0.133477 rn = 3: 0.095928）
    /// </summary>
    private static float RewardedAdsEcpmDecay(float rn)
    {
        if (rn <= 0)
            return 0f;

        float remoteX = (float)GameManager.Firebase.GetDouble(Constant.RemoteConfig.Remote_KVGroup_DecayX, 0d);
        float remoteY = (float)GameManager.Firebase.GetDouble(Constant.RemoteConfig.Remote_KVGroup_DecayY, 0d);
        if (remoteX != 0 && remoteY != 0)
            return remoteX * Mathf.Pow(rn, remoteY);
        
        return 0.236811260098781f * Mathf.Pow(rn, -0.8286233054921487f);
    }
    
    /// <summary>
    /// 插屏ecpm衰减
    /// </summary>
    private static float InterstitialEcpmDecay(float rn)
    {
        if (rn <= 0)
            return 0f;

        float remoteX = (float)GameManager.Firebase.GetDouble(Constant.RemoteConfig.Remote_KVGroup_DecayX, 0d);
        float remoteY = (float)GameManager.Firebase.GetDouble(Constant.RemoteConfig.Remote_KVGroup_DecayY, 0d);
        if (remoteX != 0 && remoteY != 0)
            return remoteX * Mathf.Pow(rn, remoteY);
        
        return 0.236811260098781f * Mathf.Pow(rn, -0.8286233054921487f);
    }

    private static Dictionary<string, float> IntTargetEcpmDict;
    private static Dictionary<string, float> RVTargetEcpmDict;

    private static void InitIntTargetEcpmDict()
    {
        IntTargetEcpmDict = new Dictionary<string, float>();
        string intJson = @"
{
    ""INTGroup1"": ""150"",
    ""INTGroup2"": ""100"",
    ""INTGroup3"": ""75"",
    ""INTGroup4"": ""55"",
    ""INTGroup5"": ""40"",
    ""INTGroup6"": ""30"",
    ""INTGroup7"": ""20"",
    ""INTGroup8"": ""10"",
    ""INTGroup9"": ""7"",
    ""INTGroup10"": ""4"",
    ""INTGroup_1"": ""1"",
    ""INTGroup_0_9"": ""0.9"",
    ""INTGroup_0_8"": ""0.8"",
    ""INTGroup_0_7"": ""0.7"",
    ""INTGroup_0_6"": ""0.6"",
    ""INTGroup_0_5"": ""0.5"",
    ""INTGroup_0_4"": ""0.4"",
    ""INTGroup_0_3"": ""0.3"",
    ""INTGroup_0_2"": ""0.2""
}";
        string remoteJson = GameManager.Firebase.GetString(Constant.RemoteConfig.Remote_Int_KVGroup_Json, string.Empty);
        if (!string.IsNullOrEmpty(remoteJson))
            intJson = remoteJson;
        
        Dictionary<string, string> ecpmMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(intJson);
        foreach (var pair in ecpmMap)
        {
            if (float.TryParse(pair.Value, out float targetEcpm))
            {
                IntTargetEcpmDict.Add(pair.Key, targetEcpm);
                Debug.Log($"[KV]IntEcpmRangeDict Add Success:{pair.Key}[{targetEcpm}]");   
            }
            else
            {
                Debug.LogError($"[KV]IntEcpmRangeDict Add Fail:{pair.Key}[{pair.Value}]");   
            }
        }
    }

    private static void InitRVTargetEcpmDict()
    {
        RVTargetEcpmDict = new Dictionary<string, float>();
        string rvJson = @"
{
    ""RVGroup1"": ""150"",
    ""RVGroup2"": ""100"",
    ""RVGroup3"": ""75"",
    ""RVGroup4"": ""55"",
    ""RVGroup5"": ""40"",
    ""RVGroup6"": ""30"",
    ""RVGroup7"": ""20"",
    ""RVGroup8"": ""10"",
    ""RVGroup9"": ""7"",
    ""RVGroup10"": ""4"",
    ""RVGroup_1"": ""1"",
    ""RVGroup_0_9"": ""0.9"",
    ""RVGroup_0_8"": ""0.8"",
    ""RVGroup_0_7"": ""0.7"",
    ""RVGroup_0_6"": ""0.6"",
    ""RVGroup_0_5"": ""0.5"",
    ""RVGroup_0_4"": ""0.4"",
    ""RVGroup_0_3"": ""0.3"",
    ""RVGroup_0_2"": ""0.2""
}";
        string remoteJson = GameManager.Firebase.GetString(Constant.RemoteConfig.Remote_RV_KVGroup_Json, string.Empty);
        if (!string.IsNullOrEmpty(remoteJson))
            rvJson = remoteJson;
        
        Dictionary<string, string> ecpmMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(rvJson);
        foreach (var pair in ecpmMap)
        {
            if (float.TryParse(pair.Value, out float targetEcpm))
            {
                RVTargetEcpmDict.Add(pair.Key, targetEcpm);
                Debug.Log($"[KV]RVEcpmRangeDict Add Success:{pair.Key}[{targetEcpm}]");   
            }
            else
            {
                Debug.LogError($"[KV]RVEcpmRangeDict Add Fail:{pair.Key}[{pair.Value}]");   
            }
        }
    }
}
