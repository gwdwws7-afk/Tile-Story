using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalPackEntrance : UIForm
{
    [SerializeField]
    private DelayButton button;
    [SerializeField]
    private CountdownTimer countdownTimer;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        UpdateBtnActiveAndTimer();

        button.SetBtnEvent(OnButtonClick);

        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnRelease()
    {
        button.onClick.RemoveAllListeners();

        base.OnRelease();
    }

    private void Update()
    {
        countdownTimer.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);
    }


    private void OnButtonClick()
    {
        if (GameManager.Process.Count > 0)
            return;

        GameManager.UI.ShowUIForm("AnimalPackMenu", UIFormType.PopupUI);
    }


    private void UpdateBtnActiveAndTimer()
    {
        if (DateTime.Now > AnimalPackMenu.GiftPackStartTime && DateTime.Now < AnimalPackMenu.GiftPackEndTime &&
            GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockPackLevel)
        {
            gameObject.SetActive(true);

            countdownTimer.timeText.gameObject.SetActive(true);
            countdownTimer.StartCountdown(AnimalPackMenu.GiftPackEndTime);
            countdownTimer.CountdownOver += OnCountdownOver;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnCountdownOver(object sender, CountdownOverEventArgs e)
    {
        gameObject.SetActive(false);
    }

    public override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
    {
    }

    public override void OnPause()
    {
        button.interactable = false;
        base.OnPause();
    }

    public override void OnResume()
    {
        button.interactable = true;
        base.OnResume();
    }
}
