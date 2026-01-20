using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 进程管理器
/// </summary>
public sealed partial class ProcessManager: GameFrameworkModule, IProcessManager
{
    private readonly LinkedList<Process> processList;
    private string currentProcessName;
    private bool pause;
    private bool processPaused;
    private bool processEnd;

    private Action lockActionByStartProcess;
    private Action unlockActionByEndProcess;

    public ProcessManager()
    {
        processList = new LinkedList<Process>();
        currentProcessName = null;
        pause = false;
        processPaused = false;
        processEnd = true;
    }

    public void SetProcessCallBack(Action lockByStartProcess,Action unlockByEndProcess)
    {
        this.lockActionByStartProcess = lockByStartProcess;
        this.unlockActionByEndProcess = unlockByEndProcess;
    }

    /// <summary>
    /// 进程的数量
    /// </summary>
    public int Count
    {
        get
        {
            return processList.Count;
        }
    }

    /// <summary>
    /// 当前正在进行的进程名称
    /// </summary>
    public string CurrentProcessName
    {
        get
        {
            return currentProcessName;
        }
    }

    /// <summary>
    /// 是否进程暂停
    /// </summary>
    public bool Pause
    {
        get
        {
            return pause;
        }
        set
        {
            pause = value;
        }
    }

    /// <summary>
    /// 进程是否结束
    /// </summary>
    public bool ProcessEnd
    {
        get
        {
            return processEnd;
        }
    }

    public override void Update(float elapseSeconds, float realElapseSeconds)
    {
        if (processPaused && !pause)
        {
            processPaused = false;
            ExecuteProcess();
        }
    }

    public override void Shutdown()
    {
        Clear();
    }

    public void Clear()
    {
        processEnd = true;
        pause = false;
        processPaused = false;
        processList.Clear();
        currentProcessName = null;
    }

    public ProcessInfo GetProcessInfo(string processName)
    {
        if (string.IsNullOrEmpty(processName))
        {
            return default(ProcessInfo);
        }

        LinkedListNode<Process> current = processList.First;

        while (current != null)
        {
            LinkedListNode<Process> next = current.Next;

            if (current.Value.Name == processName)
            {
                return new ProcessInfo(processName, current.Value.Priority, current.Value.Status, current.Value.IsLock);
            }

            current = next;
        }

        return default(ProcessInfo);
    }

    public ProcessInfo GetLockProcessInfo()
    {
        LinkedListNode<Process> current = processList.First;

        while (current != null)
        {
            LinkedListNode<Process> next = current.Next;

            if (current.Value.IsLock)
            {
                return new ProcessInfo(current.Value.Name, current.Value.Priority, current.Value.Status, current.Value.IsLock);
            }

            current = next;
        }

        return default(ProcessInfo);
    }

    public void Register(string name, int priority, Action startAction)
    {
        Register(name, priority, startAction, null, null);
    }

    public void Register(string name, int priority, Action startAction, Func<bool> checkLockFunc)
    {
        Register(name, priority, startAction, null, checkLockFunc);
    }

    public void Register(string name, int priority, Action startAction, Action finishAction, Func<bool> checkLockFunc)
    {
        Process processData = Process.Create(name, priority, startAction, finishAction, ProcessStatus.Wait, checkLockFunc);
        LinkedListNode<Process> current = processList.Last;
        LinkedListNode<Process> targetNode = null;

        while (current != null)
        {
            targetNode = current;

            if (priority <= current.Value.Priority)
            {
                break;
            }
            else
            {
                current = current.Previous;
            }
        }

        if (targetNode != null)
        {
            if (targetNode.Value.Priority >= priority || targetNode.Value.Status == ProcessStatus.Playing) 
            {
                processList.AddAfter(targetNode, processData);
            }
            else
            {
                processList.AddBefore(targetNode, processData);
            }
        }
        else
        {
            processList.AddLast(processData);
        }
    }

    public void RegisterAfter(string nodeName, string name, Action startAction)
    {
        RegisterAfter(nodeName, name, startAction, null, null);
    }

    public void RegisterAfter(string nodeName, string name, Action startAction, Func<bool> checkLockFunc)
    {
        RegisterAfter(nodeName, name, startAction, null, checkLockFunc);
    }

    public void RegisterAfter(string nodeName, string name, Action startAction, Action finishAction, Func<bool> checkLockFunc)
    {
        LinkedListNode<Process> beforeProcessNode = InternalGetProcessNode(nodeName);

        int priority = 0;
        if (beforeProcessNode != null)
        {
            priority = beforeProcessNode.Value.Priority;
        }
        Process afterProcess = Process.Create(name, priority, startAction, finishAction, ProcessStatus.Wait, checkLockFunc);

        if (beforeProcessNode != null)
        {
            processList.AddAfter(beforeProcessNode, afterProcess);
        }
        else
        {
            processList.AddLast(afterProcess);
        }
    }

    public bool Unregister(string name)
    {
        foreach (Process processData in processList)
        {
            if (processData.Name.Equals(name, StringComparison.Ordinal))
            {
                if (processData.Status == ProcessStatus.Playing)
                {
                    continue;
                }

                processData.Status = ProcessStatus.None;

                return true;
            }
        }

        return false;
    }

    public int UnregisterAll()
    {
        int count = processList.Count;

        foreach (Process processData in processList)
        {
            processData.Status = ProcessStatus.None;
        }

        processList.Clear();
        
        unlockActionByEndProcess?.Invoke();
        return count;
    }

    public bool UnregisterAllUnlock()
    {
        bool haslockProcess = false;

        foreach (Process processData in processList)
        {
            if (!processData.IsLock)
            {
                if (processData.Status != ProcessStatus.Playing)
                {
                    processData.Status = ProcessStatus.None;
                }
            }
            else
            {
                haslockProcess = true;
            }
        }
        unlockActionByEndProcess?.Invoke();

        return haslockProcess;
    }

    public void ExecuteProcess()
    {
        if (pause)
        {
            if (processList.Count > 0)
            {
                processEnd = false;
            }

            processPaused = true;
            return;
        }

        if (processList.Count > 0)
        {
            processEnd = false;

            LinkedListNode<Process> current = processList.First;

            if (current.Value.Status == ProcessStatus.Wait)
            {
                currentProcessName = current.Value.Name;
                current.Value.StartProcess(LockByStartProcess);
            }
            else if (current.Value.Status == ProcessStatus.End)
            {
                processList.RemoveFirst();
                current.Value.EndProcess(UnlockByEndProcess);
                currentProcessName = null;
                ExecuteProcess();
            }
            else if (current.Value.Status == ProcessStatus.None)
            {
                Log.Warning("Current process status is none");
                processList.RemoveFirst();
                currentProcessName = null;
                unlockActionByEndProcess?.Invoke();
                ExecuteProcess();
            }
            else if (current.Value.Status == ProcessStatus.Playing)
            {
                Log.Warning("Execute First Process is Playing");
            }
        }
        else
        {
            processEnd = true;
        }
    }

    public bool EndProcess(string name)
    {
        foreach (Process process in processList)
        {
            if (process.Name.Equals(name, StringComparison.Ordinal))
            {
                unlockActionByEndProcess?.Invoke();
                if (process.Status == ProcessStatus.Playing)
                {
                    processList.RemoveFirst();
                    process.EndProcess();
                    currentProcessName = null;
                    ExecuteProcess();
                }
                else if (process.Status == ProcessStatus.Wait)
                {
                    process.Status = ProcessStatus.End;
                }
                else if (process.Status == ProcessStatus.None) 
                {
                    Log.Warning("try end process status is none");
                    process.Status = ProcessStatus.End;
                }
                return true;
            }
        }
        return false;
    }

    private LinkedListNode<Process> InternalGetProcessNode(string name)
    {
        LinkedListNode<Process> current = processList.First;
        while (current != null)
        {
            if (current.Value.Name.Equals(name, StringComparison.Ordinal) && current.Value.Status != ProcessStatus.None) 
            {
                return current;
            }

            current = current.Next;
        }

        return null;
    }

    private void LockByStartProcess()
    {
        bool isLock = false;
        foreach (var process in processList)
        {
            if (process.IsLock)
            {
                isLock = true;
                break;
            }
        }

        if (isLock)
        {
            lockActionByStartProcess?.Invoke();   
        }
    }

    private void UnlockByEndProcess()
    {
        bool isLock = false;
        foreach (var process in processList)
        {
            if (process.IsLock)
            {
                isLock = true;
                break;
            }
        }

        if (!isLock)
        {
            unlockActionByEndProcess?.Invoke();   
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool IsContainProcess(ProcessType type)
    {
        foreach (var process in processList)
        {
            if (process.Name.Equals(type.ToString()))
            {
                return true;
            }
        }
        return false;
    }
}
