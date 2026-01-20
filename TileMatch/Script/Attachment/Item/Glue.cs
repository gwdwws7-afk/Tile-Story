using DG.Tweening;
using Spine.Unity;
using System;
using UnityEngine;

public sealed class Glue : AttachItem
{
    //public SkeletonGraphic m_GlueBorder;
    public SkeletonGraphic m_GlueMain;

    [HideInInspector]
    public bool isLeftGlue;

    public override void Show()
    {
        //m_GlueBorder.Clear();
        m_GlueMain.Clear();
        //m_GlueBorder.Initialize(false);
        m_GlueMain.Initialize(false);

        if (isLeftGlue)
        {
            //m_GlueBorder.transform.localPosition = new Vector3(67, 11, 0);
            //m_GlueBorder.transform.localScale = new Vector3(-0.85f, 0.85f, 0.85f);
            m_GlueMain.transform.localPosition = new Vector3(68, 11, 0);
        }
        else
        {
            //m_GlueBorder.transform.localPosition = new Vector3(-67, 11, 0);
            //m_GlueBorder.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);
            m_GlueMain.transform.localPosition = new Vector3(-68, 11, 0);
        }
        m_GlueMain.transform.localRotation = Quaternion.Euler(0, 0, 0);

        if (!gameObject.activeSelf)
        {
            //m_GlueBorder.color = new Color(m_GlueBorder.color.r, m_GlueBorder.color.g, m_GlueBorder.color.b, 0);
            //m_GlueBorder.DOFade(1, 0.2f);
            m_GlueMain.color = new Color(m_GlueMain.color.r, m_GlueMain.color.g, m_GlueMain.color.b, 0);
            m_GlueMain.DOFade(1, 0.2f);
        }

        base.Show();
    }

    public override void SetColor(bool isBeCover)
    {
		if (!isBeCover)
		{
            m_GlueMain.DOColor(Color.white, 0.2f);
        }
		else
		{
            m_GlueMain.DOKill();
            m_GlueMain.color = isBeCover ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white;
        }
	}

    public void ShowSeparateAnim(Transform tileTrans, Action callback)
    {
        void animAction()
        {
            if (m_GlueMain.gameObject.activeSelf)
            {
                m_GlueMain.transform.SetParent(tileTrans.parent);
                m_GlueMain.AnimationState.SetAnimation(0, "active", false);
            }

            void AnimComplete()
            {
                m_GlueMain.gameObject.SetActive(false);
                m_GlueMain.transform.SetParent(transform);

                callback?.Invoke();
            }

            if (isLeftGlue)
            {
                tileTrans.DOBlendableLocalMoveBy(new Vector3(-60, -10, 0), 0.33f).SetEase(Ease.Linear);
                tileTrans.DOBlendableLocalRotateBy(new Vector3(0, 0, 19), 0.33f).SetEase(Ease.Linear);
            }
            else
            {
                tileTrans.DOBlendableLocalMoveBy(new Vector3(60, -10, 0), 0.33f).SetEase(Ease.Linear);
                tileTrans.DOBlendableLocalRotateBy(new Vector3(0, 0, -19), 0.33f).SetEase(Ease.Linear);
            }

            if (GameManager.Task != null)
                GameManager.Task.AddDelayTriggerTask(0.33f, AnimComplete);
            else
                tileTrans.parent.transform.DOScale(new Vector3(1f, 1f, 1), 0.33f).onComplete = AnimComplete;
        }

        if (isLeftGlue)
            tileTrans.DOBlendableLocalMoveBy(new Vector3(-5, 16, 0), 0.2f).SetEase(Ease.OutCubic);
        else
            tileTrans.DOBlendableLocalMoveBy(new Vector3(5, 16, 0), 0.2f).SetEase(Ease.OutCubic);

        if (GameManager.Task != null)
        {
            tileTrans.DOScale(1.1f, 0.2f).SetEase(Ease.OutCubic);
            GameManager.Task.AddDelayTriggerTask(0.16f, animAction);
        }
        else
        {
            tileTrans.DOScale(1.1f, 0.2f).SetEase(Ease.OutCubic).onComplete = animAction;
        }
    }
}
