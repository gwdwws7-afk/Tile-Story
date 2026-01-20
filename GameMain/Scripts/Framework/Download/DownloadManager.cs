using System;
using System.Collections;
using System.Collections.Generic;

namespace GameFramework.Download
{
    /// <summary>
    /// 下载管理器
    /// </summary>
    public sealed partial class DownloadManager : GameFrameworkModule, IDownloadManager
    {
        private readonly TaskPool<DownloadTask> m_TaskPool;
        private readonly Dictionary<string, int> m_AssetDownloadingDic;
        private float m_Timeout;
        private EventHandler<DownloadStartEventArgs> m_DownloadStartEventHandler;
        private EventHandler<DownloadUpdateEventArgs> m_DownloadUpdateEventHandler;
        private EventHandler<DownloadSuccessEventArgs> m_DownloadSuccessEventHandler;
        private EventHandler<DownloadFailureEventArgs> m_DownloadFailureEventHandler;

        public DownloadManager()
        {
            m_TaskPool = new TaskPool<DownloadTask>();
            m_AssetDownloadingDic = new Dictionary<string, int>();
            m_Timeout = 30f;
            m_DownloadStartEventHandler = null;
            m_DownloadUpdateEventHandler = null;
            m_DownloadSuccessEventHandler = null;
            m_DownloadFailureEventHandler = null;
        }

        /// <summary>
        /// 获取下载代理总数量
        /// </summary>
        public int TotalAgentCount
        {
            get
            {
                return m_TaskPool.TotalAgentCount;
            }
        }

        /// <summary>
        /// 获取可用下载代理数量
        /// </summary>
        public int FreeAgentCount
        {
            get
            {
                return m_TaskPool.FreeAgentCount;
            }
        }

        /// <summary>
        /// 获取工作中下载代理数量
        /// </summary>
        public int WorkingAgentCount
        {
            get
            {
                return m_TaskPool.WorkingAgentCount;
            }
        }

        /// <summary>
        /// 获取等待下载任务数量
        /// </summary>
        public int WaitingTaskCount
        {
            get
            {
                return m_TaskPool.WaitingTaskCount;
            }
        }

        /// <summary>
        /// 获取或设置下载超时时长，以秒为单位
        /// </summary>
        public float Timeout
        {
            get
            {
                return m_Timeout;
            }
            set
            {
                m_Timeout = value;
            }
        }

        /// <summary>
        /// 下载开始事件
        /// </summary>
        public event EventHandler<DownloadStartEventArgs> DownloadStart
        {
            add
            {
                m_DownloadStartEventHandler += value;
            }
            remove
            {
                m_DownloadStartEventHandler -= value;
            }
        }

        /// <summary>
        /// 下载更新事件
        /// </summary>
        public event EventHandler<DownloadUpdateEventArgs> DownloadUpdate
        {
            add
            {
                m_DownloadUpdateEventHandler += value;
            }
            remove
            {
                m_DownloadUpdateEventHandler -= value;
            }
        }

        /// <summary>
        /// 下载成功事件
        /// </summary>
        public event EventHandler<DownloadSuccessEventArgs> DownloadSuccess
        {
            add
            {
                m_DownloadSuccessEventHandler += value;
            }
            remove
            {
                m_DownloadSuccessEventHandler -= value;
            }
        }

        /// <summary>
        /// 下载失败事件
        /// </summary>
        public event EventHandler<DownloadFailureEventArgs> DownloadFailure
        {
            add
            {
                m_DownloadFailureEventHandler += value;
            }
            remove
            {
                m_DownloadFailureEventHandler -= value;
            }
        }

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            m_TaskPool.Update(elapseSeconds, realElapseSeconds);
        }

        public override void Shutdown()
        {
            m_TaskPool.Shutdown();
        }

        /// <summary>
        /// 增加下载代理辅助器
        /// </summary>
        /// <param name="downloadAgentHelper">要增加的下载代理辅助器</param>
        public void AddDownloadAgentHelper(IDownloadAgentHelper downloadAgentHelper)
        {
            DownloadAgent agent = new DownloadAgent(downloadAgentHelper);
            agent.DownloadAgentStart += OnDownloadAgentStart;
            agent.DownloadAgentUpdate += OnDownloadAgentUpdate;
            agent.DownloadAgentSuccess += OnDownloadAgentSuccess;
            agent.DownloadAgentFailure += OnDownloadAgentFailure;

            m_TaskPool.AddAgent(agent);
        }

        /// <summary>
        /// 根据下载任务的序列编号获取下载任务的信息
        /// </summary>
        /// <param name="serialId">要获取信息的下载任务的序列编号</param>
        /// <returns>下载任务的信息</returns>
        public TaskInfo GetDownloadInfo(int serialId)
        {
            return m_TaskPool.GetTaskInfo(serialId);
        }

        /// <summary>
        /// 根据下载任务的标签获取下载任务的信息
        /// </summary>
        /// <param name="tag">要获取信息的下载任务的标签</param>
        /// <returns>下载任务的信息</returns>
        public TaskInfo[] GetDownloadInfos(string tag)
        {
            return m_TaskPool.GetTaskInfos(tag);
        }

        /// <summary>
        /// 是否正在下载
        /// </summary>
        /// <param name="downloadKey">下载键</param>
        public bool IsDownloading(string downloadKey)
        {
            if (m_AssetDownloadingDic.TryGetValue(downloadKey, out int id))
            {
                TaskInfo info = GetDownloadInfo(id);
                if (info.IsValid)
                {
                    if (info.Status == TaskStatus.Done)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 增加下载任务
        /// </summary>
        /// <param name="downloadKey">下载键</param>
        /// <returns>下载任务序号</returns>
        public int AddDownload(string downloadKey)
        {
            return AddDownload(downloadKey, null, 0, null);
        }

        /// <summary>
        /// 增加下载任务
        /// </summary>
        /// <param name="downloadKey">下载键</param>
        /// <param name="tag">下载标签</param>
        /// <param name="priority">下载优先级</param>
        /// <returns>下载任务序号</returns>
        public int AddDownload(string downloadKey, string tag, int priority)
        {
            return AddDownload(downloadKey, tag, priority, null);
        }

        /// <summary>
        /// 增加下载任务
        /// </summary>
        /// <param name="downloadKey">下载键</param>
        /// <param name="tag">下载标签</param>
        /// <param name="priority">下载优先级</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>下载任务序号</returns>
        public int AddDownload(string downloadKey, string tag, int priority, object userData)
        {
            if (string.IsNullOrEmpty(downloadKey))
            {
                throw new Exception("Download Key is invalid.");
            }

            if (TotalAgentCount <= 0)
            {
                throw new Exception("You must add download agent first.");
            }

            DownloadTask downloadTask = DownloadTask.Create(downloadKey, tag, priority, m_Timeout, userData);
            m_TaskPool.AddTask(downloadTask);

            m_AssetDownloadingDic[downloadKey] = downloadTask.SerialId;

            return downloadTask.SerialId;
        }

        /// <summary>
        /// 根据下载任务的序列编号移除下载任务
        /// </summary>
        /// <param name="serialId">要移除下载任务的序列编号</param>
        /// <returns>是否移除下载任务成功</returns>
        public bool RemoveDownload(int serialId)
        {
            return m_TaskPool.RemoveTask(serialId);
        }

        /// <summary>
        /// 根据下载任务的标签移除下载任务
        /// </summary>
        /// <param name="tag">要移除下载任务的标签</param>
        /// <returns>移除下载任务的数量</returns>
        public int RemoveDownloads(string tag)
        {
            return m_TaskPool.RemoveTasks(tag);
        }

        /// <summary>
        /// 移除所有下载任务
        /// </summary>
        /// <returns>移除下载任务的数量</returns>
        public int RemoveAllDownloads()
        {
            return m_TaskPool.RemoveAllTasks();
        }

        private void OnDownloadAgentStart(DownloadAgent sender)
        {
            if (m_DownloadStartEventHandler != null)
            {
                DownloadStartEventArgs downloadStartEventArgs = DownloadStartEventArgs.Create(sender.Task.SerialId, sender.Task.DownloadKey, sender.Task.UserData);
                m_DownloadStartEventHandler(this, downloadStartEventArgs);
                ReferencePool.Release(downloadStartEventArgs);
            }
        }

        private void OnDownloadAgentUpdate(DownloadAgent sender, long downloadedLength, float percent)
        {
            //m_DownloadCounter.RecordDeltaLength(deltaLength);
            if (m_DownloadUpdateEventHandler != null)
            {
                DownloadUpdateEventArgs downloadUpdateEventArgs = DownloadUpdateEventArgs.Create(sender.Task.SerialId, sender.Task.DownloadKey, downloadedLength, percent, sender.Task.UserData);
                m_DownloadUpdateEventHandler(this, downloadUpdateEventArgs);
                ReferencePool.Release(downloadUpdateEventArgs);
            }
        }

        private void OnDownloadAgentSuccess(DownloadAgent sender, long length)
        {
            if (m_DownloadSuccessEventHandler != null)
            {
                DownloadSuccessEventArgs downloadSuccessEventArgs = DownloadSuccessEventArgs.Create(sender.Task.SerialId, sender.Task.DownloadKey, sender.Task.UserData);
                m_DownloadSuccessEventHandler(this, downloadSuccessEventArgs);
                ReferencePool.Release(downloadSuccessEventArgs);
            }
        }

        private void OnDownloadAgentFailure(DownloadAgent sender, string errorMessage)
        {
            if (m_DownloadFailureEventHandler != null)
            {
                DownloadFailureEventArgs downloadFailureEventArgs = DownloadFailureEventArgs.Create(sender.Task.SerialId, sender.Task.DownloadKey, errorMessage, sender.Task.UserData);
                m_DownloadFailureEventHandler(this, downloadFailureEventArgs);
                ReferencePool.Release(downloadFailureEventArgs);
            }
        }
    }
}
