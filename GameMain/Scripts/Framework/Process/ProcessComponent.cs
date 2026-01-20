using System;

/// <summary>
/// 进程组件
/// </summary>
public sealed class ProcessComponent : GameFrameworkComponent
{
    private IProcessManager processManager = null;

    private bool reactivateProcess;

    /// <summary>
    /// 进程的数量
    /// </summary>
    public int Count
    {
        get
        {
            return processManager.Count;
        }
    }

    /// <summary>
    /// 当前正在进行的进程名称
    /// </summary>
    public string CurrentProcessName
    {
        get
        {
            return processManager.CurrentProcessName;
        }
    }

    /// <summary>
    /// 是否进程暂停
    /// </summary>
    public bool Pause
    {
        get
        {
            return processManager.Pause;
        }
        set
        {
            processManager.Pause = value;
        }
    }

    /// <summary>
    /// 进程是否结束
    /// </summary>
    public bool ProcessEnd
    {
        get
        {
            return processManager.ProcessEnd;
        }
    }

    /// <summary>
    /// 重新激活进程
    /// </summary>
    public bool ReactivateProcess
    {
        get
        {
            return reactivateProcess;
        }
        set
        {
            reactivateProcess = value;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        processManager = GameFrameworkEntry.GetModule<ProcessManager>();
        if (processManager == null)
        {
            Log.Fatal("Process manager is invalid");
            return;
        }
    }

    public void OnInit()
    {
    }

    public void SetProcessCallBack(Action startProgressCallBack, Action endProgressCallBack)
    {
        processManager.SetProcessCallBack(startProgressCallBack,endProgressCallBack);
    }

    public void OnReset()
    {
        processManager.Clear();
    }

    /// <summary>
    /// 获取进程信息
    /// </summary>
    /// <param name="processName">进程名称</param>
    public ProcessInfo GetProcessInfo(string processName)
    {
        return processManager.GetProcessInfo(processName);
    }

    /// <summary>
    /// 获取锁住的进程信息
    /// </summary>
    public ProcessInfo GetLockProcessInfo()
    {
        return processManager.GetLockProcessInfo();
    }
    
    /// <summary>
    /// 注册进程
    /// </summary>
    /// <param name="processType"></param>
    /// <param name="priority"></param>
    /// <param name="startAction"></param>
    public void Register(ProcessType processType, int priority, Action startAction)
    {
        processManager.Register(processType.ToString(), priority, startAction, null);
    }

    /// <summary>
    /// 注册进程
    /// </summary>
    /// <param name="name">进程名称</param>
    /// <param name="priority">进程优先级</param>
    /// <param name="startAction">进程开始事件</param>
    public void Register(string name, int priority, Action startAction)
    {
        processManager.Register(name, priority, startAction, null);
    }
    
    /// <summary>
    /// 注册进程
    /// </summary>
    /// <param name="processType"></param>
    /// <param name="priority"></param>
    /// <param name="startAction"></param>
    /// <param name="checkLockFunc"></param>
    public void Register(ProcessType processType, int priority, Action startAction, Func<bool> checkLockFunc)
    {
        processManager.Register(processType.ToString(), priority, startAction, checkLockFunc);
    }

    /// <summary>
    /// 注册进程
    /// </summary>
    /// <param name="name">进程名称</param>
    /// <param name="priority">进程优先级</param>
    /// <param name="startAction">进程开始事件</param>
    /// <param name="checkLockFunc">进程锁住</param>
    public void Register(string name, int priority, Action startAction, Func<bool> checkLockFunc)
    {
        processManager.Register(name, priority, startAction, checkLockFunc);
    }
    
    /// <summary>
    /// 注册进程
    /// </summary>
    /// <param name="processType"></param>
    /// <param name="priority"></param>
    /// <param name="startAction"></param>
    /// <param name="finishAction"></param>
    /// <param name="checkLockFunc"></param>
    public void Register(ProcessType processType, int priority, Action startAction, Action finishAction, Func<bool> checkLockFunc)
    {
        processManager.Register(processType.ToString(), priority, startAction, finishAction, checkLockFunc);
    }

    /// <summary>
    /// 注册进程
    /// </summary>
    /// <param name="name">进程名称</param>
    /// <param name="priority">进程优先级</param>
    /// <param name="startAction">进程开始事件</param>
    /// <param name="finishAction">进程结束事件</param>
    /// <param name="checkLockFunc">进程锁住</param>
    public void Register(string name, int priority, Action startAction, Action finishAction, Func<bool> checkLockFunc)
    {
        processManager.Register(name, priority, startAction, finishAction, checkLockFunc);
    }
    
    /// <summary>
    /// 注册进程
    /// </summary>
    /// <param name="nodeName"></param>
    /// <param name="processType"></param>
    /// <param name="startAction"></param>
    public void RegisterAfter(string nodeName, ProcessType processType, Action startAction)
    {
        processManager.RegisterAfter(nodeName, processType.ToString(), startAction, null, null);
    }

    /// <summary>
    /// 注册进程
    /// </summary>
    /// <param name="nodeName">前面的进程名称</param>
    /// <param name="name">进程名称</param>
    /// <param name="startAction">进程开始事件</param>
    public void RegisterAfter(string nodeName, string name, Action startAction)
    {
        processManager.RegisterAfter(nodeName, name, startAction, null, null);
    }
    
    /// <summary>
    /// 注册进程
    /// </summary>
    /// <param name="nodeName"></param>
    /// <param name="processType"></param>
    /// <param name="startAction"></param>
    /// <param name="checkLockFunc"></param>
    public void RegisterAfter(string nodeName, ProcessType processType, Action startAction, Func<bool> checkLockFunc)
    {
        processManager.RegisterAfter(nodeName, processType.ToString(), startAction, null, checkLockFunc);
    }

    /// <summary>
    /// 注册进程
    /// </summary>
    /// <param name="nodeName">前面的进程名称</param>
    /// <param name="name">进程名称</param>
    /// <param name="startAction">进程开始事件</param>
    /// <param name="checkLockFunc">进程锁住</param>
    public void RegisterAfter(string nodeName, string name, Action startAction, Func<bool> checkLockFunc)
    {
        processManager.RegisterAfter(nodeName, name, startAction, null, checkLockFunc);
    }
    
    /// <summary>
    /// 注册进程
    /// </summary>
    /// <param name="nodeName"></param>
    /// <param name="processType"></param>
    /// <param name="startAction"></param>
    /// <param name="finishAction"></param>
    /// <param name="checkLockFunc"></param>
    public void RegisterAfter(string nodeName, ProcessType processType, Action startAction, Action finishAction, Func<bool> checkLockFunc)
    {
        processManager.RegisterAfter(nodeName, processType.ToString(), startAction, finishAction, checkLockFunc);
    }

    /// <summary>
    /// 注册进程
    /// </summary>
    /// <param name="nodeName">前面的进程名称</param>
    /// <param name="name">进程名称</param>
    /// <param name="startAction">进程开始事件</param>
    /// <param name="finishAction">进程结束事件</param>
    /// <param name="checkLockFunc">进程锁住</param>
    public void RegisterAfter(string nodeName, string name, Action startAction, Action finishAction, Func<bool> checkLockFunc)
    {
        processManager.RegisterAfter(nodeName, name, startAction, finishAction, checkLockFunc);
    }
    
    /// <summary>
    /// 注销进程
    /// </summary>
    /// <param name="processType"></param>
    /// <returns></returns>
    public bool Unregister(ProcessType processType)
    {
        return processManager.Unregister(processType.ToString());
    }

    /// <summary>
    /// 注销进程
    /// </summary>
    /// <param name="name">进程名称</param>
    /// <returns>是否注销成功</returns>
    public bool Unregister(string name)
    {
        return processManager.Unregister(name);
    }

    /// <summary>
    /// 注销全部进程
    /// </summary>
    /// <returns>注销进程的数量</returns>
    public int UnregisterAll()
    {
        return processManager.UnregisterAll();
    }

    /// <summary>
    /// 注销所有不锁住的进程
    /// </summary>
    /// <returns>是否有锁住的进程</returns>
    public bool UnregisterAllUnlock()
    {
        Log.Info("注销所有不锁住的进程...............");

        return processManager.UnregisterAllUnlock();
    }

    /// <summary>
    /// 执行进程
    /// </summary>
    public void ExecuteProcess()
    {
        processManager.ExecuteProcess();
    }
    
    /// <summary>
    /// 结束进程
    /// </summary>
    /// <param name="processType"></param>
    /// <returns></returns>
    public bool EndProcess(ProcessType processType)
    {
        return processManager.EndProcess(processType.ToString());
    }

    /// <summary>
    /// 结束进程
    /// </summary>
    /// <param name="name">进程名称</param>
    /// <returns>是否结束成功</returns>
    public bool EndProcess(string name)
    {
        return processManager.EndProcess(name);
    }
    
    /// <summary>
    /// IsContainProcess
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool IsContainProcess(ProcessType type)
    {
        return processManager.IsContainProcess(type);
    }
}
