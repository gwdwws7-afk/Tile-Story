using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class RemovePopupAdsBuySuccessMenu : PopupMenuForm
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI describeText;
    public TextMeshProUGUI okButtonText;

    public Button okButton;
    public Button closeButton;

    public override void OnInit(UIGroup uiGroup, Action initCompleteAction = null, object userData = null)
    {
        okButton.onClick.AddListener(OnClose);
        closeButton.onClick.AddListener(OnClose);

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
