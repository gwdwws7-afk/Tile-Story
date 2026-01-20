using System;

/// <summary>
/// 延时任务
/// </summary>
public sealed class DelayTriggerTask : TaskBase
{
    private static int s_Serial = 0;

    public float waitTime;
    public Action endAction;
    public bool clearTrigger;

    public DelayTriggerTask()
    {
        waitTime = 0;
        endAction = null;
        clearTrigger = false;
    }

    public static DelayTriggerTask Create(float waitTime, Action endAction, bool clearTrigger)
    {
        DelayTriggerTask delayTriggerTask = GameFramework.ReferencePool.Acquire<DelayTriggerTask>();
        delayTriggerTask.Initialize(++s_Serial, "DelayTriggerTask", DefaultPriority, null);
        delayTriggerTask.waitTime = waitTime;
        delayTriggerTask.endAction = endAction;
        delayTriggerTask.clearTrigger = clearTrigger;
        return delayTriggerTask;
    }

    public override void Clear()
    {
        base.Clear();
        waitTime = 0;
        endAction = null;
        clearTrigger = false;
    }
}
