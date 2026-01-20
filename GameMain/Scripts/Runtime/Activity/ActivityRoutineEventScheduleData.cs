using System;

/// <summary>
/// 常规活动循环数据类
/// </summary>
[Serializable]
public class ActivityRoutineEventScheduleData
{
    public int ID;//期数
    public int ActivityID;//活动编号
    public string StartTime;//生效开始时间
    public string EndTime;//生效结束时间
    public int Time;//活动开启时长（min）

    private DateTime startTime = DateTime.MinValue;
    private DateTime endTime = DateTime.MinValue;

    public DateTime StartTimeDT
    {
        get
        {
            if (startTime == DateTime.MinValue)
            {
                startTime = DateTime.ParseExact(StartTime, "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            }
            return startTime;
        }
    }

    public DateTime EndTimeDT
    {
        get
        {
            if (endTime == DateTime.MinValue)
            {
                endTime = DateTime.ParseExact(EndTime, "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            }
            return endTime;
        }
    }
}
