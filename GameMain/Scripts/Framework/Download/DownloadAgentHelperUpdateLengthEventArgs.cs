using System.Collections;
using System.Collections.Generic;

namespace GameFramework.Download
{
    /// <summary>
    /// 下载代理辅助器更新数据大小事件
    /// </summary>
    public sealed class DownloadAgentHelperUpdateLengthEventArgs : GameFrameworkEventArgs
    {
        public DownloadAgentHelperUpdateLengthEventArgs()
        {
            DownloadedLength = 0;
            Percent = 0;
        }

        /// <summary>
        /// 获取下载的增量数据大小
        /// </summary>
        public long DownloadedLength
        {
            get;
            private set;
        }

        /// <summary>
        /// 已下载的百分比
        /// </summary>
        public float Percent
        {
            get;
            private set;
        }

        /// <summary>
        /// 创建下载代理辅助器更新数据大小事件
        /// </summary>
        /// <param name="deltaLength">下载的增量数据大小</param>
        /// <returns>创建的下载代理辅助器更新数据大小事件</returns>
        public static DownloadAgentHelperUpdateLengthEventArgs Create(long deltaLength, float percent)
        {
            if (deltaLength <= 0)
            {
                Log.Warning("Delta length is invalid.{0}", deltaLength);
            }

            DownloadAgentHelperUpdateLengthEventArgs downloadAgentHelperUpdateLengthEventArgs = ReferencePool.Acquire<DownloadAgentHelperUpdateLengthEventArgs>();
            downloadAgentHelperUpdateLengthEventArgs.DownloadedLength = deltaLength;
            downloadAgentHelperUpdateLengthEventArgs.Percent = percent;
            return downloadAgentHelperUpdateLengthEventArgs;
        }

        /// <summary>
        /// 清理下载代理辅助器更新数据大小事件
        /// </summary>
        public override void Clear()
        {
            DownloadedLength = 0;
            Percent = 0;
        }
    }
}