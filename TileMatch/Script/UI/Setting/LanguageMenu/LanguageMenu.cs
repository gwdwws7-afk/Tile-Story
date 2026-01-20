using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageMenu : PopupMenuForm
{
    [SerializeField]
    private LanguageItem[] LanguageItems;
    [SerializeField]
    private DelayButton CloseButton;
    [SerializeField]
    private ScrollRect ScrollRect;


    public override void OnInit(UIGroup uiGroup, Action initCompleteAction = null, object userData = null)
    {
        CloseButton.SetBtnEvent(()=>GameManager.UI.HideUIForm(this));

        for (int i = 0; i < LanguageItems.Length; i++)
        {
            LanguageItems[i].OnInit();
        }

        Refresh(true);

        base.OnInit(uiGroup, initCompleteAction, userData);
    }


    public void Refresh(bool correctScroll)
    {
        for (int i = 0; i < LanguageItems.Length; i++)
        {
            if (GameManager.Localization.Language == LanguageItems[i].language)
            {
                if (correctScroll)
                {
                    ScrollRect.verticalNormalizedPosition = (LanguageItems.Length - i) / (float)LanguageItems.Length;
                }
            }
        }
    }
}
