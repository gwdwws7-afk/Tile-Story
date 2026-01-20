using System;
using System.Collections;
using System.Collections.Generic;
using MySelf.Model;
using UnityEngine;

public class SuccessSaveDataPanel : PopupMenuForm
{
    [SerializeField] private DelayButton Close_btn,Continue_Btn;
    [SerializeField] private TextMeshProUGUILocalize TextMeshProUGUILocalize; 
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
        TextMeshProUGUILocalize.SetTerm(content);
    }

    private void BtnEvent()
    {
        Action action1 = () =>
        {
            Action action = () =>
            {
                GameManager.UI.HideUIForm(this);
                GameManager.UI.HideUIForm("LoadingMenu");
            };
            //拉去firebase数据
            GameManager.UI.ShowUIForm("LoadingMenu",(u) =>
            {
                //FirebaseServiceUtil.GetDataFromService((service,local,syncDataAction,uploadDataAction) =>
                FirebaseServiceUtil.GetDataFromServiceInOneDoc((service, local, syncDataAction, uploadDataAction) =>                
                {
                    action?.Invoke();
                    GameManager.UI.ShowUIForm("SyncAccountDataPanel",(u) =>
                    {
                        Action useServiceAction = () =>
                        {
                            syncDataAction?.Invoke();
                            GameManager.UI.HideUIForm("MapSettingMenuPanel");
                            GameManager.DataNode.SetData<int>("NowLevel",GameManager.PlayerData.NowLevel);
                            GameManager.Event.FireNow(CommonEventArgs.EventId,CommonEventArgs.Create(CommonEventType.RefreshUIBySyncData));
                            GameManager.UI.RefreshUIByFirebaseLogin();
                        };
                        (u as SyncAccountDataPanel).SetControlls(service,local,useServiceAction,uploadDataAction);
                    });
                }, () =>
                {
                    action?.Invoke();
                });
            });
        };
        Close_btn.SetBtnEvent(() =>
        {
            action1?.Invoke();
        });
        Continue_Btn.SetBtnEvent(() =>
        {
            action1?.Invoke();
        });
    }
}
