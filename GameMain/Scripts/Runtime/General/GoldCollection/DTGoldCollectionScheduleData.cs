using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DTGoldCollectionScheduleData
{
    public List<GoldCollectionSchedule> GoldCollectionSchedules;

    /// <summary>
	/// 获取当前DateTime去表格中找到起效的ActivityID
	/// </summary>
	public int GetNowActiveActivityID()
    {
        DateTime now = DateTime.Now;
        for (int i = 0; i < GoldCollectionSchedules.Count; ++i)
        {
            if (GoldCollectionSchedules[i].StartTimeDT <= now &&
                GoldCollectionSchedules[i].EndTimeDT >= now)
            {
                return GoldCollectionSchedules[i].ActivityID;
            }
        }
        return -1;
    }

    public DateTime GetNowActiveActivityEndTime()
    {
        DateTime now = DateTime.Now;
        for (int i = 0; i < GoldCollectionSchedules.Count; ++i)
        {
            if (GoldCollectionSchedules[i].StartTimeDT <= now &&
                GoldCollectionSchedules[i].EndTimeDT >= now)
            {
                return GoldCollectionSchedules[i].EndTimeDT;
            }
        }
        return DateTime.MinValue;
    }
}

[Serializable]
public class GoldCollectionSchedule
{
    public int ID;
    public int ActivityID;
    public string StartTime;
    public string EndTime;

    public DateTime StartTimeDT
    {
        get
        {
            if (startTimeDT == DateTime.MinValue)
            {
                startTimeDT = DateTime.ParseExact(StartTime, "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            }
            return startTimeDT;
        }
    }
    private DateTime startTimeDT = DateTime.MinValue;

    public DateTime EndTimeDT
    {
        get
        {
            if (endTimeDT == DateTime.MinValue)
            {
                endTimeDT = DateTime.ParseExact(EndTime, "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            }
            return endTimeDT;
        }
    }
    private DateTime endTimeDT = DateTime.MinValue;
}
