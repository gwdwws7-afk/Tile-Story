using System;
using System.Collections.Generic;
using MySelf.Model;

/// <summary>
/// 任务组件
/// </summary>
public sealed class TaskComponent : GameFrameworkComponent
{
    private ITaskManager taskManager = null;
    
    public PersonRankActivityManager PersonRankManager { get; private set; }
    public CalendarChallengeManager CalendarChallengeManager { get; private set; }
    public GoldCollectionTaskManager GoldCollectionTaskManager { get; private set; }
    public GlacierQuestTaskManager GlacierQuestTaskManager { get; private set; }
    public BalloonRiseActivityManager BalloonRiseManager { get; private set; }
    
    private bool isInit = false;

    public bool IsInit => isInit;

    protected override void Awake()
    {
        base.Awake();

        taskManager = GameFrameworkEntry.GetModule<TaskManager>();
        if (taskManager == null)
        {
            Log.Fatal("Task manager is invalid");
            return;
        }
    }

    public void OnInit()
    {
        if(isInit)return;
        
        isInit = true;
        
        PersonRankManager = new PersonRankActivityManager();
        CalendarChallengeManager = new CalendarChallengeManager();
        GoldCollectionTaskManager = new GoldCollectionTaskManager();
        GlacierQuestTaskManager = new GlacierQuestTaskManager();
        BalloonRiseManager = new BalloonRiseActivityManager();
        
        taskManager.AddTaskManager(PersonRankManager);
        taskManager.AddTaskManager(CalendarChallengeManager);
        taskManager.AddTaskManager(GoldCollectionTaskManager);
        taskManager.AddTaskManager(GlacierQuestTaskManager);
        taskManager.AddTaskManager(BalloonRiseManager);
        
        taskManager.OnInit();
    }

    public void OnReset()
    {
        taskManager.OnReset();
    }

    /// <summary>
    /// 获取任务目标收集数
    /// </summary>
    /// <param name="rewardTaskTarget">任务目标</param>
    public int GetTargetCollectNum(TaskTarget rewardTaskTarget)
    {
        return taskManager.GetTargetCollectNum(rewardTaskTarget);
    }

    /// <summary>
    /// 增加任务目标收集数
    /// </summary>
    /// <param name="rewardTaskTarget">任务目标</param>
    /// <param name="collectNum">收集数量</param>
    public void AddTargetCollection(TaskTarget rewardTaskTarget, int collectNum)
    {
        taskManager.AddTargetCollection(rewardTaskTarget, collectNum);
    }

    /// <summary>
    /// 清空任务目标收集数
    /// </summary>
    /// <param name="rewardTaskTarget">任务目标</param>
    public void ClearTargetCollectNum(TaskTarget rewardTaskTarget)
    {
        taskManager.ClearTargetCollectNum(rewardTaskTarget);
    }

    /// <summary>
    /// 确认任务目标收集数
    /// </summary>
    public void ConfirmTargetCollection()
    {
        taskManager.ConfirmTargetCollection();
    }

    /// <summary>
    /// 清空任务目标收集数
    /// </summary>
    public void ClearTargetCollection()
    {
        taskManager.ClearTargetCollection();
    }

    /// <summary>
    /// 增加延时触发任务
    /// </summary>
    /// <param name="waitTime">延时时间</param>
    /// <param name="endAction">结束事件</param>
    /// <returns>任务序列号</returns>
    public int AddDelayTriggerTask(float waitTime, Action endAction)
    {
        return taskManager.AddDelayTriggerTask(waitTime, endAction, false);
    }

    /// <summary>
    /// 增加延时触发任务
    /// </summary>
    /// <param name="waitTime">延时时间</param>
    /// <param name="endAction">结束事件</param>
    /// <param name="clearTrigger">是否清空时触发结束事件</param>
    /// <returns>任务序列号</returns>
    public int AddDelayTriggerTask(float waitTime, Action endAction, bool clearTrigger)
    {
        return taskManager.AddDelayTriggerTask(waitTime, endAction, clearTrigger);
    }

    /// <summary>
    /// 移除延时触发任务
    /// </summary>
    /// <param name="serialID">任务序列号</param>
    /// <returns>是否移除成功</returns>
    public bool RemoveDelayTriggerTask(int serialID)
    {
        return taskManager.RemoveDelayTriggerTask(serialID);
    }

    /// <summary>
    /// 清空延时触发任务
    /// </summary>
    public void ClearAllDelayTriggerTasks()
    {
        taskManager.ClearAllDelayTriggerTasks();
    }

    /// <summary>
    /// 获取所有任务的任务目标
    /// </summary>
    public List<TaskTarget> GetAllTaskTargets()
    {
        return taskManager.GetAllTaskTargets();
    }

    public void AddExecutionByFrame(string taskName, Action callBack)
    {
        taskManager.AddExecutionByFrame(taskName, callBack);
    }

    public void DelayWaitForEndOfFrameEvent(Action action1, Action action2)
    {
        StartCoroutine(taskManager.DelayWaitForEndOfFrameEvent(action1, action2));
    }
    
    /// <summary>
    /// 保存数据
    /// </summary>
    public void AddSaveDataTask()
    {
        taskManager.AddSaveDataTask();
    }
}
