using GameFramework;
using GameFramework.Download;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// 默认下载代理辅助器
/// </summary>
public class DefaultDownloadAgentHelper : MonoBehaviour, IDownloadAgentHelper
{
    private AsyncOperationHandle handle;

    private EventHandler<DownloadAgentHelperUpdateLengthEventArgs> m_DownloadAgentHelperUpdateLengthEventHandler = null;
    private EventHandler<DownloadAgentHelperCompleteEventArgs> m_DownloadAgentHelperCompleteEventHandler = null;
    private EventHandler<DownloadAgentHelperErrorEventArgs> m_DownloadAgentHelperErrorEventHandler = null;

    private bool isDownloading = false;

    /// <summary>
    /// 下载代理辅助器更新数据事件
    /// </summary>
    public event EventHandler<DownloadAgentHelperUpdateLengthEventArgs> DownloadAgentHelperUpdateLength
    {
        add
        {
            m_DownloadAgentHelperUpdateLengthEventHandler += value;
        }
        remove
        {
            m_DownloadAgentHelperUpdateLengthEventHandler -= value;
        }
    }

    /// <summary>
    /// 下载代理辅助器完成事件
    /// </summary>
    public event EventHandler<DownloadAgentHelperCompleteEventArgs> DownloadAgentHelperComplete
    {
        add
        {
            m_DownloadAgentHelperCompleteEventHandler += value;
        }
        remove
        {
            m_DownloadAgentHelperCompleteEventHandler -= value;
        }
    }

    /// <summary>
    /// 下载代理辅助器错误事件
    /// </summary>
    public event EventHandler<DownloadAgentHelperErrorEventArgs> DownloadAgentHelperError
    {
        add
        {
            m_DownloadAgentHelperErrorEventHandler += value;
        }
        remove
        {
            m_DownloadAgentHelperErrorEventHandler -= value;
        }
    }

    public void Download(string downloadKey, object userData)
    {
        if (m_DownloadAgentHelperUpdateLengthEventHandler == null || m_DownloadAgentHelperCompleteEventHandler == null || m_DownloadAgentHelperErrorEventHandler == null)
        {
            Log.Fatal("Download agent helper handler is invalid.");
            return;
        }

        if (isDownloading)
        {
            Log.Fatal("Download agent is already downloading");
            return;
        }

        handle = Addressables.DownloadDependenciesAsync(downloadKey);
        handle.Completed += DownloadCompleteCallback;

        isDownloading = true;
    }

    public void OnReset()
    {
        if (handle.IsValid())
        {
            Addressables.Release(handle);
            handle = default;
        }

        isDownloading = false;
    }

    private void Update()
    {
        if (isDownloading && handle.IsValid()) 
        {
            DownloadStatus downloadStatus = handle.GetDownloadStatus();
            DownloadAgentHelperUpdateLengthEventArgs downloadAgentHelperUpdateLengthEventArgs = DownloadAgentHelperUpdateLengthEventArgs.Create(downloadStatus.DownloadedBytes, downloadStatus.Percent);
            m_DownloadAgentHelperUpdateLengthEventHandler(this, downloadAgentHelperUpdateLengthEventArgs);
        }
    }

    private void DownloadCompleteCallback(AsyncOperationHandle obj)
    {
        //如果下载器已被重置就没有回调
        if (!isDownloading || !obj.IsValid()) 
        {
            return;
        }
        isDownloading = false;

        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            long downloadedBytes = obj.GetDownloadStatus().DownloadedBytes;
            DownloadAgentHelperCompleteEventArgs downloadAgentHelperCompleteEventArgs = DownloadAgentHelperCompleteEventArgs.Create(downloadedBytes);
            m_DownloadAgentHelperCompleteEventHandler(this, downloadAgentHelperCompleteEventArgs);
            ReferencePool.Release(downloadAgentHelperCompleteEventArgs);
        }
        else
        {
            string errorMessage;
            if (obj.OperationException != null)
            {
                errorMessage = obj.OperationException.Message;
            }
            else
            {
                errorMessage = "Unknow Error";
            }

            DownloadAgentHelperErrorEventArgs downloadAgentHelperErrorEventArgs = DownloadAgentHelperErrorEventArgs.Create(errorMessage);
            m_DownloadAgentHelperErrorEventHandler(this, downloadAgentHelperErrorEventArgs);
            ReferencePool.Release(downloadAgentHelperErrorEventArgs);
        }

        if (obj.IsValid())
        {
            Addressables.Release(obj);
        }
    }
}
