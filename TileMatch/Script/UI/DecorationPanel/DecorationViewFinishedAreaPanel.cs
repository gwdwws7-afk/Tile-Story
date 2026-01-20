using GameFramework.Event;
using MySelf.Model;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Spine.Unity;

public class DecorationViewFinishedAreaPanel : CenterForm, ICustomOnEscapeBtnClicked
{
    [SerializeField]
    private DelayButton closeBtn;


    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        closeBtn.SetBtnEvent(() =>
        {
            DecorationModel.Instance.ClearTempDecoratingOperatingAreaID();
            GameManager.UI.HideUIForm(this);
        });
        base.OnInit(uiGroup, completeAction, userData);
    }


    public override void OnRelease()
    {
        closeBtn.OnReset();
        base.OnRelease();
    }

    public void OnEscapeBtnClicked()
    {
        if (!closeBtn.isActiveAndEnabled)
            return;

        closeBtn?.onClick?.Invoke();
    }
}
