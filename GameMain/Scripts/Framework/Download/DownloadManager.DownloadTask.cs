using System.Collections;
using System.Collections.Generic;

namespace GameFramework.Download
{
    public sealed partial class DownloadManager : GameFrameworkModule, IDownloadManager
    {
        /// <summary>
        /// 下载任务
        /// </summary>
        private sealed class DownloadTask : TaskBase
        {
            private static int s_Serial = 0;

            private DownloadTaskStatus m_Status;
            private string m_DownloadKey;
            private float m_Timeout;

            public DownloadTask()
            {
                m_Status = DownloadTaskStatus.Todo;
                m_DownloadKey = null;
                m_Timeout = 0;
            }

            /// <summary>
            /// 获取或设置下载任务的状态
            /// </summary>
            public DownloadTaskStatus Status
            {
                get
                {
                    return m_Status;
                }
                set
                {
                    m_Status = value;
                }
            }

            /// <summary>
            /// 下载键
            /// </summary>
            public string DownloadKey
            {
                get
                {
                    return m_DownloadKey;
                }
            }

            /// <summary>
            /// 获取下载超时时长，以秒为单位
            /// </summary>
            public float Timeout
            {
                get
                {
                    return m_Timeout;
                }
            }

            public static DownloadTask Create(string downloadKey, string tag, int priority, float timeout, object userData)
            {
                DownloadTask downloadTask = ReferencePool.Acquire<DownloadTask>();
                downloadTask.Initialize(++s_Serial, tag, priority, userData);
                downloadTask.m_DownloadKey = downloadKey;
                downloadTask.m_Timeout = timeout;
                return downloadTask;
            }

            /// <summary>
            /// 清理下载任务。
            /// </summary>
            public override void Clear()
            {
                base.Clear();
                m_Status = DownloadTaskStatus.Todo;
                m_DownloadKey = null;
                m_Timeout = 0f;
            }
        }
    }
}
