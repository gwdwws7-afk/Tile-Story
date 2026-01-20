using GameFramework.Event;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 下载开始事件
/// </summary>
public sealed class DownloadStartEventArgs : GameEventArgs
{
    /// <summary>
    /// 下载开始事件编号
    /// </summary>
    public static readonly int EventId = typeof(DownloadStartEventArgs).GetHashCode();

    public DownloadStartEventArgs()
    {
        SerialId = 0;
        DownloadKey = null;
        UserData = null;
    }

    /// <summary>
    /// 获取下载开始事件编号
    /// </summary>
    public override int Id
    {
        get
        {
            return EventId;
        }
    }

    // <summary>
    /// 获取下载任务的序列编号
    /// </summary>
    public int SerialId
    {
        get;
        private set;
    }

    /// <summary>
    /// 获取下载键
    /// </summary>
    public string DownloadKey
    {
        get;
        private set;
    }

    /// <summary>
    /// 获取用户自定义数据
    /// </summary>
    public object UserData
    {
        get;
        private set;
    }

    public static DownloadStartEventArgs Create(GameFramework.Download.DownloadStartEventArgs e)
    {
        DownloadStartEventArgs downloadStartEventArgs = GameFramework.ReferencePool.Acquire<DownloadStartEventArgs>();
        downloadStartEventArgs.SerialId = e.SerialId;
        downloadStartEventArgs.DownloadKey = e.DownloadKey;
        downloadStartEventArgs.UserData = e.UserData;
        return downloadStartEventArgs;
    }

    public override void Clear()
    {
        SerialId = 0;
        DownloadKey = null;
        UserData = null;
    }
}
