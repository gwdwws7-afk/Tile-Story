using System.Collections;
using System.Collections.Generic;

namespace GameFramework.Download
{
    /// <summary>
    /// 下载更新事件
    /// </summary>
    public sealed class DownloadUpdateEventArgs : GameFrameworkEventArgs
    {
        /// <summary>
        /// 初始化下载更新事件的新实例。
        /// </summary>
        public DownloadUpdateEventArgs()
        {
            SerialId = 0;
            DownloadKey = null;
            DownloadedLength = 0L;
            Percent = 0f;
            UserData = null;
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

        /// <summary>
        /// 创建下载更新事件
        /// </summary>
        /// <param name="serialId">下载任务的序列编号</param>
        /// <param name="downloadKey">下载键</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>创建的下载更新事件</returns>
        public static DownloadUpdateEventArgs Create(int serialId, string downloadKey, long downloadedLength, float percent, object userData)
        {
            DownloadUpdateEventArgs downloadUpdateEventArgs = ReferencePool.Acquire<DownloadUpdateEventArgs>();
            downloadUpdateEventArgs.SerialId = serialId;
            downloadUpdateEventArgs.DownloadKey = downloadKey;
            downloadUpdateEventArgs.DownloadedLength = downloadedLength;
            downloadUpdateEventArgs.Percent = percent;
            downloadUpdateEventArgs.UserData = userData;
            return downloadUpdateEventArgs;
        }

        /// <summary>
        /// 清理下载更新事件。
        /// </summary>
        public override void Clear()
        {
            SerialId = 0;
            DownloadKey = null;
            DownloadedLength = 0L;
            Percent = 0f;
            UserData = null;
        }
    }
}