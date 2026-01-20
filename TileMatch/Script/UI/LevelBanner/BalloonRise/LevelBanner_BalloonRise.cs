using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Firebase.Analytics;
using MySelf.Model;

public sealed class LevelBanner_BalloonRise : LevelBannerBase
{
    public TextMeshProUGUILocalize title;
    public GameObject startPanel;
    public DelayButton startButton;
    public GameObject start;
    public GameObject lifeIcon;
    public GameObject mainPanel;
    public BalloonRiseSmallPlayerPanel[] playerPanels;

    private bool isHide = true;
    
    protected override void OnInitialize()
    {
        BalloonRiseActivityManager balloonRiseManager = GameManager.Task.BalloonRiseManager;
        if (balloonRiseManager.StageEndTime == DateTime.MinValue)
        { 
            int stage = balloonRiseManager.Stage;
            if (balloonRiseManager.Upgrade)
                stage += 1;
            stage = Math.Clamp(stage, 1, 3);
            title.SetParameterValue("0", stage.ToString());
            
            startButton.OnInit(OnStartButtonClicked);
            start.SetActive(GameManager.Task.BalloonRiseManager.ShowedStartMenu);
            lifeIcon.SetActive(!start.activeSelf);
            startPanel.SetActive(true);
            mainPanel.SetActive(false);
        }
        else
        {
            title.SetParameterValue("0", balloonRiseManager.Stage.ToString());
            
            List<BalloonRisePlayerBase> playerDatas = balloonRiseManager.PlayerDatas;
            for (int i = 0; i < playerDatas.Count; i++)
            {
                playerPanels[i].OnInit(playerDatas[playerDatas.Count - 1 - i]);
            }
            startPanel.SetActive(false);
            mainPanel.SetActive(true);
        }
    }

    protected override void OnRelease()
    {
        startButton.OnReset();
        foreach (var child in playerPanels)
        {
            child.OnReset();
        }
    }

    private void OnStartButtonClicked()
    {
        // if (!GameManager.Task.BalloonRiseManager.ShowedStartMenu)
        // {
        //     RewardManager.Instance.AddNeedGetReward(TotalItemData.InfiniteLifeTime, 60);
        //     RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.TransparentRewardPanel, true, () =>
        //     {
        //         GameManager.Task.BalloonRiseManager.ShowedStartMenu = true;
        //     });
        // }
        
        BalloonRiseModel.Instance.InitStage();
        GameManager.Task.AddDelayTriggerTask(0.01f, () =>
        {
            MapTopPanelManager mapTop = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
            if (mapTop != null)
                mapTop.balloonRiseEntrance.OnInit(null);

            if (!GameManager.Task.BalloonRiseManager.ShowedStartMenu)
            {
                RewardManager.Instance.SaveRewardData(TotalItemData.InfiniteLifeTime, 60, true);
                GameManager.Task.BalloonRiseManager.ShowedStartMenu = true;
            }
        
            List<BalloonRisePlayerBase> playerDatas = GameManager.Task.BalloonRiseManager.PlayerDatas;
            for (int i = 0; i < playerDatas.Count; i++)
            {
                playerPanels[i].OnInit(playerDatas[playerDatas.Count - 1 - i]);
            }
            startPanel.SetActive(false);
            mainPanel.SetActive(true);
            
            GameManager.Firebase.RecordMessageByEvent("Balloon_BannerStart", new Parameter("Activity", GameManager.DataTable.GetDataTable<DTBalloonRiseScheduleData>().Data.GetNowActiveActivityID()));
        });
    }
}
