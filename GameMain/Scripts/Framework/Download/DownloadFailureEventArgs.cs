using System.Collections;
using System.Collections.Generic;

namespace GameFramework.Download
{
    /// <summary>
    /// 下载失败事件
    /// </summary>
    public sealed class DownloadFailureEventArgs : GameFrameworkEventArgs
    {
        public DownloadFailureEventArgs()
        {
            SerialId = 0;
            DownloadKey = null;
            ErrorMessage = null;
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

        /// <summary>
        /// 创建下载失败事件
        /// </summary>
        /// <param name="serialId">下载任务的序列编号</param>
        /// <param name="downloadKey">下载键</param>
        /// <param name="errorMessage">错误信息</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>创建的下载失败事件</returns>
        public static DownloadFailureEventArgs Create(int serialId, string downloadKey, string errorMessage, object userData)
        {
            DownloadFailureEventArgs downloadFailureEventArgs = ReferencePool.Acquire<DownloadFailureEventArgs>();
            downloadFailureEventArgs.SerialId = serialId;
            downloadFailureEventArgs.DownloadKey = downloadKey;
            downloadFailureEventArgs.ErrorMessage = errorMessage;
            downloadFailureEventArgs.UserData = userData;
            return downloadFailureEventArgs;
        }

        /// <summary>
        /// 清理下载失败事件。
        /// </summary>
        public override void Clear()
        {
            SerialId = 0;
            DownloadKey = null;
            ErrorMessage = null;
            UserData = null;
        }
    }
}