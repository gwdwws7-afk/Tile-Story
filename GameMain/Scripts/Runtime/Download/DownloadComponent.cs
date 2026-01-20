using GameFramework.Download;
using MySelf.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityGameFramework.Runtime;

/// <summary>
/// 下载组件
/// </summary>
public sealed class DownloadComponent : GameFrameworkComponent
{
    private IDownloadManager m_DownloadManager = null;
    private IDownloadHelper m_DownloadHelper = null;
    private EventComponent m_EventComponent = null;

    [SerializeField]
    private Transform m_InstanceRoot = null;

    [SerializeField]
    private int m_DownloadAgentHelperCount = 3;

    [SerializeField]
    private float m_Timeout = 30f;

    protected override void Awake()
    {
        base.Awake();

        m_DownloadManager = GameFrameworkEntry.GetModule<DownloadManager>();
        if (m_DownloadManager == null)
        {
            Log.Fatal("Download manger is invalid");
            return;
        }

        m_DownloadHelper = new DefaultDownloadHelper();

        m_DownloadManager.DownloadStart += OnDownloadStart;
        m_DownloadManager.DownloadUpdate += OnDownloadUpdate;
        m_DownloadManager.DownloadSuccess += OnDownloadSuccess;
        m_DownloadManager.DownloadFailure += OnDownloadFailure;
        m_DownloadManager.DownloadFailure += RegisterRussiaFailureCallBack;
        m_DownloadManager.Timeout = m_Timeout;
    }

    private void Start()
    {
        m_EventComponent = GameManager.GetGameComponent<EventComponent>();
        if (m_EventComponent == null)
        {
            Log.Fatal("Event component is invalid.");
            return;
        }

        if (m_InstanceRoot == null)
        {
            m_InstanceRoot = new GameObject("Download Agent Instances").transform;
            m_InstanceRoot.SetParent(gameObject.transform);
            m_InstanceRoot.localScale = Vector3.one;
        }

        for (int i = 0; i < m_DownloadAgentHelperCount; i++)
        {
            AddDownloadAgentHelper(i);
        }
    }

    /// <summary>
    /// 增加下载代理辅助器
    /// </summary>
    /// <param name="index">下载代理辅助器索引</param>
    private void AddDownloadAgentHelper(int index)
    {
        DefaultDownloadAgentHelper downloadAgentHelper = (DefaultDownloadAgentHelper)new GameObject().AddComponent(typeof(DefaultDownloadAgentHelper));
        if (downloadAgentHelper == null)
        {
            Log.Error("Can not create download agent helper.");
            return;
        }

        downloadAgentHelper.name = string.Format("Download Agent Helper - {0}", index.ToString());
        Transform transform = downloadAgentHelper.transform;
        transform.SetParent(m_InstanceRoot);
        transform.localScale = Vector3.one;

        m_DownloadManager.AddDownloadAgentHelper(downloadAgentHelper);
    }

    /// <summary>
    /// 获取需要下载的大小
    /// </summary>
    /// <param name="downloadKey">下载键</param>
    /// <param name="successCallback">成功回调</param>
    /// <param name="failCallback">失败回调</param>
    public void GetDownloadSize(string downloadKey, Action<long> successCallback, Action<string> failCallback)
    {
        m_DownloadHelper.GetDownloadSize(downloadKey, successCallback, failCallback);
    }

    /// <summary>
    /// 根据下载任务的序列编号获取下载任务的信息
    /// </summary>
    /// <param name="serialId">要获取信息的下载任务的序列编号</param>
    /// <returns>下载任务的信息</returns>
    public TaskInfo GetDownloadInfo(int serialId)
    {
        return m_DownloadManager.GetDownloadInfo(serialId);
    }

    /// <summary>
    /// 根据下载任务的标签获取下载任务的信息
    /// </summary>
    /// <param name="tag">要获取信息的下载任务的标签</param>
    /// <returns>下载任务的信息</returns>
    public TaskInfo[] GetDownloadInfos(string tag)
    {
        return m_DownloadManager.GetDownloadInfos(tag);
    }

    /// <summary>
    /// 是否正在下载
    /// </summary>
    /// <param name="downloadKey">下载键</param>
    public bool IsDownloading(string downloadKey)
    {
        return m_DownloadManager.IsDownloading(downloadKey);
    }

    /// <summary>
    /// 增加下载任务
    /// </summary>
    /// <param name="downloadKey">下载键</param>
    /// <returns>下载任务序号</returns>
    public int AddDownload(string downloadKey)
    {
        return m_DownloadManager.AddDownload(downloadKey);
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
        return m_DownloadManager.AddDownload(downloadKey, tag, priority);
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
        return m_DownloadManager.AddDownload(downloadKey, tag, priority, userData);
    }

    /// <summary>
    /// 根据下载任务的序列编号移除下载任务
    /// </summary>
    /// <param name="serialId">要移除下载任务的序列编号</param>
    /// <returns>是否移除下载任务成功</returns>
    public bool RemoveDownload(int serialId)
    {
        return m_DownloadManager.RemoveDownload(serialId);
    }

    /// <summary>
    /// 根据下载任务的标签移除下载任务
    /// </summary>
    /// <param name="tag">要移除下载任务的标签</param>
    /// <returns>移除下载任务的数量</returns>
    public int RemoveDownloads(string tag)
    {
        return m_DownloadManager.RemoveDownloads(tag);
    }

    /// <summary>
    /// 移除所有下载任务
    /// </summary>
    /// <returns>移除下载任务的数量</returns>
    public int RemoveAllDownloads()
    {
        return m_DownloadManager.RemoveAllDownloads();
    }

    private void OnDownloadStart(object sender, GameFramework.Download.DownloadStartEventArgs e)
    {
        Log.Info("Download {0} Success", e.DownloadKey);

        m_EventComponent.Fire(this, DownloadStartEventArgs.Create(e));
    }

    private void OnDownloadUpdate(object sender, GameFramework.Download.DownloadUpdateEventArgs e)
    {
        Log.Info("Downloading...{0}...{1}", e.DownloadKey, e.Percent);

        m_EventComponent.Fire(this, DownloadUpdateEventArgs.Create(e));
    }

    private void OnDownloadSuccess(object sender, GameFramework.Download.DownloadSuccessEventArgs e)
    {
        Log.Info("Download {0} Success", e.DownloadKey);

        m_EventComponent.Fire(this, DownloadSuccessEventArgs.Create(e));
    }

    private void OnDownloadFailure(object sender, GameFramework.Download.DownloadFailureEventArgs e)
    {
        Log.Warning("Download failure, download serial id '{0}', download key '{1}', error message '{2}'.", e.SerialId.ToString(), e.DownloadKey, e.ErrorMessage);

        m_EventComponent.Fire(this, DownloadFailureEventArgs.Create(e));
    }
    
    /// <summary>
    /// 添加下载失败回调
    /// </summary>
    /// <param name="failCallBack"></param>
    public void RegisterRussiaFailureCallBack(object sender, GameFramework.Download.DownloadFailureEventArgs e)
    {
        MyDownloadUrl.DownloadUrlManager.SetRussiaFailCallBack();
    }

    #region Download

    public bool IsCurAreaHaveAsset()
    {
        int areaId = DecorationModel.Instance.GetDecorationOperatingAreaID();
        if (areaId < Constant.GameConfig.StartNeedDownloadArea)
            return true;
        else
            return AddressableUtils.IsHaveAsset(DecorationModel.Instance.GetNowAreaResourceName());
    }

    public bool IsHaveAssetByAreaId(int areaId)
    {
        //做一次转换【需要保证上次未转换】
        areaId = DecorationModel.Instance.GetAlteredDecorationAreaID(areaId);
        string name = DecorationModel.GetAreaNameById(areaId);
        return AddressableUtils.IsHaveAsset(name);
    }

    public int GetCurNeedDownloadAreaId()
    {
        //当前的id是
        var curDecorationAreaID = DecorationModel.Instance.Data.DecorationAreaID;
        //如果当前章节ab有了，那么判断当前章节是否完成，如果完成这时候就需要下载下一个章节
        if (IsHaveAssetByAreaId(curDecorationAreaID))
        {
            bool lastComplete = DecorationModel.Instance.CheckTargetAreaIsComplete(curDecorationAreaID);
            //如果是当前章节的话，那么给下一个id
            if (lastComplete)
            {
                return curDecorationAreaID + 1;
            }
        }
        return curDecorationAreaID;
    }

    public void DownloadAreaByBackGround()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
            return;

        if (SystemInfoManager.DeviceType <= DeviceType.Normal)
            return;

        var list = GameManager.PlayerData.GetNeedDownNameList(true, 3);
        var needDownDict = AddressableUtils.GetNeedDownLoadAssetSize(list);
        foreach (var item in needDownDict)
        {
            if (!GameManager.Download.IsDownloading(item.Key))
            {
                Log.Info($"DownloadAreaByBackGround:{item.Key}");
                AddDownload(item.Key);
            }
        }
    }

    public void DownloadDecorationAreaThumbnailByBackGround()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
            return;

        List<string> needDownloadNameList = new List<string>();

        int areaId = Constant.GameConfig.MaxDecorationArea;
        if (SystemInfoManager.DeviceType < DeviceType.Normal)
        {
            areaId = DecorationModel.Instance.GetDecorationOperatingAreaID();
            areaId = areaId + 1 > Constant.GameConfig.MaxDecorationArea ? Constant.GameConfig.MaxDecorationArea : areaId + 1;
        }

        for (int i = 11; i <= areaId; i += 5)
        {
            string name = $"Area{i}_After";
            needDownloadNameList.Add(name);
        }

        var needDownDict = AddressableUtils.GetNeedDownLoadAssetSize(needDownloadNameList);
        foreach (var item in needDownDict)
        {
            if (!GameManager.Download.IsDownloading(item.Key))
            {
                Log.Info($"DownloadAreaThumbnailByBackGround:{item.Key}");
                AddDownload(item.Key);
            }
        }
    }

    #endregion
}
