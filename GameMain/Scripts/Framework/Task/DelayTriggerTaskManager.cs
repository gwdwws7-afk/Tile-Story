using GameFramework;
using System;
using System.Collections.Generic;

/// <summary>
/// 延时触发任务管理器
/// </summary>
public sealed class DelayTriggerTaskManager
{
    private readonly LinkedList<DelayTriggerTask> delayTriggerTasks;
    private LinkedListNode<DelayTriggerTask> cachedNode;
    private readonly LinkedList<DelayTriggerTask> newDelayTriggerTasks;

    public DelayTriggerTaskManager()
    {
        delayTriggerTasks = new LinkedList<DelayTriggerTask>();
        newDelayTriggerTasks = new LinkedList<DelayTriggerTask>();
    }

    public void OnInit()
    {
    }

    public void OnReset()
    {
        ClearAllDelayTriggerTasks();
    }

    public void Update(float elapseSeconds, float realElapseSeconds)
    {
        foreach (DelayTriggerTask item in newDelayTriggerTasks)
        {
            delayTriggerTasks.AddLast(item);
        }
        newDelayTriggerTasks.Clear();

        LinkedListNode<DelayTriggerTask> current = delayTriggerTasks.First;
        while (current != null)
        {
            DelayTriggerTask delayTriggerTask = current.Value;
            cachedNode = current.Next;
            delayTriggerTask.waitTime -= elapseSeconds;
            if (delayTriggerTask.waitTime <= 0) 
            {
                delayTriggerTasks.Remove(current);
#if UNITY_EDITOR
                delayTriggerTask.endAction?.Invoke();
#else
                try
                {
                    delayTriggerTask.endAction?.Invoke();
                }
                catch(Exception e)
                {
                    Log.Error("DelayTriggerTask Error.{0}", e.Message);
                }
#endif                
                ReferencePool.Release(delayTriggerTask);
            }
            current = cachedNode;
        }
    }

    /// <summary>
    /// 添加延迟触发任务
    /// </summary>
    /// <param name="waitTime">延迟时间</param>
    /// <param name="endAction">结束事件</param>
    /// <param name="clearTrigger">是否清除时触发事件</param>
    /// <returns>任务序列号</returns>
    public int AddDelayTriggerTask(float waitTime, Action endAction, bool clearTrigger)
    {
        DelayTriggerTask task = DelayTriggerTask.Create(waitTime, endAction, clearTrigger);
        newDelayTriggerTasks.AddLast(task);
        return task.SerialId;
    }

    /// <summary>
    /// 移除延迟触发任务
    /// </summary>
    /// <param name="serialID">任务序列号</param>
    /// <returns>是否移除成功</returns>
    public bool RemoveDelayTriggerTask(int serialID)
    {
        var current = delayTriggerTasks.First;
        while (current != null)
        {
            DelayTriggerTask delayTriggerTask = current.Value;
            LinkedListNode<DelayTriggerTask> next = current.Next;
            if (delayTriggerTask.SerialId == serialID) 
            {
                if (current == cachedNode)
                {
                    cachedNode = current.Next;
                }
                delayTriggerTasks.Remove(current);
                ReferencePool.Release(delayTriggerTask);
                return true;
            }
            current = next;
        }
        return false;
    }

    /// <summary>
    /// 清除所有延迟触发任务
    /// </summary>
    public void ClearAllDelayTriggerTasks()
    {
        cachedNode = null;

        var current = delayTriggerTasks.First;
        while (current != null)
        {
            DelayTriggerTask delayTriggerTask = current.Value;
            LinkedListNode<DelayTriggerTask> next = current.Next;
            if (delayTriggerTask.clearTrigger)
            {
                try
                {
                    delayTriggerTask.endAction?.InvokeSafely();
                }
                catch (Exception e)
                {
                   Log.Error($"ClearAllDelayTriggerTasks:{e.Message}");
                }
            }
            ReferencePool.Release(delayTriggerTask);
            current = next;
        }
        delayTriggerTasks.Clear();
    }
}
