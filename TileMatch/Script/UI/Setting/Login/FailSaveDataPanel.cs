using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FailSaveDataPanel : PopupMenuForm
{
    [SerializeField] private DelayButton Close_Btn,Play_Btn;
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        BtnEvent();
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
            GameManager.UI.HideUIForm(this);
        });
    }
}
