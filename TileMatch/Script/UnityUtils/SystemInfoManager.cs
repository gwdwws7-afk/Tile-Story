using UnityEngine;

public enum DeviceType
{
    None,
    SurpLow,
    Low,
    Normal,
    High,
}
public static class SystemInfoManager
{
    public static bool IsGraphicsMultiThreaded()
    {
        return SystemInfo.graphicsMultiThreaded;
    }

    public static bool IsLowPerformanceMachine()
    {
        return DeviceType<=DeviceType.Low;
    }

    static DeviceType deviceType = DeviceType.None;
    public static DeviceType DeviceType
    {
        get
        {
            if (deviceType == DeviceType.None)
            {
                deviceType = GetDeviceType();
            }
            return deviceType;
        }
    }

    public static bool IsSuperLowMemorySize
    {
        get
        {
            return DeviceType <= DeviceType.SurpLow || (DeviceType <= DeviceType.Low && IsLowMemory);
        }
    }
    
    public static bool IsLowMemorySize => DeviceType <= DeviceType.Low;

    public static bool IsLowMemory;

    private static DeviceType GetDeviceType()
    {
#if UNITY_EDITOR||UNITY_STANDALONE_WIN
        return DeviceType.High;
#endif
        if (SystemInfo.systemMemorySize <= 1024 || SystemInfo.graphicsMemorySize <= 200)
        {
            return DeviceType.SurpLow;
        }
        else if (SystemInfo.systemMemorySize <= 3072 || SystemInfo.graphicsMemorySize <= 512) 
        {
            return DeviceType.Low;
        }
#if UNITY_IOS
        else
        {
            return DeviceType.High;
        }
#elif UNITY_ANDROID
        else if (SystemInfo.systemMemorySize > 4096)
        {
            return DeviceType.High;
        }
        else
        {
            return DeviceType.Normal;
        }
#endif
    }
    
    public static int GetSystemMemory()
    {
        try
        {
            return SystemInfo.systemMemorySize / 1000;
        }
        catch
        {
            return 0;
        }
    }

    private static int IsSpecialDevice;

    public static bool CheckIsSpecialDevice()
    {
        if (IsSpecialDevice != 0)
            return IsSpecialDevice == 1 ? true : false;
        
        if (!GameManager.Firebase.GetBool(Constant.RemoteConfig.Is_Use_Special_Device_Strage, true))
            return false;
        
        string deviceModel = SystemInfo.deviceModel.ToLower();
        bool isSpecial = false;
        
        if (!isSpecial)
            isSpecial = DeviceType == DeviceType.SurpLow;
        
        //内存小于5G的motorola ellis (moto g pure)，motorola sabahl (moto e13)，motorola malta (moto e(7))
        if (!isSpecial)
            isSpecial = SystemInfo.systemMemorySize < 5120 && (deviceModel.Contains("moto g pure") ||
                                                               deviceModel.Contains("moto e13") ||
                                                               deviceModel.Contains("moto e(7)") ||
                                                               deviceModel.Contains("moto e7"));
        
        //内存小于4G的Redmi 9A
        if (!isSpecial) isSpecial = SystemInfo.systemMemorySize < 4096 && (deviceModel.Contains("m2006c3lg") || deviceModel.Contains("m2006c3li") || deviceModel.Contains("m2006c3lc") || deviceModel.Contains("m2004c3l"));
        
        //内存小于4G的Redmi A2
        if (!isSpecial) isSpecial = SystemInfo.systemMemorySize < 4096 && (deviceModel.Contains("redmi a2"));
        
        //内存小于4G的Oppo A17k
        if (!isSpecial) isSpecial = SystemInfo.systemMemorySize < 4096 && (deviceModel.Contains("a17k") || deviceModel.Contains("cph2471"));
        
        //内存小于3G的TECNO BG6m
        if (!isSpecial) isSpecial = SystemInfo.systemMemorySize < 3072 && deviceModel.Contains("tecno bg6m");
        
        //内存小于4G的vivo 1906和vivo 1904
        if (!isSpecial) isSpecial = SystemInfo.systemMemorySize < 4096 && (deviceModel.Contains("vivo 1906") || deviceModel.Contains("vivo 1904")||deviceModel.Contains("vivo 1914"));
        
        //内存小于5G的HONOR X5b
        if (!isSpecial) isSpecial = SystemInfo.systemMemorySize < 5120 && (deviceModel.Contains("honor x5b"));
        
        IsSpecialDevice = isSpecial ? 1 : 2;
        
        return IsSpecialDevice == 1;
    }

    public static bool CheckIsSpecialDeviceTurnOffParticleEffect()
    {
        if (CheckIsSpecialDevice())
        {
            return GameManager.Firebase.GetBool(Constant.RemoteConfig.Is_Use_Special_Device_Strage_Turn_Off_Particle_Effect, true);
        }

        return false;
    }

    public static bool CheckIsSpecialDeviceCloseLowPriorityPop()
    {
        if (CheckIsSpecialDevice())
        {
            return GameManager.Firebase.GetBool(Constant.RemoteConfig.Is_Use_Special_Device_Strage_Close_Low_Priority_Pop, true);
        }

        return false;
    }

    public static bool CheckIsSpecialDeviceOptimizeHeavyWork()
    {
        if (CheckIsSpecialDevice())
        {
            return GameManager.Firebase.GetBool(Constant.RemoteConfig.Is_Use_Special_Device_Strage_Optimize_Heavy_Work, true);
        }

        return false;
    }

    public static bool CheckIsSpecialDeviceUseNativeAds()
    {
        if (CheckIsSpecialDevice())
        {
            return GameManager.Firebase.GetBool(Constant.RemoteConfig.Is_Use_Special_Device_Strage_Use_Native_Ads, false);
        }

        return false;
    }
}
