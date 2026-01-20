using System;
using System.Collections;
using System.Collections.Generic;

namespace GameFramework.Download
{
    public sealed partial class DownloadManager : GameFrameworkModule, IDownloadManager
    {
        /// <summary>
        /// 下载代理
        /// </summary>
        private sealed class DownloadAgent : ITaskAgent<DownloadTask>
        {
            private readonly IDownloadAgentHelper m_Helper;
            private DownloadTask m_Task;
            private float m_WaitTime;
            private long m_DownloadedLength;

            public Action<DownloadAgent> DownloadAgentStart;
            public Action<DownloadAgent, long, float> DownloadAgentUpdate;
            public Action<DownloadAgent, long> DownloadAgentSuccess;
            public Action<DownloadAgent, string> DownloadAgentFailure;

            public DownloadAgent(IDownloadAgentHelper downloadAgentHelper)
            {
                m_Helper = downloadAgentHelper;
                m_Task = null;
                m_WaitTime = 0f;
                m_DownloadedLength = 0L;

                DownloadAgentStart = null;
                DownloadAgentUpdate = null;
                DownloadAgentSuccess = null;
                DownloadAgentFailure = null;
            }

            /// <summary>
            /// 获取下载任务
            /// </summary>
            public DownloadTask Task
            {
                get
                {
                    return m_Task;
                }
            }

            /// <summary>
            /// 获取已经等待时间
            /// </summary>
            public float WaitTime
            {
                get
                {
                    return m_WaitTime;
                }
            }

            /// <summary>
            /// 获取本次已经下载的大小
            /// </summary>
            public long DownloadedLength
            {
                get
                {
                    return m_DownloadedLength;
                }
            }

            public void Initialize()
            {
                m_Helper.DownloadAgentHelperUpdateLength += OnDownloadAgentHelperUpdateLength;
                m_Helper.DownloadAgentHelperComplete += OnDownloadAgentHelperComplete;
                m_Helper.DownloadAgentHelperError += OnDownloadAgentHelperError;
            }

            public void Update(float elapseSeconds, float realElapseSeconds)
            {
                if (m_Task.Status == DownloadTaskStatus.Doing)
                {
                    m_WaitTime += realElapseSeconds;
                    if (m_WaitTime >= m_Task.Timeout)
                    {
                        DownloadAgentHelperErrorEventArgs downloadAgentHelperErrorEventArgs = DownloadAgentHelperErrorEventArgs.Create("Timeout");
                        OnDownloadAgentHelperError(this, downloadAgentHelperErrorEventArgs);
                        ReferencePool.Release(downloadAgentHelperErrorEventArgs);
                    }
                }
            }

            public void Shutdown()
            {
                m_Helper.DownloadAgentHelperUpdateLength -= OnDownloadAgentHelperUpdateLength;
                m_Helper.DownloadAgentHelperComplete -= OnDownloadAgentHelperComplete;
                m_Helper.DownloadAgentHelperError -= OnDownloadAgentHelperError;
            }

            public StartTaskStatus Start(DownloadTask task)
            {
                if (task == null)
                {
                    throw new Exception("Task is invalid.");
                }

                m_Task = task;
                m_Task.Status = DownloadTaskStatus.Doing;

                try
                {
                    DownloadAgentStart?.Invoke(this);

                    m_Helper.Download(m_Task.DownloadKey, m_Task.UserData);
                }
                catch(Exception e)
                {
                    Log.Error("Download Task Start Error.Message {0}", e.Message);

                    m_Task.Status = DownloadTaskStatus.Error;
                    return StartTaskStatus.UnknownError;
                }

                return StartTaskStatus.CanResume;
            }

            public void Reset()
            {
                m_Helper.OnReset();

                m_Task = null;
                m_WaitTime = 0f;
                m_DownloadedLength = 0L;
            }

            private void OnDownloadAgentHelperUpdateLength(object sender, DownloadAgentHelperUpdateLengthEventArgs e)
            {
                m_WaitTime = 0f;
                m_DownloadedLength += e.DownloadedLength;
                DownloadAgentUpdate?.Invoke(this, e.DownloadedLength, e.Percent);
            }

            private void OnDownloadAgentHelperComplete(object sender, DownloadAgentHelperCompleteEventArgs e)
            {
                m_WaitTime = 0f;
                m_DownloadedLength = e.Length;

                m_Helper.OnReset();

                m_Task.Status = DownloadTaskStatus.Done;

                DownloadAgentSuccess?.Invoke(this, e.Length);

                m_Task.Done = true;
            }

            private void OnDownloadAgentHelperError(object sender, DownloadAgentHelperErrorEventArgs e)
            {
                m_Helper.OnReset();

                m_Task.Status = DownloadTaskStatus.Error;

                DownloadAgentFailure?.Invoke(this, e.ErrorMessage);

                m_Task.Done = true;
            }
        }
    }
}
