using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeekendPicnicPackEntrance : UIForm
{
    [SerializeField]
    private DelayButton button;
    [SerializeField]
    private CountdownTimer countdownTimer;

    private DateTime signTime = new DateTime(2024, 5, 31);
    private DateTime endTime;

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

        GameManager.UI.ShowUIForm("WeekendPicnicPackMenu",userData: endTime);
    }


    private void UpdateBtnActiveAndTimer()
    {
        if (GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockWeekendPack && (DateTime.Now.DayOfWeek == DayOfWeek.Sunday || DateTime.Now.DayOfWeek >= DayOfWeek.Friday))
        {
            // 计算当前的期数，加1的目的时为了使当前的期数默认为1，好与本地记录的期数做判断
            int giftPeriod = (DateTime.Now - signTime).Days / 7 + 1;

            // 购买过本期礼包 或者 周五15点前礼包
            //if (PlayerPrefs.GetInt("WeekendPicnicPeriod", 0) == giftPeriod ||
            //    (DateTime.Now.DayOfWeek == DayOfWeek.Friday && DateTime.Now.Hour < 15))
            //{
            //    // 隐藏入口按钮
            //    gameObject.SetActive(false);
            //    return;
            //}

            // 没有购买过，计算本期的开始时间和结束时间
            if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                endTime = DateTime.Now.AddDays(1).Date;
            else
                endTime = DateTime.Now.AddDays(7 - (int)DateTime.Now.DayOfWeek + 1).Date;

            gameObject.SetActive(true);
            countdownTimer.timeText.gameObject.SetActive(true);
            countdownTimer.StartCountdown(endTime);
            countdownTimer.CountdownOver += OnCountdownOver;
        }
        else// 非礼包开放时间，关闭礼包入口
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
