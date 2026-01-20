using System;

/// <summary>
/// 条件触发任务
/// </summary>
public sealed class ConditionTriggerTask : TaskBase
{
    private static int s_Serial = 0;

    private ConditionTriggerTaskStatus status;
    private Action startAction;
    private Func<bool> checkCompleteAction;
    private Action completeAction;
    private Action failAction;
    private float timeOut;

    public ConditionTriggerTaskStatus Status { get => status; set => status = value; }
    public Action StartAction { get => startAction; }
    public Func<bool> CheckCompleteAction { get => checkCompleteAction; }
    public Action CompleteAction { get => completeAction; }
    public Action FailAction { get => failAction; }
    public float TimeOut { get => timeOut; }

    public ConditionTriggerTask()
    {
        status = ConditionTriggerTaskStatus.Todo;
        startAction = null;
        checkCompleteAction = null;
        completeAction = null;
        failAction = null;
        timeOut = 0f;
    }

    public static ConditionTriggerTask Create(Action startAction, Func<bool> checkCompleteAction, Action completeAction, Action failAction, float timeOut)
    {
        ConditionTriggerTask conditionTriggerTask = GameFramework.ReferencePool.Acquire<ConditionTriggerTask>();
        conditionTriggerTask.Initialize(++s_Serial, "ConditionTriggerTask", DefaultPriority, null);
        conditionTriggerTask.startAction = startAction;
        conditionTriggerTask.checkCompleteAction = checkCompleteAction;
        conditionTriggerTask.completeAction = completeAction;
        conditionTriggerTask.failAction = failAction;
        conditionTriggerTask.timeOut = timeOut;
        return conditionTriggerTask;
    }

    public override void Clear()
    {
        base.Clear();
        status = ConditionTriggerTaskStatus.Todo;
        startAction = null;
        checkCompleteAction = null;
        completeAction = null;
        failAction = null;
        timeOut = 0f;
    }
}
