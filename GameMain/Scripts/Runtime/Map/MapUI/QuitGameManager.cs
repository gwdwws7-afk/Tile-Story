using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class QuitGameManager : PopupMenuForm
{
    public Button quitButton;
    public Button closeButton;

    public override void OnInit(UIGroup uiGroup, Action initCompleteAction, object userData)
    {
        quitButton.SetBtnEvent(OnQuitButtonClick);
        closeButton.SetBtnEvent(OnClose);

        base.OnInit(uiGroup, initCompleteAction, userData);
    }

    public override void OnClose()
    {
        GameManager.UI.HideUIForm(this);
    }

    public override bool CheckInitComplete()
    {
        return true;
    }

    private void OnQuitButtonClick()
    {
        Application.Quit();
    }
}
