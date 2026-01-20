using GameFramework.Event;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 下载失败事件
/// </summary>
public sealed class DownloadFailureEventArgs : GameEventArgs
{
    /// <summary>
    /// 下载失败事件编号
    /// </summary>
    public static readonly int EventId = typeof(DownloadFailureEventArgs).GetHashCode();

    public DownloadFailureEventArgs()
    {
        SerialId = 0;
        DownloadKey = null;
        ErrorMessage = null;
        UserData = null;
    }

    /// <summary>
    /// 获取下载失败事件编号
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
    /// 获取错误信息
    /// </summary>
    public string ErrorMessage
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

    public static DownloadFailureEventArgs Create(GameFramework.Download.DownloadFailureEventArgs e)
    {
        DownloadFailureEventArgs downloadFailureEventArgs = GameFramework.ReferencePool.Acquire<DownloadFailureEventArgs>();
        downloadFailureEventArgs.SerialId = e.SerialId;
        downloadFailureEventArgs.DownloadKey = e.DownloadKey;
        downloadFailureEventArgs.ErrorMessage = e.ErrorMessage;
        downloadFailureEventArgs.UserData = e.UserData;
        return downloadFailureEventArgs;
    }

    public override void Clear()
    {
        SerialId = 0;
        DownloadKey = null;
        ErrorMessage = null;
        UserData = null;
    }
}
