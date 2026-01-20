using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionLostPanel : PopupMenuForm
{
    [SerializeField] private DelayButton Close_Btn, Play_Btn;

    private Action action;
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        BtnEvent();
        if (userData != null)
            action = userData as Action;
        base.OnInit(uiGroup, completeAction, userData);
    }

    private void BtnEvent()
    {
        Close_Btn.SetBtnEvent(() =>
        {
            GameManager.UI.HideUIForm(this);
        });
        Play_Btn.SetBtnEvent(() =>
        {
            action?.InvokeSafely();
            GameManager.UI.HideUIForm(this);
        });
    }
}
