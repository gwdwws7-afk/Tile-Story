using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HarvestKitchenExplainMenu : UIForm
{
    public DelayButton closeButon;
    public Action closeAction;

    public CanvasGroup[] guides;
    public GameObject[] tips;

    private bool m_IsAnimFinish;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        closeButon.SetBtnEvent(OnCloseBtnClick);

        for (int i = 0; i < guides.Length; i++)
        {
            guides[i].alpha = 0;
        }

        for (int i = 0; i < tips.Length; i++)
        {
            tips[i].transform.localScale = Vector3.zero;
        }

        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        for (int i = 0; i < guides.Length; i++)
        {
            guides[i].alpha = 0;
        }

        gameObject.SetActive(true);
        GameManager.Sound.PlayUIOpenSound();

        float delayTime = -0.3f;
        float showTime = 0.3f;
        for (int i = 0; i < guides.Length; i++)
        {
            delayTime += 0.3f;
            guides[i].DOFade(1f, showTime).SetDelay(delayTime);
        }

        delayTime += 0.3f;
        tips[0].transform.DOScale(1.1f, 0.25f).SetDelay(delayTime).onComplete = () =>
        {
            tips[0].transform.DOScale(1f, 0.25f);

            m_IsAnimFinish = true;
        };

        m_IsAvailable = true;
        showSuccessAction?.Invoke(this);
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        gameObject.SetActive(false);
        OnReset();

        base.OnHide();
    }

    public override void OnReset()
    {
        m_IsAnimFinish = false;
        base.OnReset();
    }

    public void SetCloseAction(Action action)
    {
        closeAction = action;
    }
    
    public void OnCloseBtnClick()
    {
        if (!m_IsAnimFinish)
        {
            return;
        }

        GameManager.UI.HideUIForm(this);
        closeAction?.Invoke();
    }
}
