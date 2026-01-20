using DG.Tweening;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Curtain : AttachItem
{
    public SkeletonGraphic m_Spine;

    private CurtainState m_State;

    public CurtainState State
    {
        get
        {
            return m_State;
        }
        set
        {
            if (m_State != value)
            {
                m_State = value;
                Refresh(true);
            }
        }
    }

    public override void Init(AttachLogic logic)
    {
        base.Init(logic);

        AttachLogic_5_6 curtainLogic = logic as AttachLogic_5_6;
        m_State = curtainLogic.State;
    }

    public override void Show()
    {
        m_Spine.Clear();
        m_Spine.Initialize(false);

        Refresh(false);

        if (!gameObject.activeSelf)
        {
            m_Spine.color = new Color(m_Spine.color.r, m_Spine.color.g, m_Spine.color.b, 0);
            m_Spine.DOFade(1, 0.2f);
        }

        base.Show();
    }

    public override void SetColor(bool isBeCover)
    {
        if (!isBeCover)
        {
            m_Spine.DOColor(Color.white, 0.2f);
        }
        else
        {
            m_Spine.DOKill();
            m_Spine.color = isBeCover ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white;
        }
    }

    public override void OnClick()
    {
        base.OnClick();

        if (m_State == CurtainState.Open)
        {
            m_Spine.DOKill();
            m_Spine.DOColor(new Color(1, 1, 1, 0), 0.2f);
        }
        else if (m_State == CurtainState.Close)
        {
            m_Spine.AnimationState.SetAnimation(0, "close_click", false);
        }
    }

    private void Refresh(bool isShowAnim)
    {
        if (isShowAnim)
        {
            if (m_State == CurtainState.Open)
                m_Spine.AnimationState.SetAnimation(0, "open", false);
            else if (m_State == CurtainState.Close)
                m_Spine.AnimationState.SetAnimation(0, "close", false);
        }
        else
        {
            if (m_State == CurtainState.Open)
                m_Spine.AnimationState.SetAnimation(0, "open_idle", false);
            else if (m_State == CurtainState.Close)
                m_Spine.AnimationState.SetAnimation(0, "close_idle", false);
        }
    }
}
