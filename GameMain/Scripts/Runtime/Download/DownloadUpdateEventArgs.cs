using GameFramework.Event;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 下载更新事件
/// </summary>
public sealed class DownloadUpdateEventArgs : GameEventArgs
{
    /// <summary>
    /// 下载更新事件编号
    /// </summary>
    public static readonly int EventId = typeof(DownloadUpdateEventArgs).GetHashCode();

    public DownloadUpdateEventArgs()
    {
        SerialId = 0;
        DownloadKey = null;
        DownloadedLength = 0L;
        Percent = 0f;
        UserData = null;
    }

    /// <summary>
    /// 获取下载更新事件编号
    /// </summary>
    public override int Id
    {
        get
        {
            return EventId;
        }
    }

    /// <summary>
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
    /// 获取已经下载的大小
    /// </summary>
    public long DownloadedLength
    {
        get;
        private set;
    }

    /// <summary>
    /// 下载进度百分比
    /// </summary>
    public float Percent
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

    public static DownloadUpdateEventArgs Create(GameFramework.Download.DownloadUpdateEventArgs e)
    {
        DownloadUpdateEventArgs downloadUpdateEventArgs = GameFramework.ReferencePool.Acquire<DownloadUpdateEventArgs>();
        downloadUpdateEventArgs.SerialId = e.SerialId;
        downloadUpdateEventArgs.DownloadKey = e.DownloadKey;
        downloadUpdateEventArgs.DownloadedLength = e.DownloadedLength;
        downloadUpdateEventArgs.Percent = e.Percent;
        downloadUpdateEventArgs.UserData = e.UserData;
        return downloadUpdateEventArgs;
    }

    public override void Clear()
    {
        SerialId = 0;
        DownloadKey = null;
        DownloadedLength = 0L;
        Percent = 0f;
        UserData = null;
    }
}
