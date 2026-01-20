using System;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// 关卡横幅逻辑基类
/// </summary>
public abstract class LevelBannerLogicBase
{
    private LevelBannerBase m_LevelBanner;
    private AsyncOperationHandle<GameObject> m_AssetHandle;
    private Action<LevelBannerBase> m_OnCreateBannerComplete;
    private bool m_DeferTriggerCreateBannerCompleteEvent;
    private bool m_IsReleased;

    /// <summary>
    /// 横幅类型
    /// </summary>
    public abstract LevelBannerType BannerType { get; }

    /// <summary>
    /// 生成横幅完毕时
    /// </summary>
    public event Action<LevelBannerBase> OnCreateBannerComplete
    {
        add
        {
            m_OnCreateBannerComplete += value;

            if (m_AssetHandle.IsValid() && m_AssetHandle.IsDone) 
            {
                m_DeferTriggerCreateBannerCompleteEvent = true;
            }
        }
        remove
        {
            m_OnCreateBannerComplete -= value;
        }
    }

    /// <summary>
    /// 轮询
    /// </summary>
    public void OnUpdate()
    {
        if (m_IsReleased)
            return;

        if (m_DeferTriggerCreateBannerCompleteEvent)
        {
            m_DeferTriggerCreateBannerCompleteEvent = false;

            m_OnCreateBannerComplete?.Invoke(m_LevelBanner);
            m_OnCreateBannerComplete = null;
        }
    }

    /// <summary>
    /// 检测横幅是否可以展示
    /// </summary>
    public abstract bool CheckBannerCanShow();

    /// <summary>
    /// 生成横幅
    /// </summary>
    /// <param name="parent">父物体</param>
    public void CreateBanner(Transform parent)
    {
        if (!m_AssetHandle.IsValid())
        {
            m_IsReleased = false;

            string assetName = BannerType.ToString();
            m_AssetHandle = UnityUtility.InstantiateAsync(assetName, parent, obj =>
            {
                if (m_IsReleased)
                    return;

                m_LevelBanner = obj.GetComponent<LevelBannerBase>();
                m_LevelBanner.Initialize();

                m_DeferTriggerCreateBannerCompleteEvent = true;
            });
        }
        else
        {
            Log.Warning("Create banner {0} fail - banner asset handle is exist", BannerType.ToString());
        }
    }

    /// <summary>
    /// 释放横幅
    /// </summary>
    public void ReleaseBanner()
    {
        m_IsReleased = true;

        if (m_LevelBanner != null) 
        {
            m_LevelBanner.Release();
            m_LevelBanner = null;
        }

        m_OnCreateBannerComplete = null;
        m_DeferTriggerCreateBannerCompleteEvent = false;
        UnityUtility.UnloadInstance(m_AssetHandle);
        m_AssetHandle = default;
    }

    /// <summary>
    /// 展示横幅
    /// </summary>
    public void ShowBanner()
    {
        if (m_AssetHandle.IsValid() && !m_AssetHandle.IsDone)
        {
            m_OnCreateBannerComplete += banner =>
            {
                ShowBanner();
            };

            return;
        }

        if (m_LevelBanner != null)
        {
            m_LevelBanner.Show();
        }
    }

    /// <summary>
    /// 隐藏横幅
    /// </summary>
    public void HideBanner()
    {
        if (m_AssetHandle.IsValid() && !m_AssetHandle.IsDone)
        {
            m_OnCreateBannerComplete += banner =>
            {
                HideBanner();
            };

            return;
        }

        if (m_LevelBanner != null)
        {
            m_LevelBanner.Hide();
        }
    }
}
