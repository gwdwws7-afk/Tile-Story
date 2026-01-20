using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 关卡横幅基类
/// </summary>
public abstract class LevelBannerBase : MonoBehaviour
{
    private bool m_IsInitialized;
    private bool m_IsShowing;

    /// <summary>
    /// 是否展示中
    /// </summary>
    public bool IsShowing => m_IsShowing;

    /// <summary>
    /// 初始化横幅
    /// </summary>
    public void Initialize()
    {
        if (m_IsInitialized)
            return;

        m_IsShowing = false;

        OnInitialize();

        m_IsInitialized = true;
    }

    /// <summary>
    /// 释放横幅
    /// </summary>
    public void Release()
    {
        if (!m_IsInitialized) 
            return;
        m_IsInitialized = false;

        Hide();
        OnRelease();
    }

    /// <summary>
    /// 展示横幅
    /// </summary>
    public void Show()
    {
        if (m_IsShowing)
            return;
        m_IsShowing = true;

        GameManager.Ads.HideBanner("LevelBanner");

        OnShow();
    }

    /// <summary>
    /// 隐藏横幅
    /// </summary>
    public void Hide()
    {
        if (!m_IsShowing)
            return;
        m_IsShowing = false;

        GameManager.Ads.ShowBanner("LevelBanner");

        OnHide();
    }

    protected abstract void OnInitialize();

    protected abstract void OnRelease();

    protected virtual void OnShow()
    {
        RectTransform rectTrans = transform.GetComponent<RectTransform>();
        rectTrans.DOKill();
        rectTrans.anchoredPosition = new Vector2(0, -600);
        gameObject.SetActive(true);
        rectTrans.DOAnchorPosY(20, 0.2f).onComplete = () =>
        {
            rectTrans.DOAnchorPosY(0, 0.2f);
        };
    }

    protected virtual void OnHide()
    {
        transform.DOKill();
        gameObject.SetActive(false);
    }
}
