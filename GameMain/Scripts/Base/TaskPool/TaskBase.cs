using GameFramework;

/// <summary>
/// 任务基类
/// </summary>
public abstract class TaskBase : IReference
{
    /// <summary>
    /// 任务默认优先级
    /// </summary>
    public const int DefaultPriority = 0;

    private int serialId;
    private string tag;
    private int priority;
    private object userData;

    private bool done;

    public TaskBase()
    {
        serialId = 0;
        tag = null;
        priority = DefaultPriority;
        done = false;
        userData = null;
    }

    /// <summary>
    /// 获取任务的序列编号。
    /// </summary>
    public int SerialId { get => serialId; }

    /// <summary>
    /// 获取任务的标签。
    /// </summary>
    public string Tag { get => tag; }

    /// <summary>
    /// 获取任务的优先级。
    /// </summary>
    public int Priority { get => priority; }

    /// <summary>
    /// 获取任务的用户自定义数据。
    /// </summary>
    public object UserData { get => userData; }

    /// <summary>
    /// 获取或设置任务是否完成。
    /// </summary>
    public bool Done { get => done; set => done = value; }

    /// <summary>
    /// 获取任务描述
    /// </summary>
    public virtual string Description { get => null; }

    /// <summary>
    /// 初始化任务基类。
    /// </summary>
    /// <param name="serialId">任务的序列编号。</param>
    /// <param name="tag">任务的标签。</param>
    /// <param name="priority">任务的优先级。</param>
    /// <param name="userData">任务的用户自定义数据。</param>
    public void Initialize(int serialId, string tag, int priority, object userData)
    {
        this.serialId = serialId;
        this.tag = tag;
        this.priority = priority;
        this.userData = userData;
        done = false;
    }

    /// <summary>
    /// 清理任务基类。
    /// </summary>
    public virtual void Clear()
    {
        serialId = 0;
        tag = null;
        priority = DefaultPriority;
        userData = null;
        done = false;
    }
}
