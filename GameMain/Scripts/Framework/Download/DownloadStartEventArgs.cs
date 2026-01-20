using System.Collections;
using System.Collections.Generic;

namespace GameFramework.Download
{
    /// <summary>
    /// 下载开始事件
    /// </summary>
    public sealed class DownloadStartEventArgs : GameFrameworkEventArgs
    {
        public DownloadStartEventArgs()
        {
            SerialId = 0;
            DownloadKey = null;
            UserData = null;
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

        /// <summary>
        /// 创建下载开始事件
        /// </summary>
        /// <param name="serialId">下载任务的序列编号。</param>
        /// <param name="downloadKey">下载地址。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>创建的下载开始事件。</returns>
        public static DownloadStartEventArgs Create(int serialId, string downloadKey, object userData)
        {
            DownloadStartEventArgs downloadStartEventArgs = ReferencePool.Acquire<DownloadStartEventArgs>();
            downloadStartEventArgs.SerialId = serialId;
            downloadStartEventArgs.DownloadKey = downloadKey;
            downloadStartEventArgs.UserData = userData;
            return downloadStartEventArgs;
        }

        /// <summary>
        /// 清理下载开始事件。
        /// </summary>
        public override void Clear()
        {
            SerialId = 0;
            DownloadKey = null;
            UserData = null;
        }
    }
}