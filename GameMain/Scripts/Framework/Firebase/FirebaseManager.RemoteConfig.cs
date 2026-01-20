using Firebase.Extensions;
using Firebase.RemoteConfig;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed partial class FirebaseManager : GameFrameworkModule, IFirebaseManager
{
    public void InitRemoteConfig()
    {
        if (isInitRemoteConfig)
        {
            Log.Warning("RemoteConfig is already init");
            return;
        }

        Dictionary<string, object> defaults = new Dictionary<string, object>
        {
            // These are the values that are used if we haven't fetched data from the server
            // yet, or if we ask for values that the server doesn't have:
            { Constant.RemoteConfig.Show_Banner_Start_Level, 10L },
            { Constant.RemoteConfig.Show_Interstitial_Start_Level, 10L },
            { Constant.RemoteConfig.If_Use_AdmobSDK, true },
            { Constant.RemoteConfig.ShowRateLevel, 10L },
            { Constant.RemoteConfig.Ads_Timeout, 60L },
            { Constant.RemoteConfig.Use_Max_Load_Fail_Count, 0 },
            { Constant.RemoteConfig.Round_Delay_Load_Ads_Time, 20 },
            { Constant.RemoteConfig.LoadAds_DelayTime, 5L },
            { Constant.RemoteConfig.Refresh_Banner_FrameCount, 1L },

            { Constant.RemoteConfig.Yandex_Banner_Refresh_Time, 30L },

            {Constant.RemoteConfig.Is_Force_Open_Rate_URL, true},

            {Constant.RemoteConfig.Use_Level_Type_Index,0},
            {Constant.RemoteConfig.Can_Open_Rate_MaxCount_ByVersion, 3 },
            {Constant.RemoteConfig.If_Open_Rate_ByLevel, true},
            {Constant.RemoteConfig.If_Open_Rate_ByUnlockBG, true },
            {Constant.RemoteConfig.If_Open_Rate_ByFiveWin, true },

            {Constant.RemoteConfig.If_Use_Record_Level_Data, false },
            {Constant.RemoteConfig.SpinAdTimesLimit, Constant.GameConfig.AdsSpinLimitTime },

            {Constant.RemoteConfig.If_Only_Once_Show_Ads_Continue_Btn, true },
            {Constant.RemoteConfig.If_Low_Device_Load_Banner, true },

            {Constant.RemoteConfig.Enable_GPRP,true},

            {Constant.RemoteConfig.If_Use_Tile_Furniture, false},
            {Constant.RemoteConfig.If_Show_Rate_Us_Menu, true},

            { Constant.RemoteConfig.TaiChi_Top50Percent, 10d },
            { Constant.RemoteConfig.TaiChi_Top40Percent, 10d },
            { Constant.RemoteConfig.TaiChi_Top30Percent, 10d },
            { Constant.RemoteConfig.TaiChi_Top20Percent, 10d },
            { Constant.RemoteConfig.TaiChi_Top10Percent, 10d },

            { Constant.RemoteConfig.Level_RV_CD, 0 },
            { Constant.RemoteConfig.Is_Show_Level_PropAD, true },

            { Constant.RemoteConfig.Level_Win_Rewards_RV_Multiple, false },
            { Constant.RemoteConfig.RV_Times_Total, 99 },
            { Constant.RemoteConfig.RV_Times_Props, 99 },
            { Constant.RemoteConfig.RV_Times_Continue, 99 },

            { Constant.RemoteConfig.Is_Open_Native_Interstitial_Ads, true},
            { Constant.RemoteConfig.Is_Open_Native_Reward_Ads, true},

            { Constant.RemoteConfig.Is_Interstitial_Change_To_Native, false},
            { Constant.RemoteConfig.Is_Reward_Change_To_Native, false},

            { Constant.RemoteConfig.Win_Direct_Next_End_Level, 5 },

            {Constant.RemoteConfig.Pk_Game_Each_Time_Lenght, 15},

            {Constant.RemoteConfig.Is_Can_Open_Pk_Game, true},

            {Constant.RemoteConfig.Is_Turn_Off_Hint, false},

            {Constant.RemoteConfig.Bg_Music_Volume, 30},

            {Constant.RemoteConfig.Enable_Offline_Restrictions, true},

            {Constant.RemoteConfig.Event_Merge_UnlockLevel, 21},

            {Constant.RemoteConfig.If_Use_B_Level_Table_Data, false},

            {Constant.RemoteConfig.Ads_Init_Level, 8},

            {Constant.RemoteConfig.Is_Use_KV_Ads, true },

            {Constant.RemoteConfig.Is_Use_Special_Device_Strage,true },

            {Constant.RemoteConfig.Is_Use_Special_Device_Strage_Use_Low_Interstitial,false },

            {Constant.RemoteConfig.Is_Use_Special_Device_Strage_Use_Low_RV,false },

            {Constant.RemoteConfig.Is_Use_Special_Device_Strage_Close_Low_Priority_Pop,true },

            {Constant.RemoteConfig.Is_Use_Special_Device_Strage_Turn_Off_Particle_Effect,true },

            {Constant.RemoteConfig.Is_Use_Special_Device_Strage_Optimize_Heavy_Work,true },
            
            {Constant.RemoteConfig.Is_Use_Special_Device_Strage_Use_Native_Ads,false},
            
            {Constant.RemoteConfig.Winpanel_Chest_IncreaseRewards, false},
            
            {Constant.RemoteConfig.If_Have_Thumb, true},
            
            {Constant.RemoteConfig.If_Use_Level_Start_FirstTry,false},
            
            {Constant.RemoteConfig.Is_Use_KVGroup,false},
            {Constant.RemoteConfig.Remote_Int_KVGroup_Json,string.Empty},
            {Constant.RemoteConfig.Remote_RV_KVGroup_Json,string.Empty},
            {Constant.RemoteConfig.Remote_KVGroup_DecayX,0d},
            {Constant.RemoteConfig.Remote_KVGroup_DecayY,0d},
            
            {Constant.RemoteConfig.ItemFunction_Change_Scale,false},
            
            {Constant.RemoteConfig.BG_Change_1to5,true},
        };

        FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults).ContinueWithOnMainThread(task =>
        {
            if (!task.IsCompleted)
            {
                Log.Warning("RemoteConfig defaults Init Fail.{0}", task.Exception.ToString());
            }
            else
            {
                Log.Info("RemoteConfig defaults Init Success.");
            }
            isInitRemoteConfig = true;

            FetchRemoteConfigDataAsync();
        });
    }

    public void FetchRemoteConfigDataAsync()
    {
        if (isFetchRemoteConfig)
        {
            return;
        }

        Log.Info("Remote Config Fetching data...");
        FirebaseRemoteConfig.DefaultInstance.FetchAndActivateAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Log.Info("RemoteConfig Fetch canceled.");
            }
            else if (task.IsFaulted)
            {
                Log.Info("RemoteConfig Fetch encountered an error.");
            }
            else if (task.IsCompleted)
            {
                isFetchRemoteConfig = true;
                Log.Info("RemoteConfig Fetch completed successfully!");
            }
            isFetchRemoteConfig = true;
        });
    }

    Dictionary<string, bool> boolDic = new Dictionary<string, bool>();

    public bool GetBool(string key, bool defaultValue = false)
    {
        if (!isFetchRemoteConfig)
        {
            return defaultValue;
        }

        try
        {
            if (boolDic.TryGetValue(key, out bool res)) 
            {
                return res;
            }
            
            var configValue=FirebaseRemoteConfig.DefaultInstance.GetValue(key);
            if (configValue.Source != ValueSource.StaticValue)
            {
                boolDic.Add(key, configValue.BooleanValue);
                return configValue.BooleanValue;
            }
            else
            {
                boolDic.Add(key, defaultValue);
                return defaultValue;
            }
        }
        catch (Exception e)
        {
            Log.Warning("Firebase RemoteConfig GetBool {0} fail.Exception:{1}", key, e.Message);
            return defaultValue;
        }
    }

    Dictionary<string, long> longDic = new Dictionary<string, long>();
    public long GetLong(string key, long defaultValue = 0)
    {
        if (!isFetchRemoteConfig)
        {
            return defaultValue;
        }

        try
        {
            if (longDic.TryGetValue(key, out long res)) 
            {
                return res;
            }
            
            var configValue=FirebaseRemoteConfig.DefaultInstance.GetValue(key);
            if (configValue.Source != ValueSource.StaticValue)
            {
                longDic.Add(key, configValue.LongValue);
                return configValue.LongValue;
            }
            else
            {
                longDic.Add(key, defaultValue);
                return defaultValue;
            }
        }
        catch (Exception e)
        {
            Log.Warning("Firebase RemoteConfig GetLong {0} fail.Exception:{1}", key, e.Message);
            return defaultValue;
        }
    }

    Dictionary<string, double> doubleDic = new Dictionary<string, double>();
    public double GetDouble(string key, double defaultValue = 0)
    {
        if (!isFetchRemoteConfig)
        {
            return defaultValue;
        }

        try
        {
            if (doubleDic.TryGetValue(key, out double res))
            {
                return res;
            }
            var configValue=FirebaseRemoteConfig.DefaultInstance.GetValue(key);
            if (configValue.Source != ValueSource.StaticValue)
            {
                doubleDic.Add(key, configValue.DoubleValue);
                return configValue.DoubleValue;
            }
            else
            {
                doubleDic.Add(key, defaultValue);
                return defaultValue;
            }
        }
        catch (Exception e)
        {
            Log.Warning("Firebase RemoteConfig GetDouble {0} fail.Exception:{1}", key, e.Message);
            return defaultValue;
        }
    }

    Dictionary<string, string> stringDic = new Dictionary<string, string>();
    public string GetString(string key, string defaultValue = null)
    {
        if (!isFetchRemoteConfig)
        {
            return defaultValue;
        }

        try
        {
            if (stringDic.TryGetValue(key, out string res))
            {
                return res;
            }
            
            var configValue=FirebaseRemoteConfig.DefaultInstance.GetValue(key);
            if (configValue.Source != ValueSource.StaticValue)
            {
                stringDic.Add(key, configValue.StringValue);
                return configValue.StringValue;
            }
            else
            {
                stringDic.Add(key, defaultValue);
                return defaultValue;
            }
        }
        catch (Exception e)
        {
            Log.Warning("Firebase RemoteConfig GetString {0} fail.Exception:{1}", key, e.Message);
            return defaultValue;
        }
    }
}
