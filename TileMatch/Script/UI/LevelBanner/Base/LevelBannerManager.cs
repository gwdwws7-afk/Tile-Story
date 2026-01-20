using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �ؿ����������
/// </summary>
public sealed class LevelBannerManager : MonoBehaviour
{
    private List<LevelBannerLogicBase> m_BannerList = new List<LevelBannerLogicBase>();
    private LevelBannerLogicBase m_CurBannerLogic = null;

    public void Initialize(LevelPlayType type)
    {
        m_BannerList.Clear();
        m_BannerList.Add(new LevelBannerLogic_BalloonRise());
        m_BannerList.Add(new LevelBannerLogic_Objective());
    }

    public void Release()
    {
        m_CurBannerLogic = null;

        foreach (LevelBannerLogicBase banner in m_BannerList)
        {
            banner.ReleaseBanner();
        }
        m_BannerList.Clear();
    }

    private void Update()
    {
        if (m_BannerList.Count > 0)
        {
            for (int i = 0; i < m_BannerList.Count; i++)
            {
                m_BannerList[i].OnUpdate();
            }
        }
    }

    /// <summary>
    /// ��ȡ��չʾ�ĺ��
    /// </summary>
    /// <returns>����߼���</returns>
    public LevelBannerLogicBase GetCanShowBanner()
    {
        if (m_CurBannerLogic != null && m_CurBannerLogic.CheckBannerCanShow())
            return m_CurBannerLogic;

        foreach (LevelBannerLogicBase banner in m_BannerList)
        {
            if (banner.CheckBannerCanShow())
                return banner;
        }

        return null;
    }

    /// <summary>
    /// ���ɿ�չʾ�ĺ��
    /// </summary>
    /// <param name="parent">���ɺ���ĸ�����</param>
    public void CreateCanShowBanner(Transform parent)
    {
        if (m_CurBannerLogic == null || !m_CurBannerLogic.CheckBannerCanShow()) 
        {
            m_CurBannerLogic = GetCanShowBanner();
        }

        if (m_CurBannerLogic != null)
        {
            m_CurBannerLogic.CreateBanner(parent);
        }
    }

    /// <summary>
    /// չʾ��չʾ�ĺ��
    /// </summary>
    public void ShowCanShowBanner()
    {
        if (m_CurBannerLogic == null || !m_CurBannerLogic.CheckBannerCanShow())
        {
            m_CurBannerLogic = GetCanShowBanner();
        }

        if (m_CurBannerLogic != null)
        {
            m_CurBannerLogic.ShowBanner();
        }
    }
}
