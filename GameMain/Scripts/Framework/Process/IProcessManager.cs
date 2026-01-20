using System;

/// <summary>
/// 进程管理器接口
/// </summary>
public interface IProcessManager
{
    /// <summary>
    /// 进程的数量
    /// </summary>
    int Count { get; }

    /// <summary>
    /// 当前正在进行的进程名称
    /// </summary>
    string CurrentProcessName { get; }

    /// <summary>
    /// 是否暂停进程
    /// </summary>
    bool Pause { get; set; }

    /// <summary>
    /// 进程是否结束
    /// </summary>
    bool ProcessEnd { get; }

    /// <summary>
    /// 获取进程信息
    /// </summary>
    /// <param name="processName">进程名称</param>
    ProcessInfo GetProcessInfo(string processName);

    /// <summary>
    /// 获取锁住的进程信息
    /// </summary>
    ProcessInfo GetLockProcessInfo();

    /// <summary>
    /// 注册进程
    /// </summary>
    /// <param name="name">进程名称</param>
    /// <param name="priority">进程优先级</param>
    /// <param name="startAction">进程开始事件</param>
    void Register(string name, int priority, Action startAction);

    /// <summary>
    /// 注册进程
    /// </summary>
    /// <param name="name">进程名称</param>
    /// <param name="priority">进程优先级</param>
    /// <param name="startAction">进程开始事件</param>
    /// <param name="checkLockFunc">进程锁住</param>
    void Register(string name, int priority, Action startAction, Func<bool> checkLockFunc);

    /// <summary>
    /// 注册进程
    /// </summary>
    /// <param name="name">进程名称</param>
    /// <param name="priority">进程优先级</param>
    /// <param name="startAction">进程开始事件</param>
    /// <param name="finishAction">进程结束事件</param>
    /// <param name="checkLockFunc">进程锁住</param>
    void Register(string name, int priority, Action startAction, Action finishAction, Func<bool> checkLockFunc);

    /// <summary>
    /// 注册进程
    /// </summary>
    /// <param name="nodeName">前面的进程名称</param>
    /// <param name="name">进程名称</param>
    /// <param name="startAction">进程开始事件</param>
    void RegisterAfter(string nodeName, string name, Action startAction);

    /// <summary>
    /// 注册进程
    /// </summary>
    /// <param name="nodeName">前面的进程名称</param>
    /// <param name="name">进程名称</param>
    /// <param name="startAction">进程开始事件</param>
    /// <param name="checkLockFunc">进程锁住</param>
    void RegisterAfter(string nodeName, string name, Action startAction, Func<bool> checkLockFunc);

    /// <summary>
    /// 注册进程
    /// </summary>
    /// <param name="nodeName">前面的进程名称</param>
    /// <param name="name">进程名称</param>
    /// <param name="startAction">进程开始事件</param>
    /// <param name="finishAction">进程结束事件</param>
    /// <param name="checkLockFunc">进程锁住</param>
    void RegisterAfter(string nodeName, string name, Action startAction, Action finishAction, Func<bool> checkLockFunc);

    /// <summary>
    /// 注销进程
    /// </summary>
    /// <param name="name">进程名称</param>
    /// <returns>是否注销成功</returns>
    bool Unregister(string name);

    /// <summary>
    /// 注销全部进程
    /// </summary>
    /// <returns>注销进程的数量</returns>
    int UnregisterAll();

    /// <summary>
    /// 注销所有不锁住的进程
    /// </summary>
    /// <returns>是否有锁住的进程</returns>
    bool UnregisterAllUnlock();

    /// <summary>
    /// 执行进程
    /// </summary>
    void ExecuteProcess();

    /// <summary>
    /// 结束进程
    /// </summary>
    /// <param name="name">进程名称</param>
    /// <returns>是否结束成功</returns>
    bool EndProcess(string name);

    /// <summary>
    /// 进程开始结束callback
    /// </summary>
    /// <param name="lockByStartProgress"></param>
    /// <param name="unlockByEndProcess"></param>
    void SetProcessCallBack(Action lockByStartProgress, Action unlockByEndProcess);

    /// <summary>
    /// 是否包含某个进程
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    bool IsContainProcess(ProcessType type);
    
    /// <summary>
    /// 清理所有数据
    /// </summary>
    void Clear();
}
