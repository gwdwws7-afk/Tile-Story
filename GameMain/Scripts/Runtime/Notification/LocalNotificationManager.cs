using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Merge;
using MySelf.Model;

public enum NotificationType
{
    Test,
    EightMorning,//早上8点
    TwelveMoon,//中午12点
    SevenEvening,//晚上17点
}

public class LocalNotificationManager
{
    private bool isInit = false;

    private int curNotificationNum = 0;

    // Start is called before the first frame update
    public void Init()
    {
        if (isInit) return;
        isInit = true;
        
        //初始化
        Gley.Notifications.API.CancelAllNotifications();
        Gley.Notifications.API.Initialize();
    }

	public void OnApplicationFocus(bool focus)
    {
        if(!isInit)return;
        
        //if (GameManager.Purchase != null && GameManager.Purchase.IsPurchasing)
        //{
        //    return;
        //}
        
        Action action = () =>
        {
            try
            {
                if (focus)
                {
                    CancalAllNotifications();
                }
                else
                {
                    CancalAllNotifications();
                    if (GameManager.PlayerData.NotificationForbidden)
                        return;
                    //进入后台
                    SendNotifications();
                }
            }
            catch (Exception e)
            {
                Log.Error($"OnApplicationFocus Notification:{e.Message}");
            }
        };
        action?.InvokeSafely();
    }

    public void CancalAllNotifications()
    {
        if(!isInit)return;

        curNotificationNum = 0;
        Gley.Notifications.API.CancelAllNotifications();
        Log.Info($"CancalAllNotifications");
    }

    private void SendNotifications()
    {
        if(!isInit)return;
        if(curNotificationNum>0)return;
        try
        {
            DTNotificationData notificationDataTable = GameManager.DataTable.GetDataTable<DTNotificationData>().Data;
            if (notificationDataTable == null)return;

            List<DateTime> dateTimeList = new List<DateTime>();
            for (int j = 0; j < 7; j++)
            {
                DateTime scheduleTime = DateTime.Now.AddDays(j);
                List<NotificationData> notificationData = notificationDataTable.GetNotificationData(scheduleTime.DayOfWeek);
                for (int i = 0; i < notificationData.Count; i++)
                {
                    DateTime deliverTime = new DateTime(scheduleTime.Year, scheduleTime.Month, scheduleTime.Day, notificationData[i].Hours, notificationData[i].Minutes, 0, DateTimeKind.Local);
                    if(dateTimeList.Contains(deliverTime))continue;
                    if (deliverTime <= DateTime.Now) continue;
                    dateTimeList.Add(deliverTime);
                    SendNotification(notificationData[i].Key, deliverTime);
                }
            }
            NotificationModel.Instance.SaveToLocal();
        }
        catch (Exception e)
        {
            Log.Error($"SendNotifications:{e.Message}");
        }
        
        
        //merge notification
        MergeManager.Instance.SendNotificationByMerge();

    }
    
    public void SendNotification(NotificationKey notificationKey, DateTime deliveryTime)
    {
        //常规唤回，随机一个Term，但当天不重复
        if (notificationKey == NotificationKey.TileMatch_Normal)
        {
            int lastRandomNum = GameManager.PlayerData.LastNotificationRandomNum;
            int randomNum = UnityEngine.Random.Range((int)NotificationKey.Normal_RandomTerm_1, (int)NotificationKey.Normal_RandomTerm_12);
            while (lastRandomNum == randomNum)
            {
                randomNum = UnityEngine.Random.Range((int)NotificationKey.Normal_RandomTerm_1, (int)NotificationKey.Normal_RandomTerm_12);
            }
            GameManager.PlayerData.LastNotificationRandomNum = randomNum;
            notificationKey = (NotificationKey)randomNum;
        }
        else if (notificationKey == NotificationKey.TileMatch_TurnTable)
        {
            if (GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockNormalTurntableLevel) return;
               
            int randomNum = UnityEngine.Random.Range((int)NotificationKey.TurnTable_RandomTerm_1, (int)NotificationKey.TurnTable_RandomTerm_2);
            notificationKey = (NotificationKey)randomNum;
        }
        else if (notificationKey == NotificationKey.TileMatch_PersonRank_Finished)
        {
            if (GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockPersonRankGameLevel) return;
        }
        else if (notificationKey == NotificationKey.TileMatch_PersonRank_StartOrOngoing)
        {
            if (GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockPersonRankGameLevel)return;
                
            if (GameManager.Task.PersonRankManager.TaskState == PersonRankState.Playing)
                notificationKey = NotificationKey.PersonRank_OnGoing;
            else
                notificationKey = NotificationKey.PersonRank_Start;
        }

        string key = notificationKey.ToString();
        string titleKey = $"Notification.{key}_Title";
        string bodyKey = $"Notification.{key}_Body";

        string title = GameManager.Localization.GetString(titleKey);
        if(title == titleKey)
             title = GameManager.Localization.GetString("Notification.GeneralTitle");
        string body = GameManager.Localization.GetString(bodyKey);

        curNotificationNum++;
        Gley.Notifications.API.SendNotification(title,body, deliveryTime-DateTime.Now,  "normal_small", "normal_big");
        Log.Info($"SendNotification：\n title:{title},\n boby:{body},\n Open:{deliveryTime},TimeSpan:{deliveryTime-DateTime.Now},smallIcon:normal_small,largeIcon:normal_big");
    }
}
