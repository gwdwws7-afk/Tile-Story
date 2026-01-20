using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GameFramework.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

public class TimeLimitGuidePrefab : UIForm
{
    public Image bg;
    public Transform clock;
    public TextMeshProUGUI tipText1, tipText2, timeText;
    public DelayButton bgBtn;
    public Action callback;
    private Vector3 target;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        bgBtn.enabled = false;
        bg.DOFade(0, 0);
        clock.localScale = Vector3.zero;
        tipText1.transform.localScale = Vector3.zero;
        tipText2.color = new Color(tipText2.color.r, tipText2.color.g, tipText2.color.b, 0f);
        bgBtn.SetBtnEvent(OnClickBtn);
        
        var form = GameManager.UI.GetUIForm("GameSettingPanel");
        if (form && form.gameObject.activeSelf)
        {
            bgBtn.GetComponent<Image>().raycastTarget = false;
        }
        
        GameManager.Event.Subscribe(CommonEventArgs.EventId, OpenClickEvent);
        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        bg.DOFade(1, 0.5f);
        clock.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
        tipText1.transform.DOScale(1, 0.5f).SetEase(Ease.OutBack).onComplete += () =>
        {
            tipText2.DOFade(1, 0.5f);
            bgBtn.enabled = true;
        };
        base.OnShow(showSuccessAction, userData);
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        callback?.Invoke();
        base.OnHide(hideSuccessAction, userData);
    }

    public override void OnRelease()
    {
        // 重置组件状态
        clock.localPosition = Vector3.up * 190;
        tipText1.color = new Color(tipText1.color.r, tipText1.color.g, tipText1.color.b, 1f);
        timeText.color = new Color(timeText.color.r, timeText.color.g, timeText.color.b, 1f);
        base.OnRelease();
    }

    public void OnClickBtn()
    {
        var form = GameManager.UI.GetUIForm("GameSettingPanel");
        if (form && form.gameObject.activeSelf) return;
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId, OpenClickEvent);
        tipText1.DOFade(0, 0.4f);
        timeText.DOFade(0, 0.4f);
        tipText2.DOFade(0, 0.4f);
        bg.DOFade(0, 0.4f).onComplete += () =>
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(clock.DOJump(target, 0.1f, 1, 0.8f));
            seq.Join(clock.DOScale(0.4f, 0.8f));
            seq.onComplete += () =>
            {
                GameManager.UI.HideUIForm(this);
            }; 
        };
    }

    public void SetInfo(float time, Vector3 pos)
    {
        timeText.text = string.Format("{0:D2}:{1:D2}", (int)time / 60, (int)time % 60);
        target = pos;
    }

    public void SetCallBack(Action action)
    {
        callback = action;
    }
    
    public void OpenClickEvent(Object obj, GameEventArgs args)
    {
        CommonEventArgs ne = (CommonEventArgs)args;

        if(ne.Type == CommonEventType.ContinueLevelTime)
        {
            bgBtn.GetComponent<Image>().raycastTarget = true;
        }
    }
}
