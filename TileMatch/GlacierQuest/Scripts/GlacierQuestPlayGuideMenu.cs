using DG.Tweening;
using System;
using UnityEngine;

public class GlacierQuestPlayGuideMenu : UIForm
{
    public GameObject[] guides;
    public DelayButton PlayBtn, closeBtn;
    private bool m_IsAnimFinish;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        PlayBtn.SetBtnEvent(() => GameManager.UI.HideUIForm(this));
        closeBtn.SetBtnEvent(() => GameManager.UI.HideUIForm(this));
        for (int i = 0; i < guides.Length; i++)
        {
            guides[i].transform.localScale = Vector3.zero;
            guides[i].SetActive(true);
        }
        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        GameManager.Sound.PlayUIOpenSound();
        float showTime = 0;
        float fadeTime = 0;
        float delayTime = -0.2f;
        for (int i = 0; i < guides.Length; i++)
        {
            int index = i;
            fadeTime = 0.25f;
            showTime = 0.25f;
            delayTime += 0.2f;
            guides[i].transform.DOScale(1.1f, showTime).SetDelay(delayTime).onComplete = () =>
            {
                if (index == guides.Length - 1)
                {
                    guides[index].transform.DOScale(1f, fadeTime).onComplete = () =>
                    {
                        m_IsAnimFinish = true;
                    };
                }
                else
                {
                    guides[index].transform.DOScale(1f, fadeTime);
                }
            };
        }
        base.OnShow(showSuccessAction, userData);
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        gameObject.SetActive(false);
        OnReset();
        GameManager.Sound.PlayUICloseSound();
        base.OnHide(hideSuccessAction, userData);
    }
}
