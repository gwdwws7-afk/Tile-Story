using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MySelf.Model;
using UnityEngine;

public class ATTAuth
{
#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void _RequestTrackingAuthorizationWithCompletionHandler();

    [DllImport("__Internal")]
    private static extern int _GetAppTrackingAuthorizationStatus();
#endif

    private static Action<int> getAuthorizationStatusAction;

    public const string GetTrackingAuthorizationKey = "CanGetTrackingAuthorization";

    /// <summary>
    /// 是否可以弹出获取权限弹窗
    /// </summary>
    public static bool CanGetTrackingAuthorization
    {
        get
        {
            //立刻弹权限弹窗
            if (PlayerPrefs.GetInt(GetTrackingAuthorizationKey, 0) <= 3)
            {
                PlayerPrefs.SetInt(GetTrackingAuthorizationKey, 4);
                PlayerPrefs.Save();
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// 请求ATT授权窗口
    /// </summary>
    /// <param name="getResult"></param>
    public static void RequestTrackingAuthorizationWithCompletionHandler()
    {
        //0: "ATT 授权状态待定";
        //1: "ATT 授权状态受限";
        //2: "ATT 已拒绝";
        //3: "ATT 已授权";
#if UNITY_IOS
        _RequestTrackingAuthorizationWithCompletionHandler();
#endif
    }

    /// <summary>
    /// 获取当前ATT授权状态
    /// </summary>
    /// <returns></returns>
    public static int GetAppTrackingAuthorizationStatus()
    {
#if UNITY_IOS
        return _GetAppTrackingAuthorizationStatus();
#else 
        return -1;
#endif
    }
}
