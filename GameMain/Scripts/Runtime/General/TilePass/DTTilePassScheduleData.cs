using MySelf.Model;
using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class DTTilePassScheduleData
{
    public List<TilePassSchedule> TilePassSchedules;

    /// <summary>
	/// 获取当前DateTime去表格中找到起效的ActivityID
	/// </summary>
	public int GetNowActiveActivityID()
    {
        //DateTime now = DateTime.Now;
        for (int i = 0; i < TilePassSchedules.Count; ++i)
        {
            if (TilePassSchedules[i].EndTimeDT == TilePassModel.Instance.EndTime)
            {
                return TilePassSchedules[i].ActivityID;
            }
        }
        return -1;
    }

    public DateTime GetNowActiveActivityEndTime()
    {
        DateTime now = DateTime.Now;
        for (int i = 0; i < TilePassSchedules.Count; ++i)
        {
            if (TilePassSchedules[i].StartTimeDT <= now &&
                TilePassSchedules[i].EndTimeDT >= now)
            {
                return TilePassSchedules[i].EndTimeDT;
            }
        }
        return DateTime.MinValue;
    }

    public int GetCurPeriodsIdByEndDateTime(DateTime endDateTime)
    {
        var targetData = TilePassSchedules.FirstOrDefault(d => d.StartTimeDT < endDateTime && d.EndTimeDT == endDateTime);
        if (targetData != null)
        {
            return targetData.PeriodsID;
        }
        return 0;
    }
}

[Serializable]
public class TilePassSchedule
{
    public int ID;
    public int ActivityID;
    public int PeriodsID;
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

