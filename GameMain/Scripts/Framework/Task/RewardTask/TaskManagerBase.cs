using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 任务管理器基类
/// </summary>
public abstract class TaskManagerBase
{
    public abstract void OnInit();

    public abstract void OnReset();

    public abstract void ConfirmTargetCollection(TaskTargetCollection collection);

    public abstract TaskTarget GetTaskTarget();
}
