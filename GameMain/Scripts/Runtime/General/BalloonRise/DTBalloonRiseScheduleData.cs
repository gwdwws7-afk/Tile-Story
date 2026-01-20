using System;
using System.Collections.Generic;

[Serializable]
public class DTBalloonRiseScheduleData
{
    public List<BalloonRiseSchedule> BalloonRiseSchedules;

    public int GetNowActiveActivityID()
    {
        for (int i = 0; i < BalloonRiseSchedules.Count; ++i)
        {
            if (BalloonRiseSchedules[i].EndTimeDT == GameManager.Task.BalloonRiseManager.EndTime)
            {
                return BalloonRiseSchedules[i].ActivityID;
            }
        }
        return -1;
    }

    public DateTime GetNowActiveActivityEndTime()
    {
        DateTime now = DateTime.Now;
        for (int i = 0; i < BalloonRiseSchedules.Count; ++i)
        {
            if (BalloonRiseSchedules[i].StartTimeDT <= now &&
                BalloonRiseSchedules[i].EndTimeDT >= now)
            {
                return BalloonRiseSchedules[i].EndTimeDT;
            }
        }
        return DateTime.MinValue;
    }
}

[Serializable]
public class BalloonRiseSchedule
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
