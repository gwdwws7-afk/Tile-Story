using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteAccountPanel : PopupMenuForm
{
    [SerializeField] private DelayButton Close_Btn,Continue_Btn;
        
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
        Continue_Btn.SetBtnEvent(() =>
        {
            //
            GameManager.UI.ShowUIForm("SureDeleteAccountPanel",(u) =>
            {
                GameManager.UI.HideUIForm(this);
            });
        });
    }
}
