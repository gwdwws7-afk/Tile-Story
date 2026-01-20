using System;
using System.Collections;
using System.Collections.Generic;

public interface ITaskManager
{
    void OnInit();

    void OnReset();

    void AddTaskManager(TaskManagerBase taskManagerBase);

    void RemoveTaskManager(TaskManagerBase taskManagerBase);

    void ClearTaskManager();

    int GetTargetCollectNum(TaskTarget rewardTaskTarget);

    void AddTargetCollection(TaskTarget rewardTaskTarget, int collectNum);

    void ConfirmTargetCollection();

    void ClearTargetCollectNum(TaskTarget rewardTaskTarget);

    void ClearTargetCollection();

    int AddDelayTriggerTask(float waitTime, Action endAction, bool clearTrigger);

    bool RemoveDelayTriggerTask(int serialID);

    void ClearAllDelayTriggerTasks();

    List<TaskTarget> GetAllTaskTargets();

    void AddExecutionByFrame(string taskName, Action taskAction);

    IEnumerator DelayWaitForEndOfFrameEvent(Action action1, Action action2);

    void AddSaveDataTask();
}
