using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseCancelMenu : PopupMenuForm
{
    public Button okButton;
    public Button closeButton;
    public GameObject purchaseCancelArea, offlineArea;

    public override void OnInit(UIGroup uiGroup, Action initCompleteAction = null, object userData = null)
    {
        okButton.onClick.AddListener(OnClose);
        closeButton.onClick.AddListener(OnClose);

        purchaseCancelArea.SetActive(Application.internetReachability != NetworkReachability.NotReachable);
        offlineArea.SetActive(Application.internetReachability == NetworkReachability.NotReachable);

        base.OnInit(uiGroup, initCompleteAction, userData);
    }

    public override void OnReset()
    {
        okButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();

        base.OnReset();
    }

    public override void OnClose()
    {
        GameManager.UI.HideUIForm(this);

        base.OnClose();
    }
}
