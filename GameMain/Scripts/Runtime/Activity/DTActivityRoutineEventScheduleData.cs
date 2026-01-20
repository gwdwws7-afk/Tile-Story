using System;
using System.Collections.Generic;

/// <summary>
/// 常规活动循环数据表
/// </summary>
[Serializable]
public class DTActivityRoutineEventScheduleData
{
    public List<ActivityRoutineEventScheduleData> ScheduleDatas;

    /// <summary>
    /// 获取活动周期数据
    /// </summary>
    /// <param name="id">序号</param>
    /// <returns>活动周期数据</returns>
    public ActivityRoutineEventScheduleData GetScheduleData(int id)
    {
        foreach (ActivityRoutineEventScheduleData data in ScheduleDatas)
        {
            if (data.ID == id)
                return data;
        }

        return null;
    }

    /// <summary>
    /// 确认是否在活动周期内
    /// </summary>
    /// <param name="activityID">活动编号</param>
    /// <param name="periodID">周期编号</param>
    /// <returns>活动周期数据</returns>
    public ActivityRoutineEventScheduleData CheckIsInSchedule(int activityID, int periodID)
    {
        foreach (ActivityRoutineEventScheduleData data in ScheduleDatas)
        {
            if (data.ActivityID == activityID && data.ID != periodID && DateTime.Now >= data.StartTimeDT && DateTime.Now < data.EndTimeDT) 
                return data;
        }

        return null;
    }
}
