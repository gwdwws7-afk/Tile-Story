using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignInPanel : PopupMenuForm
{
    [SerializeField] private DelayButton Close_Btn,Google_Btn,FaceBook_Btn;
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
        Google_Btn.SetBtnEvent(() =>
        {
            //google sign in
            SignInBySdk(true);
        });
        FaceBook_Btn.SetBtnEvent(() =>
        {
            //facebook sign in
            SignInBySdk(false);
        });
    }

    private void SignInBySdk(bool isGoogle)
    {
        Action<bool> finishAction = (b) =>
        {
            if (!b)
            {
                GameManager.UI.ShowUIForm("FailSaveDataPanel",(u) =>
                {
                    GameManager.UI.HideUIForm("LoadingMenu");
                });
            }
            else
            {
                GameManager.UI.ShowUIForm("SuccessSaveDataPanel",(u) =>
                {
                    GameManager.UI.HideUIForm(this);
                    GameManager.UI.HideUIForm("LoadingMenu");
                });
            }
        };

        GameManager.UI.ShowUIForm("LoadingMenu",f =>
        {
            if (isGoogle)
            {
                GameManager.Firebase.SigninWithGoogle((b) =>
                {
                    finishAction?.Invoke(b);
                });
            }
            else
            {
                GameManager.Firebase.SigninWithFacebook((b) =>
                {
                    finishAction?.Invoke(b);
                });
            }
        }, userData: 10f);
    }
}
