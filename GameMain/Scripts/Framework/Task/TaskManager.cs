using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 任务管理器
/// </summary>
public sealed class TaskManager : GameFrameworkModule, ITaskManager
{
    private TaskTargetCollection targetCollection;
    private List<TaskManagerBase> taskManagerList;

    private DelayTriggerTaskManager delayTriggerTaskManager;

    private OrderDictionary<string, Action> serializeTaskManager;
    private bool needSaveData;

    public TaskManager()
    {
        targetCollection = new TaskTargetCollection();
        taskManagerList = new List<TaskManagerBase>();
        delayTriggerTaskManager = new DelayTriggerTaskManager();
        serializeTaskManager = new OrderDictionary<string, Action>();
    }

    public void OnInit()
    {
        for (int i = 0; i < taskManagerList.Count; i++)
        {
            taskManagerList[i].OnInit();
        }

        delayTriggerTaskManager.OnInit();
    }

    public void OnReset()
    {
        for (int i = 0; i < taskManagerList.Count; i++)
        {
            taskManagerList[i].OnReset();
        }

        delayTriggerTaskManager.ClearAllDelayTriggerTasks();
    }

    public override void Shutdown()
    {
    }

    public override void Update(float elapseSeconds, float realElapseSeconds)
    {
        delayTriggerTaskManager.Update(elapseSeconds, realElapseSeconds);

        ExecutionByFrame();
        ExecutionSaveDataTask();

        if (GameManager.Task.IsInit)
        {
            GameManager.Task.BalloonRiseManager.Update(elapseSeconds, realElapseSeconds);
        }
    }

    #region TaskMangerList

    public void AddTaskManager(TaskManagerBase taskManagerBase)
    {
        if (!taskManagerList.Contains(taskManagerBase))
        {
            taskManagerList.Add(taskManagerBase);
        }
    }

    public void RemoveTaskManager(TaskManagerBase taskManagerBase)
    {
        if (taskManagerList.Contains(taskManagerBase))
        {
            taskManagerList.Remove(taskManagerBase);
        }
    }

    public void ClearTaskManager()
    {
        taskManagerList.Clear();
    }

    public List<TaskTarget> GetAllTaskTargets()
    {
        List<TaskTarget> taskTargets = new List<TaskTarget>();
        for (int i = 0; i < taskManagerList.Count; i++)
        {
            var target = taskManagerList[i].GetTaskTarget();
            if (target != TaskTarget.None && !taskTargets.Contains(target)) 
            {
                taskTargets.Add(target);
            }
        }

        return taskTargets;
    }

    #endregion

    #region TargetCollection
    /// <summary>
    /// 获取任务目标收集数
    /// </summary>
    /// <param name="rewardTaskTarget">任务目标</param>
    public int GetTargetCollectNum(TaskTarget rewardTaskTarget)
    {
        return targetCollection.GetTargetCollectNum(rewardTaskTarget);
    }

    /// <summary>
    /// 增加任务目标收集数
    /// </summary>
    /// <param name="rewardTaskTarget">任务目标</param>
    /// <param name="collectNum">收集数量</param>
    public void AddTargetCollection(TaskTarget rewardTaskTarget, int collectNum)
    {
        targetCollection.AddTargetCollectNum(rewardTaskTarget, collectNum);
    }

    /// <summary>
    /// 确认任务目标收集数
    /// </summary>
    public void ConfirmTargetCollection()
    {
        for (int i = 0; i < taskManagerList.Count; i++)
        {
            taskManagerList[i].ConfirmTargetCollection(targetCollection);
        }

        targetCollection.Clear();
    }

    /// <summary>
    /// 清空任务目标收集数
    /// </summary>
    /// <param name="rewardTaskTarget">任务目标</param>
    public void ClearTargetCollectNum(TaskTarget rewardTaskTarget)
    {
        targetCollection.ClearTargetCollectNum(rewardTaskTarget);
    }

    /// <summary>
    /// 清空任务目标收集数
    /// </summary>
    public void ClearTargetCollection()
    {
        targetCollection.Clear();
    }
    #endregion

    #region DelayTriggerTask
    public int AddDelayTriggerTask(float waitTime, Action endAction, bool clearTrigger)
    {
        return delayTriggerTaskManager.AddDelayTriggerTask(waitTime, endAction, clearTrigger);
    }

    public bool RemoveDelayTriggerTask(int serialID)
    {
        return delayTriggerTaskManager.RemoveDelayTriggerTask(serialID);
    }

    public void ClearAllDelayTriggerTasks()
    {
        delayTriggerTaskManager.ClearAllDelayTriggerTasks();
    }
    #endregion

    #region serializeTaskManager

    public void AddExecutionByFrame(string taskName, Action taskAction)
    {
        if (serializeTaskManager.ContainsKey(taskName))
        {
            serializeTaskManager[taskName] = taskAction;
        }
        else
            serializeTaskManager.Add(taskName, taskAction);
    }

    private void ExecutionByFrame()
    {
        if (serializeTaskManager.Keys.Count > 0)
        {
            serializeTaskManager[serializeTaskManager.Keys[0]].Invoke();
            serializeTaskManager.Remove(serializeTaskManager.Keys[0]);
        }
    }

    #endregion

    #region IEnumerator

    public IEnumerator DelayWaitForEndOfFrameEvent(Action action1, Action action2)
    {
        yield return new WaitForEndOfFrame();
        action1?.InvokeSafely();
        yield return new WaitForEndOfFrame();
        action2?.InvokeSafely();
    }
    #endregion

    public void AddSaveDataTask()
    {
        needSaveData = true;
    }

    private void ExecutionSaveDataTask()
    {
        if (needSaveData)
        {
            needSaveData = false;
            PlayerPrefs.Save();
        }
    }
}
