using System;
using System.Collections;
using System.Collections.Generic;
using MySelf.Model;
using UnityEngine;

public class CardSetStartPanel : PopupMenuForm
{
    public DelayButton closeButton, greenButton;
    public TextMeshProUGUILocalize buttonText;
    public GameObject preview;
    public ClockBar clockBar;
    public GameObject start;
    
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);
        
        closeButton.OnInit(OnButtonClick);
        greenButton.OnInit(OnButtonClick);
        
        if (DateTime.Now < CardModel.Instance.CardStartTime)
        {
            buttonText.SetTerm("Common.OK");
            clockBar.OnReset();
            clockBar.CountdownOver += (sender, args) =>
            {
                GameManager.UI.HideUIForm(this);
                // CardModel.Instance.ShowedStartPanel = true;
                // SetStart();
            };
            clockBar.StartCountdown(CardModel.Instance.CardStartTime);
            preview.SetActive(true);
            start.SetActive(false);
        }
        else
        {
            SetStart();
            // GameManager.Sound.PlayMusic(CardModel.Instance.BgmName);
        }
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        clockBar.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    private void OnButtonClick()
    {
        GameManager.UI.HideUIForm(this);
        
        if (DateTime.Now >= CardModel.Instance.CardStartTime)
        {
            //原定弹出活动主界面
            // GameManager.UI.ShowUIForm<CardSetMainMenu>(form =>
            // {
            //     form.SetHideAction(() => GameManager.Process.EndProcess(ProcessType.ShowCardSetStartPanel));
            // });
            //现改为送卡包
            CardModel.Instance.ShowedStartPanel = true;
            RewardManager.Instance.AddNeedGetReward(TotalItemData.CardPack5, 1);
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.StartCardPackRewardPanel, false, () =>
            {
                MapTopPanelManager mapTopPanel = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
                mapTopPanel?.cardSetEntrance.ShowGuide();
            });
        }
    }

    private void SetStart()
    {
        buttonText.SetTerm("Card.Ready");
        clockBar.OnReset();
        preview.SetActive(false);
        start.SetActive(true);
    }
}
