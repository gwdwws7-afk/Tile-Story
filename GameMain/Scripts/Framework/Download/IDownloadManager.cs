using System;

namespace GameFramework.Download
{
    public interface IDownloadManager
    {
        int FreeAgentCount { get; }
        int WorkingAgentCount { get; }
        float Timeout { get; set; }
        int WaitingTaskCount { get; }
        int TotalAgentCount { get; }

        event EventHandler<DownloadSuccessEventArgs> DownloadSuccess;
        event EventHandler<DownloadFailureEventArgs> DownloadFailure;
        event EventHandler<DownloadStartEventArgs> DownloadStart;
        event EventHandler<DownloadUpdateEventArgs> DownloadUpdate;

        void AddDownloadAgentHelper(IDownloadAgentHelper downloadAgentHelper);

        TaskInfo GetDownloadInfo(int serialId);

        TaskInfo[] GetDownloadInfos(string tag);

        bool IsDownloading(string downloadKey);

        int AddDownload(string downloadKey);

        int AddDownload(string downloadKey, string tag, int priority);

        int AddDownload(string downloadKey, string tag, int priority, object userData);

        bool RemoveDownload(int serialId);

        int RemoveDownloads(string tag);

        int RemoveAllDownloads();
    }
}