using System;
using System.Collections;
using System.Collections.Generic;
using MySelf.Model;
using UnityEngine;

public class PkGameFailPanel : PopupMenuForm
{
    [SerializeField] private PkSorce PkSorce;
    [SerializeField] private DelayButton HelpBtn,CloseBtn,ContinueBtn;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        PkSorce.Init();
        SetBtnEvetn();
        PkGameModel.Instance.RemoveListen();
        GameManager.Sound.PlayAudio("SFX_PK_Lose");
        base.OnInit(uiGroup, completeAction, userData);
    }

    private void SetBtnEvetn()
    {
        Action finishAction = () =>
        {
            GameManager.Firebase.RecordMessageByEvent("PKMatch_Win");
            //结算活动
            PkGameModel.Instance.BalanceActivity();
                     
            GameManager.Event.FireNow(CommonEventArgs.EventId,CommonEventArgs.Create(CommonEventType.PkGameOver));
            
            if (!PkGameModel.Instance.IsActivityOpen&&PkGameModel.Instance.IsCanOpenByTime)
            {
                Action finishAction= this.m_ProcessFinishAction;
                this.m_ProcessFinishAction = null;
                GameManager.UI.ShowUIForm("PkGameStartPanel",u =>
                {
                    u.SetHideAction(finishAction);
                });
            }
            //关闭界面
            GameManager.UI.HideUIForm(this);
        };
        HelpBtn.SetBtnEvent(() =>
        {
            //展示玩法说明界面
            GameManager.UI.ShowUIForm("PkRulesPanel");
        });
        CloseBtn.SetBtnEvent(() =>
        {
            finishAction();
        });
        ContinueBtn.SetBtnEvent(() =>
        {
            finishAction();
        });
    }
}
