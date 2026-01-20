using GameFramework.Download;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// 下载辅助器
/// </summary>
public class DefaultDownloadHelper : IDownloadHelper
{
    public void GetDownloadSize(string downloadKey, Action<long> successCallback, Action<string> failCallback)
    {
        Addressables.GetDownloadSizeAsync(downloadKey).Completed += han =>
        {
            if (han.Status == AsyncOperationStatus.Succeeded)
            {
                successCallback?.Invoke(han.Result);
            }
            else
            {
                if (han.OperationException != null)
                {
                    failCallback?.Invoke(han.OperationException.Message);
                }
                else
                {
                    failCallback?.Invoke("Unknow Error");
                }
            }

            Addressables.Release(han);
        };
    }
}
