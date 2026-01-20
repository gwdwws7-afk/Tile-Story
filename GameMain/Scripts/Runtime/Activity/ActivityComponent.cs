using MySelf.Model;
using System;
using System.Collections.Generic;

/// <summary>
/// 活动组件
/// </summary>
public sealed class ActivityComponent : GameFrameworkComponent
{
    private ActivityManagerBase[] m_ActivityManagers;
    private DTActivityRoutineEventScheduleData m_ScheduleDatas;

    private int m_LastEndActivityId;

    //活动周期数据
    private DTActivityRoutineEventScheduleData ScheduleDatas
    {
        get
        {
            if (m_ScheduleDatas == null)
                m_ScheduleDatas = GameManager.DataTable.GetDataTable<DTActivityRoutineEventScheduleData>().Data;
            return m_ScheduleDatas;
        }
    }

    public void Initialize()
    {
        m_ActivityManagers = GetComponentsInChildren<ActivityManagerBase>();

        int curActivityID = GetCurActivityID();

        bool isAvailable = false;
        foreach (var activity in m_ActivityManagers)
        {
            activity.Initialize();

            if (activity.ActivityID == curActivityID)
                isAvailable = true;
        }

        //如果当前进行中的活动无效，强制关闭当前活动
        if (curActivityID != 0 && !isAvailable)
        {
            SetCurActivityID(0);
        }
    }

    public void Shutdown()
    {
        foreach (var activity in m_ActivityManagers)
        {
            activity.Shutdown();
        }
    }

    public bool CheckInitializationComplete()
    {
        if (m_ActivityManagers == null)
            return false;

        foreach (var activity in m_ActivityManagers)
        {
            if (!activity.CheckInitializationComplete())
                return false;
        }

        return true;
    }

    public void ActivityStartProcess()
    {
        List<ActivityManagerBase> canStartActivityList = new List<ActivityManagerBase>();
        List<ActivityManagerBase> openedActivityList = new List<ActivityManagerBase>();
        ActivityManagerBase hasOpenedActivity = null;        //如果是刚结束的活动，检测是否能开启顺序放到最后

        List<int> list = GetOpenedActivities();

        foreach (var activity in m_ActivityManagers)
        {
            if (activity.ActivityID == m_LastEndActivityId && hasOpenedActivity == null)
            {
                hasOpenedActivity = activity;

                continue;
            }

            if (list.Contains(activity.ActivityID))  
            {
                openedActivityList.Add(activity);

                continue;
            }

            if (activity.CheckActivityCanStart())
            {
                canStartActivityList.Add(activity);
            }
        }

        if (canStartActivityList.Count > 0)
        {
            canStartActivityList[UnityEngine.Random.Range(0, canStartActivityList.Count)].ActivityStartProcess();
        }
        else if (openedActivityList.Count > 0)
        {
            openedActivityList[UnityEngine.Random.Range(0, openedActivityList.Count)].ActivityStartProcess();
        }
        else if (hasOpenedActivity != null) 
        {
            hasOpenedActivity.ActivityStartProcess();
        }
    }

    public void ActivityEndProcess()
    {
        foreach (var activity in m_ActivityManagers)
        {
            activity.ActivityEndProcess();
        }
    }

    public void ActivityPreEndProcess()
    {
        foreach (var activity in m_ActivityManagers)
        {
            activity.ActivityPreEndProcess();
        }
    }

    public void ActivityAfterStartProcess()
    {
        foreach (var activity in m_ActivityManagers)
        {
            activity.ActivityAfterStartProcess();
        }
    }

    public void OnLevelWin(int levelFailTime, int hardIndex)
    {
        foreach (var activity in m_ActivityManagers)
        {
            activity.OnLevelWin(levelFailTime, hardIndex);
        }
    }

    public void OnLevelLose()
    {
        foreach (var activity in m_ActivityManagers)
        {
            activity.OnLevelLose();
        }
    }

    /// <summary>
    /// 获取当前周期编号
    /// </summary>
    /// <returns>周期编号</returns>
    public int GetCurPeriodID()
    {
        return ActivityModel.Instance.CurPeriodID;
    }

    private void SetCurPeriodID(int periodID)
    {
        ActivityModel.Instance.CurPeriodID = periodID;
    }

    /// <summary>
    /// 获取当前活动编号
    /// </summary>
    /// <returns>活动编号</returns>
    public int GetCurActivityID()
    {
        return ActivityModel.Instance.CurActivityID;
    }

    private void SetCurActivityID(int activityID)
    {
        ActivityModel.Instance.CurActivityID = activityID;
    }
    
    /// <summary>
    /// 获取当前活动的开始时间
    /// </summary>
    public DateTime GetCurActivityStartTime()
    {
        return ActivityModel.Instance.CurActivityStartTime;
    }

    /// <summary>
    /// 获取当前活动的结束时间
    /// </summary>
    public DateTime GetCurActivityEndTime()
    {
        return ActivityModel.Instance.CurActivityEndTime;
    }

    private void SetCurActivityEndTime(DateTime endTime)
    {
        ActivityModel.Instance.CurActivityEndTime = endTime;
    }
    
    private void SetCurActivityStartTime(DateTime startTime)
    {
        ActivityModel.Instance.CurActivityStartTime = startTime;
    }

    /// <summary>
    /// 确认活动是否可以开启
    /// </summary>
    /// <param name="activityID">活动编号</param>
    public bool CheckActivityCanStart(int activityID)
    {
        if (GetCurActivityID() != 0)
            return false;

        return CheckIsInSchedule(activityID, GetCurPeriodID()) != null;
    }

    /// <summary>
    /// 确认活动是否已经开启
    /// </summary>
    /// <param name="activityID">活动编号</param>
    /// <param name="periodID">周期编号</param>
    public bool CheckActivityHasStarted(int activityID, int periodID)
    {
        if (periodID <= 0)
            return false;

        return GetCurPeriodID() == periodID && GetCurActivityID() == activityID;
    }

    /// <summary>
    /// 开启活动
    /// </summary>
    /// <param name="activityID">活动编号</param>
    /// <returns>活动期数编号，为0表示开启活动失败</returns>
    public int StartActivity(int activityID)
    {
        if (GetCurActivityID() != 0)
            return 0;

        var data = CheckIsInSchedule(activityID, GetCurPeriodID());

        if (data != null)
        {
            SetCurPeriodID(data.ID);
            SetCurActivityID(data.ActivityID);
            SetCurActivityStartTime(data.StartTimeDT);
            SetCurActivityEndTime(DateTime.Now.AddMinutes(data.Time));

            return data.ID;
        }

        return 0;
    }

    /// <summary>
    /// 结束活动
    /// </summary>
    /// <param name="activityID">活动编号</param>
    public void EndActivity(int activityID)
    {
        if (GetCurActivityID() == activityID)
        {
            m_LastEndActivityId = activityID;

            SetCurActivityID(0);

            AddOpenedActivities(activityID);
        }
    }

    /// <summary>
    /// 确认是否在活动周期内
    /// </summary>
    /// <param name="activityID">活动编号</param>
    /// <returns>活动周期数据</returns>
    public ActivityRoutineEventScheduleData CheckIsInSchedule(int activityID, int periodID)
    {
        return ScheduleDatas.CheckIsInSchedule(activityID, periodID);
    }

    private List<int> GetOpenedActivities()
    {
        List<int> result = new List<int>();
        string str = UnityEngine.PlayerPrefs.GetString("OpenedActivitiesTS", String.Empty);
        if (!string.IsNullOrEmpty(str))
        {
            string[] strs = str.Split("_", StringSplitOptions.RemoveEmptyEntries);
            foreach (var splitStr in strs)
            {
                if (int.TryParse(splitStr, out int res)) 
                {
                    result.Add(res);
                }
            }
        }

        return result;
    }

    private void AddOpenedActivities(int activityID)
    {
        List<int> list = GetOpenedActivities();
        if (list.Contains(activityID))
            return;

        list.Add(activityID);

        //目前机制，开过三个不同活动后刷新
        if (list.Count >= 3)
        {
            ClearOpenedActivities();
            return;
        }

        string str = list[0].ToString();
        for (int i = 1; i < list.Count; i++)
        {
            str += "_" + list[i].ToString();
        }

        UnityEngine.PlayerPrefs.SetString("OpenedActivitiesTS", str);
    }

    private void ClearOpenedActivities()
    {
        UnityEngine.PlayerPrefs.SetString("OpenedActivitiesTS", String.Empty);
    }
}
