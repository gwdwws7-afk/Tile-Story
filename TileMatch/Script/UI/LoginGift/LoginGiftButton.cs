using GameFramework.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginGiftButton : EntranceUIForm
{
    public CountdownTimer timer;
    public GameObject warningSign;

    private bool needShowLoginGiftByToday;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        GameManager.Event.Subscribe(CommonEventArgs.EventId, CommonEventCallBack);

        Refresh();
    }

    public override void OnRelease()
    {
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId, CommonEventCallBack);

        timer.OnReset();
        _isCountDownOver = false;

        base.OnRelease();
    }

    private void Update()
    {
        timer.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);
    }

    public void Refresh()
    {
        bool isShowDaliyGiftBtn = GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockLoginGiftLevel;
        gameObject.SetActive(isShowDaliyGiftBtn);

        needShowLoginGiftByToday = GameManager.PlayerData.NeedShowLoginGiftByToday();
        warningSign.SetActive(needShowLoginGiftByToday);
        timer.gameObject.SetActive(!needShowLoginGiftByToday);
        SetTimer();

        if (!needShowLoginGiftByToday)
            gameObject.SetActive(false);
    }

    private void SetTimer()
    {
        timer.OnReset();
        _isCountDownOver = false;
        if (!needShowLoginGiftByToday)
        {
            timer.CountdownOver += OnCountdownOver;
            timer.StartCountdown(DateTime.Now.AddDays(1) - DateTime.Now.TimeOfDay);
        }
    }

    private bool _isCountDownOver;
    private void OnCountdownOver(object sender, CountdownOverEventArgs e)
    {
        if (_isCountDownOver) return;
        _isCountDownOver = true;
        timer.gameObject.SetActive(false);
        SetWarningSign(true);
    }

    private void SetWarningSign(bool isShow)
    {
        warningSign.SetActive(isShow);
    }

    private void CommonEventCallBack(object sender, GameEventArgs e)
    {
        CommonEventArgs ne = (CommonEventArgs)e;
        switch (ne.Type)
        {
            case CommonEventType.GetLoginGift:
                Refresh();
                break;
        }
    }

    public override void OnButtonClick()
    {
        GameManager.UI.ShowUIForm("LoginGiftPanel");
    }
}
