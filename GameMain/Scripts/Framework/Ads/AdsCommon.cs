using UnityEngine;

public enum AdsType
{
    Banner,
    Interstitial,
    Reward,
    NativeInterstitial,
    NativeReward,
}

public class AdsCommon
{

    public static int AdsInitLevel => (int)GameManager.Firebase.GetLong(Constant.RemoteConfig.Ads_Init_Level, 8);

    public static bool IsUseKVAds =>
    GameManager.Firebase.GetBool(Constant.RemoteConfig.Is_Use_KV_Ads, true);

    public static bool IsLowDeviceLoadBanner =>
        GameManager.Firebase.GetBool(Constant.RemoteConfig.If_Low_Device_Load_Banner, true);

    public static bool IsLowPerformanceMachine => SystemInfoManager.IsLowPerformanceMachine();

    public static long InterstitialLoadLevel => InterstitialNeedLevel - 2;

    public static long BannerNeedLevel =>
        GameManager.Firebase.GetLong(Constant.RemoteConfig.Show_Banner_Start_Level,10);
    public static long InterstitialNeedLevel =>
        GameManager.Firebase.GetLong(Constant.RemoteConfig.Show_Interstitial_Start_Level,10);

    public static bool IsOpenUseGprp => 
        GameManager.Firebase.GetBool(Constant.RemoteConfig.Enable_GPRP,false)||IsCanLocalUseGprp;

    public static bool IsAdsUseAdmob=>
        GameManager.Firebase.GetBool(Constant.RemoteConfig.If_Use_AdmobSDK,true)&&!IsCanLocalUseYandex;
    
        
    public static string RemoteNativeInterstitialAdId=>GameManager.Firebase.GetString(Constant.RemoteConfig.Remote_Native_Interstitial_Ads_Id,null);
    public static string RemoteNativeRewardAdId=>GameManager.Firebase.GetString(Constant.RemoteConfig.Remote_Native_Reward_Ads_Id,null);
    
    public static bool IsOpenNativeInterstitialAds=>GameManager.Firebase.GetBool(Constant.RemoteConfig.Is_Open_Native_Interstitial_Ads,true)||IsForceOpenNativeInterstitial;
        
    public static bool IsOpenNativeRewardAds=>GameManager.Firebase.GetBool(Constant.RemoteConfig.Is_Open_Native_Reward_Ads,true)||IsForceOpenNativeReward;

    public static bool IsInterstitialChangeNative =>
        IsForceOpenNativeChangeInterstitial ||
        (SystemInfoManager.DeviceType<=DeviceType.Normal &&
        GameManager.Firebase.GetBool(Constant.RemoteConfig.Is_Interstitial_Change_To_Native, false));
    
    public static bool IsRewardChangeNative =>
        IsForceOpenNativeChangeReward||
        (SystemInfoManager.DeviceType<=DeviceType.Normal &&
        GameManager.Firebase.GetBool(Constant.RemoteConfig.Is_Interstitial_Change_To_Native, false));
    
    public static bool IsCanLocalUseYandex
    {
        get=>PlayerPrefs.GetInt("IsCanLocalUseYandex", 0)==1;
        set
        {
            PlayerPrefs.SetInt("IsCanLocalUseYandex", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    
    public static bool IsCanLocalUseGprp
    {
        get=>PlayerPrefs.GetInt("IsCanLocalUseGPRP", 0)==1;
        set
        {
            PlayerPrefs.SetInt("IsCanLocalUseGPRP", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    
     public static bool IsForceOpenNativeInterstitial
        {
            get => PlayerPrefs.GetInt("IsForceOpenNativeInterstitial",0)==1;
            set
            {
                PlayerPrefs.SetInt("IsForceOpenNativeInterstitial",value?1:0);
                PlayerPrefs.Save();
            }
        }
        
        public static bool IsForceOpenNativeReward
        {
            get => PlayerPrefs.GetInt("IsForceOpenNativeReward",0)==1;
            set
            {
                PlayerPrefs.SetInt("IsForceOpenNativeReward",value?1:0);
                PlayerPrefs.Save();
            }
        }
        
        public static bool IsForceOpenNativeChangeInterstitial
        {
            get => PlayerPrefs.GetInt("IsForceOpenNativeChangeInterstitial",0)==1;
            set
            {
                PlayerPrefs.SetInt("IsForceOpenNativeChangeInterstitial",value?1:0);
                PlayerPrefs.Save();
            }
        }
        public static bool IsForceOpenNativeChangeReward
        {
            get => PlayerPrefs.GetInt("IsForceOpenNativeChangeReward",0)==1;
            set
            {
                PlayerPrefs.SetInt("IsForceOpenNativeChangeReward",value?1:0);
                PlayerPrefs.Save();
            }
        }
        
}


