using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DTNotificationData
{
    public List<NotificationData> notificationData;

    public NotificationData GetNotificationData(NotificationKey key)
    {
        for (int i = 0; i < notificationData.Count; i++)
        {
            if (notificationData[i].Key == key)
            {
                return notificationData[i];
            }
        }

        return null;
    }

    public List<NotificationData> GetNotificationData(DayOfWeek dayOfWeek)
    {
        List<NotificationData> result = new List<NotificationData>();
        for (int i = 0; i < notificationData.Count; i++)
        {
            if (notificationData[i].DayOfWeek == dayOfWeek) 
            {
                result.Add(notificationData[i]);
            }
        }

        return result;
    }

    public List<NotificationData> GetNotificationData(DayOfWeek dayOfWeek, int hours, int minutes)
    {
        List<NotificationData> result = new List<NotificationData>();
        for (int i = 0; i < notificationData.Count; i++)
        {
            if (notificationData[i].DayOfWeek == dayOfWeek && notificationData[i].Hours == hours && notificationData[i].Minutes == minutes) 
            {
                result.Add(notificationData[i]);
            }
        }

        return result;
    }
}
