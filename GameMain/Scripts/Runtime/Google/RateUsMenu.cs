using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RateUsMenu : PopupMenuForm
{
    [SerializeField]
    private DelayButton Close_Btn, Rate_Btn;

    public override void OnInit(UIGroup uiGroup, Action initCompleteAction = null, object userData = null)
    {
        base.OnInit(uiGroup, initCompleteAction, userData);

        SetBtnEvent();
    }

    private void SetBtnEvent()
    {
        Close_Btn.SetBtnEvent(() =>
        {
            GameManager.UI.HideUIForm(this);
        });

        Rate_Btn.SetBtnEvent(() =>
        {
            GameManager.UI.HideUIForm(this);
        
#if UNITY_IPHONE || UNITY_IOS
        if (!UnityEngine.iOS.Device.RequestStoreReview()) 
        {
            Application.OpenURL(string.Format(
            "itms-apps://itunes.apple.com/cn/app/id{0}?mt=8&action=write-review",
            Application.identifier));
        }
#else
            Application.OpenURL("market://details?id=" + Application.identifier);
#endif
        });
    }
}
