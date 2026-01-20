using System.Runtime.InteropServices;

/// <summary>
/// 任务信息
/// </summary>
[StructLayout(LayoutKind.Auto)]
public struct TaskInfo
{
    private readonly bool isValid;
    private readonly int serialId;
    private readonly string tag;
    private readonly int priority;
    private readonly object userData;
    private readonly TaskStatus status;
    private readonly string description;

    /// <summary>
    /// 初始化任务信息的新实例
    /// </summary>
    /// <param name="serialId">任务的序列编号</param>
    /// <param name="tag">任务的标签</param>
    /// <param name="priority">任务的优先级</param>
    /// <param name="userData">任务的用户自定义数据</param>
    /// <param name="status">任务状态</param>
    /// <param name="description">任务描述</param>
    public TaskInfo(int serialId, string tag, int priority, object userData, TaskStatus status, string description)
    {
        isValid = true;
        this.serialId = serialId;
        this.tag = tag;
        this.priority = priority;
        this.userData = userData;
        this.status = status;
        this.description = description;
    }

    /// <summary>
    /// 获取任务信息是否有效
    /// </summary>
    public bool IsValid
    {
        get
        {
            return isValid;
        }
    }

    /// <summary>
    /// 获取任务的序列编号
    /// </summary>
    public int SerialId
    {
        get
        {
            if (!isValid)
            {
                throw new System.Exception("Data is invalid.");
            }

            return serialId;
        }
    }

    /// <summary>
    /// 获取任务的标签
    /// </summary>
    public string Tag
    {
        get
        {
            if (!isValid)
            {
                throw new System.Exception("Data is invalid.");
            }

            return tag;
        }
    }

    /// <summary>
    /// 获取任务的优先级
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
    /// 获取任务的用户自定义数据
    /// </summary>
    public object UserData
    {
        get
        {
            if (!isValid)
            {
                throw new System.Exception("Data is invalid.");
            }

            return userData;
        }
    }

    /// <summary>
    /// 获取任务状态
    /// </summary>
    public TaskStatus Status
    {
        get
        {
            if (!isValid)
            {
                throw new System.Exception("Data is invalid.");
            }

            return status;
        }
    }

    /// <summary>
    /// 获取任务描述
    /// </summary>
    public string Description
    {
        get
        {
            if (!isValid)
            {
                throw new System.Exception("Data is invalid.");
            }

            return description;
        }
    }
}
