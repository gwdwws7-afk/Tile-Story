using MySelf.Model;
using System;

public class GoldCollectionTaskManager : RewardTaskManager<RewardTask>
{
    private DTGoldCollectionData dataTable;

    public DTGoldCollectionData DataTable
    {
        get
        {
            if (dataTable == null)
            {
                GetDataTable();
            }
            return dataTable;
        }
    }

    public int ActivityID
    {
        get
        {
            if (GoldCollectionModel.Instance.ActivityID <= 0)
            {
                GoldCollectionModel.Instance.ActivityID = GameManager.DataTable.GetDataTable<DTGoldCollectionScheduleData>().Data.GetNowActiveActivityID();
            }
            return GoldCollectionModel.Instance.ActivityID;
        }
    }
    
    public int CurrentIndex
    {
        get => GoldCollectionModel.Instance.CurrentIndex;
        set => GoldCollectionModel.Instance.CurrentIndex = value;
    }

    public int TotalCollectNum
    {
        get
        {
            try
            {
                if (GoldCollectionModel.Instance.TotalCollectNum < DataTable.GetNeedCollectNum(CurrentIndex - 1))
                {
                    GoldCollectionModel.Instance.TotalCollectNum = DataTable.GetNeedCollectNum(CurrentIndex - 1);
                }
            }
            catch (Exception e)
            {
                Log.Error("GoldCollection TotalCollectNum " + e);
            }
            
            return GoldCollectionModel.Instance.TotalCollectNum;
        }
        set => GoldCollectionModel.Instance.TotalCollectNum = value;
    }

    public int LastRecordTotalCollectNum
    {
        get => GoldCollectionModel.Instance.LastRecordTotalCollectNum;
        set => GoldCollectionModel.Instance.LastRecordTotalCollectNum = value;
    }

    public int LevelCollectNum
    {
        get => GoldCollectionModel.Instance.LevelCollectNum;
        set => GoldCollectionModel.Instance.LevelCollectNum = value;
    }

    public DateTime EndTime
    {
        get => GoldCollectionModel.Instance.EndTime;
        set => GoldCollectionModel.Instance.EndTime = value;
    }

    public bool ShowedFirstMenu
    {
        get => GoldCollectionModel.Instance.ShowedFirstMenu;
        set => GoldCollectionModel.Instance.ShowedFirstMenu = value;
    }

    public bool ShowedLastMenu
    {
        get => GoldCollectionModel.Instance.ShowedLastMenu;
        set => GoldCollectionModel.Instance.ShowedLastMenu = value;
    }

    public override void OnInit()
    {
        if (GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockGoldCollectionLevel)
        {
            if (EndTime == DateTime.MinValue)
            {
                SetEndTime();
                if (EndTime == DateTime.MinValue)
                    return;
            }
            AcceptTask();
        }
    }

    public override void OnReset()
    {
        dataTable = null;
        currentTask = null;
        CurrentIndex = 0;
        TotalCollectNum = 0;
        LastRecordTotalCollectNum = 0;
        LevelCollectNum = 0;
        EndTime = DateTime.MinValue;
        ShowedFirstMenu = false;
        ShowedLastMenu = false;
    }

    public override TaskTarget GetTaskTarget()
    {
        if (currentTask != null)
        {
            return currentTask.Target;
        }
        return TaskTarget.None;
    }

    public override void ConfirmTargetCollection(TaskTargetCollection collection)
    {
        if (currentTask != null)
        {
            int collectNum = collection.GetTargetCollectNum(currentTask.Target);
            TotalCollectNum += collectNum;
        }
    }

    /// <summary>
    /// 设定结束日期
    /// </summary>
    public DateTime SetEndTime()
    {
        //活动周期：活动循环开放，每周开两期，【周一00：00：00 至 周四24：00：00】、【周五00：00：00 至 周日 24：00：00】
        //int deltaDay = 0;
        //DayOfWeek now = DateTime.Now.DayOfWeek;
        //if (now >= DayOfWeek.Monday && now <= DayOfWeek.Thursday)
        //{
        //    deltaDay = DayOfWeek.Thursday - now;
        //}
        //else if (now >= DayOfWeek.Friday)
        //{
        //    deltaDay = DayOfWeek.Saturday - now + 1;
        //}
        //else if (now == DayOfWeek.Sunday)
        //{
        //    deltaDay = 0;
        //}
        //DateTime endTime = DateTime.Now.Date.AddDays(deltaDay + 1);
        DateTime endTime = GameManager.DataTable.GetDataTable<DTGoldCollectionScheduleData>().Data.GetNowActiveActivityEndTime();
        GoldCollectionModel.Instance.EndTime = endTime;
        return endTime;
    }

    /// <summary>
    /// 获取不同阶段的任务数据表
    /// </summary>
    private void GetDataTable()
    {
        dataTable = GameManager.DataTable.GetDataTable<DTGoldCollectionData>().Data;
        // int activityID = GameManager.DataTable.GetDataTable<DTGoldCollectionScheduleData>().Data.GetNowActiveActivityID();
        if (ActivityID > 0)
        {
            dataTable.GoldCollectionStages = dataTable.GoldCollectionStages.FindAll(stage => stage.ActivityID == ActivityID);
        }
    }

    /// <summary>
    /// 接受任务
    /// </summary>
    public void AcceptTask()
    {
        if (currentTask == null && !CheckAllTaskComplete())
        {
            int currentTaskIndex = CurrentIndex;
            if (currentTaskIndex == 0)
            {
                currentTaskIndex = 1;
                CurrentIndex = currentTaskIndex;
            }
            GoldCollectionStage stageTask = DataTable.GetStageTaskByIndex(currentTaskIndex);
            if (stageTask != null)
            {
                currentTask = RewardTask.Create(stageTask.StageID, stageTask.Index, stageTask.Target, stageTask.TargetNum, stageTask.RewardTypeList, stageTask.RewardNumList, null);
            }
            else
            {
                Log.Warning("Accept GoldCollectionTask Failed. Task data is null. ID:{0}", currentTaskIndex);
            }
        }
    }

    /// <summary>
    /// 接受下一个任务
    /// </summary>
    /// <returns>任务是否接受成功</returns>
    public bool AcceptNextTask()
    {
        currentTask = null;

        int currentTaskIndex = CurrentIndex;
        if (currentTaskIndex == 0)
        {
            currentTaskIndex = 1;
            CurrentIndex = currentTaskIndex;
        }
        GoldCollectionStage stageTask = DataTable.GetStageTaskByIndex(currentTaskIndex);
        if (stageTask == null)
        {
            return false;
        }

        currentTaskIndex++;
        CurrentIndex = currentTaskIndex;
        stageTask = DataTable.GetStageTaskByIndex(currentTaskIndex);
        if (stageTask != null)
        {
            currentTask = RewardTask.Create(stageTask.StageID, stageTask.Index, stageTask.Target, stageTask.TargetNum, stageTask.RewardTypeList, stageTask.RewardNumList, null);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 是否完成所有任务
    /// </summary>
    public bool CheckAllTaskComplete()
    {
        return CurrentIndex > DataTable.GoldCollectionStages.Count;
    }
}
