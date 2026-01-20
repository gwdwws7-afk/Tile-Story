using GameFramework;
using System;

/// <summary>
/// 对象池对象基类
/// </summary>
public abstract class ObjectBase : IReference
{
    private string name;
    private object target;
    private DateTime lastUseTime;

    protected ObjectBase()
    {
        name = null;
        target = null;
        lastUseTime = default;
    }

    /// <summary>
    /// 获取对象名称
    /// </summary>
    public string Name { get => name; }

    /// <summary>
    /// 获取对象
    /// </summary>
    public object Target { get => target; }

    /// <summary>
    /// 获取对象上次使用时间
    /// </summary>
    public DateTime LastUseTime { get => lastUseTime; internal set => lastUseTime = value; }

    /// <summary>
    /// 初始化对象基类
    /// </summary>
    /// <param name="name">名称</param>
    /// <param name="target">对象</param>
    public void Initialize(string name, object target)
    {
        if (string.IsNullOrEmpty(name))
        {
            Log.Error("Object key is invalid.");
        }

        this.name = name;
        this.target = target;
        lastUseTime = DateTime.UtcNow;
    }

    /// <summary>
    /// 清理对象基类
    /// </summary>
    public virtual void Clear()
    {
        name = null;
        target = null;
        lastUseTime = default;
    }

    /// <summary>
    /// 获取对象时的事件
    /// </summary>
    protected internal virtual void OnSpawn()
    {
    }

    /// <summary>
    /// 回收对象时的事件
    /// </summary>
    protected internal virtual void OnUnspawn()
    {
    }

    /// <summary>
    /// 释放对象
    /// </summary>
    /// <param name="isShutdown">是否是关闭对象池触发</param>
    public abstract void Release(bool isShutdown);
}
