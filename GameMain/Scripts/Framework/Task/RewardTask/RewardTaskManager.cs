using GameFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 奖励任务管理器
/// </summary>
/// <typeparam name="T">奖励任务</typeparam>
public abstract class RewardTaskManager<T> : TaskManagerBase where T : TaskBase
{
    protected T currentTask;

    /// <summary>
    /// 当前任务
    /// </summary>
    public T CurrentTask
    {
        get
        {
            return currentTask;
        }
    }
}
