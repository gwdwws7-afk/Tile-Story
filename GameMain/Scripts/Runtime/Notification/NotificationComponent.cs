using System;
using UnityEngine;

/// <summary>
/// 通知组件
/// </summary>
public sealed class NotificationComponent : GameFrameworkComponent
{
    private bool m_IsInit = false;

    public bool IsInit => m_IsInit;
    
    private LocalNotificationManager m_LocalNotificationManager;
    public void Init()
    {
        if(m_IsInit)return;

        if (!GameManager.PlayerData.NotificationForbidden && !CheckForbiddenDevice()) 
        {
            m_IsInit = true;
            if (m_LocalNotificationManager == null) m_LocalNotificationManager = new LocalNotificationManager();
            m_LocalNotificationManager.Init();
        }
    }

    public void Shutdown()
    {
        if(!m_IsInit)return;
        
        if (m_LocalNotificationManager != null)m_LocalNotificationManager.CancalAllNotifications();
    }
    
    /// <summary>
    /// notification
    /// </summary>
    /// <param name="type"></param>
    /// <param name="deliveryTime"></param>
    public void SendNotification(NotificationKey type,DateTime deliveryTime)
    {
        if (!m_IsInit) return;
        if (deliveryTime<=DateTime.Now)return;
        
        string titleKey=null;
        string bodyKey=null;
        switch (type)
        {
            case NotificationKey.Merge_1:
                titleKey = "Notification.Merge_1_Title";
                bodyKey = "Notification.Merge_1_Body";
                break;
            case NotificationKey.Merge_2:
                titleKey = "Notification.Merge_2_Title";
                bodyKey = "Notification.Merge_2_Body";
                break;
            case NotificationKey.Merge_3:
                titleKey = "Notification.Merge_3_Title";
                bodyKey = "Notification.Merge_3_Body";
                break;
            case NotificationKey.Christmas_1:
                titleKey = "Notification.Christmas_1_Title";
                bodyKey = "Notification.Christmas_1_Body";
                break;
        }

        string title = GameManager.Localization.GetString(titleKey);
        if(title == titleKey)
            title = GameManager.Localization.GetString("Notification.GeneralTitle");
        string body = GameManager.Localization.GetString(bodyKey);

        Log.Info($"{type.ToString()}::{titleKey.ToString()}::{body}");
        Gley.Notifications.API.SendNotification(title,body, deliveryTime-DateTime.Now,  "normal_small", "normal_big",customData:type.ToString());
    }

    //屏蔽崩溃和anr高的机型
    private bool CheckForbiddenDevice()
    {
        string deviceModule = SystemInfo.deviceModel;
        if (deviceModule.Contains("CPH2269") || deviceModule.Contains("Redmi dandelion") || deviceModule.Contains("Redmi 10A")
            ||deviceModule.Contains("motorola")||deviceModule.Contains("Nokia EAG")|| deviceModule.Contains("Moto G Pure"))
        {
            return true;
        }

        if (SystemInfoManager.CheckIsSpecialDeviceOptimizeHeavyWork())
            return true;

        return false;
    }

    public void OnApplicationFocus(bool hasFocus)
    {
        if(!m_IsInit)return;
        
        if(m_LocalNotificationManager!=null)m_LocalNotificationManager.OnApplicationFocus(hasFocus);
    }
}
