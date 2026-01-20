using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DTCardScheduleData
{
    public List<CardSchedule> CardSchedules;
    
    /// <summary>
    /// 获取当前DateTime去表格中找到起效的ActivityID
    /// </summary>
    public int GetNowActiveActivityID()
    {
        DateTime now = DateTime.Now;
        foreach (var schedule in CardSchedules)
        {
            if (schedule.StartTimeDT.AddDays(-1) <= now &&
                schedule.EndTimeDT >= now)
            {
                return schedule.ActivityID;
            }
        }
        return -1;
    }
    
    public DateTime GetActivityStartTimeByID(int activityID)
    {
        if (activityID <= 0)
            return DateTime.MinValue;
        
        foreach (var schedule in CardSchedules)
        {
            if (schedule.ActivityID == activityID)
            {
                return schedule.StartTimeDT;
            }
        }
        return DateTime.MinValue;
    }
    
    public DateTime GetActivityEndTimeByID(int activityID)
    {
        if (activityID <= 0)
            return DateTime.MinValue;
        
        foreach (var schedule in CardSchedules)
        {
            if (schedule.ActivityID == activityID)
            {
                return schedule.EndTimeDT;
            }
        }
        return DateTime.MinValue;
    }
    
    public DateTime GetNextActivityStartTime()
    {
        DateTime now = DateTime.Now;
        if (CardSchedules[0].StartTimeDT > now)
            return CardSchedules[0].StartTimeDT;
        for (int i = 0; i < CardSchedules.Count - 1; i++)
        {
            if (CardSchedules[i].EndTimeDT < now &&
                CardSchedules[i + 1].StartTimeDT > now)
            {
                return CardSchedules[i + 1].StartTimeDT;
            }
        }
        return DateTime.MinValue;
    }
}

[Serializable]
public class CardSchedule
{
    public int ID;
    public int ActivityID;
    public string StartTime;
    public string EndTime;

    private DateTime _startTimeDT = DateTime.MinValue;
    private DateTime _endTimeDT = DateTime.MinValue;
    
    public DateTime StartTimeDT
    {
        get
        {
            if (_startTimeDT == DateTime.MinValue)
            {
                _startTimeDT = DateTime.ParseExact(StartTime, "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            }
            return _startTimeDT;
        }
    }

    public DateTime EndTimeDT
    {
        get
        {
            if (_endTimeDT == DateTime.MinValue)
            {
                _endTimeDT = DateTime.ParseExact(EndTime, "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            }
            return _endTimeDT;
        }
    }
}