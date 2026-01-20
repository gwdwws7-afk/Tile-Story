using System;
using System.Collections;
using System.Collections.Generic;

namespace GameFramework.Download
{
    /// <summary>
    /// 下载辅助器接口
    /// </summary>
    public interface IDownloadHelper
    {
        void GetDownloadSize(string downloadKey, Action<long> successCallback, Action<string> failCallback);
    }
}