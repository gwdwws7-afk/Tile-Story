using System;
using UnityEngine;

/// <summary>
/// 活动管理器基类
/// </summary>
public abstract class ActivityManagerBase : MonoBehaviour, IActivityProcess
{
    private bool m_IsInitialized = false;

    /// <summary>
    /// 活动编号
    /// </summary>
    public abstract int ActivityID { get; }

    /// <summary>
    /// 活动初始化
    /// </summary>
    public void Initialize()
    {
        if (m_IsInitialized)
            return;
        m_IsInitialized = true;

#if UNITY_EDITOR
        OnInitialize();
#else
        try
        {
            OnInitialize();
        }
        catch(Exception e)
        {
            Log.Error("Activity ID {0} initialize error - {1}", ActivityID, e.Message);
        }
#endif
    }

    /// <summary>
    /// 活动关闭
    /// </summary>
    public void Shutdown()
    {
        if (!m_IsInitialized)
            return;
        m_IsInitialized = false;

#if UNITY_EDITOR
        OnShutdown();
#else
        try
        {
            OnShutdown();
        }
        catch (Exception e)
        {
            Log.Error("Activity ID {0} shutdown error - {1}", ActivityID, e.Message);
        }
#endif
    }

    /// <summary>
    /// 检测活动是否初始化成功
    /// </summary>
    public virtual bool CheckInitializationComplete()
    {
        return m_IsInitialized;
    }

    /// <summary>
    /// 当活动初始化时
    /// </summary>
    protected virtual void OnInitialize() { }

    /// <summary>
    /// 当活动关闭时
    /// </summary>
    protected virtual void OnShutdown() { }

    /// <summary>
    /// 检测活动是否可以开启
    /// </summary>
    public abstract bool CheckActivityCanStart();

    /// <summary>
    /// 检测活动是否已经开启
    /// </summary>
    public abstract bool CheckActivityHasStarted();

    /// <summary>
    /// 活动开始流程
    /// </summary>
    public abstract void ActivityStartProcess();

    /// <summary>
    /// 活动结束流程
    /// </summary>
    public abstract void ActivityEndProcess();

    /// <summary>
    /// 活动结束前的流程
    /// </summary>
    public abstract void ActivityPreEndProcess();

    /// <summary>
    /// 活动开启后的流程
    /// </summary>
    public abstract void ActivityAfterStartProcess();

    /// <summary>
    /// 当关卡胜利时
    /// </summary>
    /// <param name="levelFailTime">关卡失败次数</param>
    public abstract void OnLevelWin(int levelFailTime, int hardIndex);

    /// <summary>
    /// 当关卡失败时
    /// </summary>
    public abstract void OnLevelLose();
}
