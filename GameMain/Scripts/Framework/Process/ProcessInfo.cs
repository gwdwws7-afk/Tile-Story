using System.Runtime.InteropServices;

/// <summary>
/// 进程信息
/// </summary>
[StructLayout(LayoutKind.Auto)]
public struct ProcessInfo
{
    private readonly bool isValid;
    private readonly string processName;
    private readonly int priority;
    private readonly ProcessStatus processStatus;
    private readonly bool isLock;

    public ProcessInfo(string processName, int priority, ProcessStatus processStatus, bool isLock)
    {
        isValid = true;
        this.processName = processName;
        this.priority = priority;
        this.processStatus = processStatus;
        this.isLock = isLock;
    }

    /// <summary>
    /// 进程信息是否有效
    /// </summary>
    public bool IsValid
    {
        get
        {
            return isValid;
        }
    }

    /// <summary>
    /// 进程名称
    /// </summary>
    public string ProcessName
    {
        get
        {
            return processName;
        }
    }

    /// <summary>
    /// 进程的优先级
    /// </summary>
    public int Priority
    {
        get
        {
            if (!isValid)
            {
                throw new System.Exception("Data is invalid.");
            }

            return priority;
        }
    }

    /// <summary>
    /// 进程状态
    /// </summary>
    public ProcessStatus ProcessStatus
    {
        get
        {
            return processStatus;
        }
    }

    /// <summary>
    /// 进程是否上锁
    /// </summary>
    public bool IsLock
    {
        get
        {
            return isLock;
        }
    }
}
