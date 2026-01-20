using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class WinningStreakTipMenu : PopupMenuForm
{
    public Button playButton;
    public Button closeButton;
    public TextMeshProUGUI describeText;
    public ClockBar clockBar;
    public GameObject[] ticks;

    public override void OnInit(UIGroup uiGroup, Action initCompleteAction = null, object userData = null)
    {
        playButton.onClick.AddListener(OnPlayButtonClick);
        closeButton.onClick.AddListener(OnClose);
        clockBar.gameObject.SetActive(false);
        base.OnInit(uiGroup, initCompleteAction, userData);
    }
    private void OnCountDownOver(object sender, CountdownOverEventArgs e)
    {
        clockBar.OnReset();
        clockBar.gameObject.SetActive(false);
    }

    public override void OnReset()
    {
        playButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();
        clockBar.OnReset();

        base.OnReset();
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        GameManager.Process.EndProcess("AutoShowWinningStreakStartMenu");

        base.OnHide(hideSuccessAction, userData);
    }

    public override void OnClose()
    {
        GameManager.UI.HideUIForm(this);
    }

    private void OnPlayButtonClick()
    {
        GameManager.UI.HideUIForm(this);
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        clockBar.OnUpdate(elapseSeconds, realElapseSeconds);

        base.OnUpdate(elapseSeconds, realElapseSeconds);
    }
}
