using GameFramework.Event;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 下载成功事件
/// </summary>
public sealed class DownloadSuccessEventArgs : GameEventArgs
{
    /// <summary>
    /// 下载成功事件编号
    /// </summary>
    public static readonly int EventId = typeof(DownloadSuccessEventArgs).GetHashCode();

    public DownloadSuccessEventArgs()
    {
        SerialId = 0;
        DownloadKey = null;
        UserData = null;
    }

    /// <summary>
    /// 获取下载成功事件编号
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
    /// 获取用户自定义数据
    /// </summary>
    public object UserData
    {
        get;
        private set;
    }

    public static DownloadSuccessEventArgs Create(GameFramework.Download.DownloadSuccessEventArgs e)
    {
        DownloadSuccessEventArgs downloadSuccessEventArgs = GameFramework.ReferencePool.Acquire<DownloadSuccessEventArgs>();
        downloadSuccessEventArgs.SerialId = e.SerialId;
        downloadSuccessEventArgs.DownloadKey = e.DownloadKey;
        downloadSuccessEventArgs.UserData = e.UserData;
        return downloadSuccessEventArgs;
    }

    public override void Clear()
    {
        SerialId = 0;
        DownloadKey = null;
        UserData = null;
    }
}
