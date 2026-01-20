using System;
using System.Collections;
using System.Collections.Generic;
using MySelf.Model;
using UnityEngine;

public class SaveYourDataPanel : PopupMenuForm
{
    [SerializeField] private DelayButton Close_Btn,Yes_Btn,No_Btn;
    [SerializeField] private TextMeshProUGUILocalize LogOutType_Text;
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        SetText();
        BtnEvent();
        base.OnInit(uiGroup, completeAction, userData);
    }

    private void SetText()
    {
        string content=$"Settings.Log out from {GameManager.PlayerData.LoginSdkName.ToString()}?";
        LogOutType_Text.SetTerm(content);
    }

    private void BtnEvent()
    {
        Close_Btn.SetBtnEvent(() =>
        {
            GameManager.UI.HideUIForm(this);
        });
        Yes_Btn.SetBtnEvent(() =>
        {
            //退出
            GameManager.Firebase.SignOut();
            GameManager.UI.HideUIForm(this);
            GameManager.UI.HideUIForm("ConnectAccountPanel");
        });
        No_Btn.SetBtnEvent(() =>
        {
            GameManager.UI.HideUIForm(this);
        });
    }
}
