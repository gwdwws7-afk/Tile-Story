using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using MySelf.Model;
using UnityEngine;

public class ConnectAccountPanel : PopupMenuForm
{
    [SerializeField] private TextMeshProUGUILocalize Tittle_Text;
    [SerializeField] private DelayButton GameSync_Btn,Quit_Btn,DeleteAccount_Btn,Close_Btn;
    [SerializeField] private TextMeshProUGUILocalize Content_Text;
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        SetTittleText();
        BtnEvent();
        base.OnInit(uiGroup, completeAction, userData);
    }

    private void SetTittleText()
    {
        //google or facebook
        string content = $"Settings.{GameManager.PlayerData.LoginSdkName.ToString()}";
        Tittle_Text.SetTerm(content);
        
        Content_Text.SetParameterValue("0",GameManager.PlayerData.LoginSdkName.ToString());
    }

    private void BtnEvent()
    {
        GameSync_Btn.SetBtnEvent(() =>
        {
            //get service data  [level,item]
            Action<bool> saveSuccessAction = (b) =>
            {
                GameManager.UI.HideUIForm("LoadingMenu");
                if (b)
                {
                    GameManager.UI.ShowWeakHint("Settings.Data sync successful!",Vector3.zero);
                }
                else
                {
                    //GameManager.UI.ShowWeakHint("Settings.Save Failed",Vector3.zero);
                    GameManager.UI.ShowUIForm("ConnectionLostPanel");
                }
            };
            //上传数据
            GameManager.UI.ShowUIForm("LoadingMenu",(u) =>
            {
                //FirebaseServiceUtil.SaveToService(saveSuccessAction);
                (u as LoadingMenu).SetTimeOutAction(() =>
                {
                    saveSuccessAction?.InvokeSafely(false);
                });
                FirebaseServiceUtil.SaveToServiceInOneDoc(saveSuccessAction);
            }, userData: 15f);
        });
        Quit_Btn.SetBtnEvent(() =>
        {
            //二次确认退出弹框
            GameManager.UI.ShowUIForm("SaveYourDataPanel");
        });
        DeleteAccount_Btn.SetBtnEvent(() =>
        {
            //delete account
            GameManager.UI.ShowUIForm("DeleteAccountPanel");
        });
        Close_Btn.SetBtnEvent(() =>
        {
            GameManager.UI.HideUIForm(this);
        });
    }
}
