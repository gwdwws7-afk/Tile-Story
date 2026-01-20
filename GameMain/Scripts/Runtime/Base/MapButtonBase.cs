using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapButtonBase : UIForm
{
    public DelayButton button;
    public CountdownTimer timer;
    public SimpleSlider slider;
    public ObjectShaker warningSign;
    public TextMeshProUGUILocalize lockText;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        button.onClick.AddListener(OnButtonClick);
    }

    public override void OnReset()
    {
        button.onClick.RemoveAllListeners();

        timer.OnReset();

        base.OnReset();
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);

        timer.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        gameObject.SetActive(true);

        base.OnShow(showSuccessAction, userData);
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        gameObject.SetActive(false);

        base.OnHide(hideSuccessAction, userData);
    }

    public override void OnCover()
    {
        base.OnCover();

        button.interactable = false;
    }

    public override void OnReveal()
    {
        base.OnReveal();

        button.interactable = true;
    }

    public virtual void OnButtonClick()
    {
    }

    public virtual void SetWarningSign(bool isShow)
    {
        if (isShow && !warningSign.gameObject.activeSelf)
        {
            warningSign.gameObject.SetActive(true);
            warningSign.OnInit();
        }

        if (!isShow && warningSign.gameObject.activeSelf)
        {
            warningSign.OnReset();
            warningSign.gameObject.SetActive(false);
        }
    }
}
