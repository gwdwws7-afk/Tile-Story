using System;
using System.Collections;
using System.Collections.Generic;

namespace GameFramework.Download
{
    /// <summary>
    /// 下载代理辅助器接口
    /// </summary>
    public interface IDownloadAgentHelper
    {
        /// <summary>
        /// 下载代理辅助器更新数据大小
        /// </summary>
        event EventHandler<DownloadAgentHelperUpdateLengthEventArgs> DownloadAgentHelperUpdateLength;

        /// <summary>
        /// 下载代理辅助器完成事件
        /// </summary>
        event EventHandler<DownloadAgentHelperCompleteEventArgs> DownloadAgentHelperComplete;

        /// <summary>
        /// 下载代理辅助器错误事件
        /// </summary>
        event EventHandler<DownloadAgentHelperErrorEventArgs> DownloadAgentHelperError;

        /// <summary>
        /// 通过下载代理辅助器下载指定地址的数据
        /// </summary>
        /// <param name="downloadUri">下载地址</param>
        /// <param name="userData">用户自定义数据</param>
        void Download(string downloadUri, object userData);

        /// <summary>
        /// 重置下载代理辅助器
        /// </summary>
        void OnReset();
    }
}