using System;
using System.Collections;
using System.Collections.Generic;
using MySelf.Model;
using UnityEditor;
using UnityEngine;

public class DeleteAccountContinuePanel : PopupMenuForm
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
            //continue
            //delete service data
            //FirebaseServiceUtil.DeleteServiceData((b) =>
            FirebaseServiceUtil.DeleteServiceDataInOneDoc((b) =>
            {
                PlayerPrefs.DeleteAll();
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
        });
    }
}
