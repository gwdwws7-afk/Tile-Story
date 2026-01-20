using Firebase.Analytics;
using GameFramework.Event;
using MySelf.Model;
using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class BalloonRiseMainMenu : UIForm
{
    public DelayButton tipButton;
    public DelayButton closeButton;
    public DelayButton playButton;
    public GameObject play;
    public GameObject claim;
    public GameObject ok;
    public Image chest;
    public Button chestButton;
    public ItemPromptBox itemPromptBox;
    public ClockBar clockBar;
    public TextMeshProUGUILocalize title;
    public TextMeshProUGUILocalize text;
    public BalloonRiseSettlementPanel settlementPanel;
    public BalloonRisePlayerPanel[] playerPanels;

    private AsyncOperationHandle m_AssetHandle;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        tipButton.OnInit(OnTipButtonClick);
        playButton.OnInit(OnPlayButtonClick);
        closeButton.OnInit(OnCloseButtonClick);
        closeButton.interactable = true;
        settlementPanel.gameObject.SetActive(false);

        BalloonRiseActivityManager balloonRiseManager = GameManager.Task.BalloonRiseManager;
        SetChest(balloonRiseManager.Stage);
        chestButton.onClick.AddListener(OnChestButtonClick);

        title.SetParameterValue("0", balloonRiseManager.Stage.ToString());
        SetStateText();

        //try fix Index was out of range
        if (balloonRiseManager.RobotPlayers == null || balloonRiseManager.RobotPlayers.Count < 4)
        {
            var playerDatas = GameManager.Task.BalloonRiseManager.PlayerDatas;
        }

        if (playerPanels.Length >= 4 && balloonRiseManager.RobotPlayers.Count >= 4) 
        {
            playerPanels[0].OnInit(balloonRiseManager.RobotPlayers[0], this);
            playerPanels[1].OnInit(balloonRiseManager.RobotPlayers[1], this);
            playerPanels[2].OnInit(balloonRiseManager.SelfPlayer, this);
            playerPanels[3].OnInit(balloonRiseManager.RobotPlayers[2], this);
            playerPanels[4].OnInit(balloonRiseManager.RobotPlayers[3], this);
        }
        GameManager.Event.Subscribe(OnRobotsScoreUpdateEventArgs.eventId, OnRobotsScoreUpdate);

        clockBar.OnReset();
        if (!GameManager.Task.BalloonRiseManager.CheckIsFinishStage)
        {
            clockBar.StartCountdown(balloonRiseManager.StageEndTime);
            clockBar.CountdownOver += OnCountdownOver;    
        }
        else
        {
            clockBar.SetFinishState();
        }
        
        GameManager.Ads.HideBanner("BalloonRise");
    }

    private void SetStateText()
    {
        play.SetActive(true);
        claim.SetActive(false);
        ok.SetActive(false);
        playButton.gameObject.SetActive(true);
        
        if (GameManager.Task.BalloonRiseManager.CheckIsFinishStage)
        {
            if (GameManager.Task.BalloonRiseManager.Rank == 1)
            {
                play.SetActive(false);
                claim.SetActive(true);
                ok.SetActive(false);
                text.SetTerm("Balloon.MainWin");
            }
            else
            {
                play.SetActive(false);
                claim.SetActive(false);
                ok.SetActive(true);
                text.SetTerm("Balloon.MainLose");
            }

            playButton.transform.localScale = Vector3.zero;
        }
        else
        {
            text.SetTerm("Balloon.MainDes");
            text.SetParameterValue("0", GameManager.Task.BalloonRiseManager.StageTarget.ToString());
        }
    }

    private void OnRobotsScoreUpdate(object sender, GameEventArgs e)
    {
        var ne = (OnRobotsScoreUpdateEventArgs)e;
        if (ne.RobotType != RobotType.BalloonRiseRobot) return;
        for (int i = 0; i < 4; i++)
        {
            BalloonRisePlayerBase playerData = GameManager.Task.BalloonRiseManager.RobotPlayers[i];
            playerPanels[i].SetSlider(playerData);
            playerPanels[i].rankMark.gameObject.SetActive(playerData.Rank == 1 && playerData.Score != 0);
        }
        playerPanels[4].rankMark.gameObject.SetActive(GameManager.Task.BalloonRiseManager.Rank == 1 && GameManager.Task.BalloonRiseManager.Score != 0);
        SetStateText();
    }

    public override void OnReset()
    {
        clockBar.OnReset();
        
        base.OnReset();
    }

    public override void OnRelease()
    {
        base.OnRelease();

        tipButton.OnReset();
        playButton.OnReset();
        closeButton.OnReset();
        UnityUtility.UnloadAssetAsync(m_AssetHandle);
        m_AssetHandle = default;
        chestButton.onClick.RemoveAllListeners();
        itemPromptBox.OnRelease();
        clockBar.OnReset();
        foreach (var child in playerPanels)
        {
            child.OnReset();
        }
        GameManager.Event.Unsubscribe(OnRobotsScoreUpdateEventArgs.eventId, OnRobotsScoreUpdate);

        GameManager.Ads.ShowBanner("BalloonRise");
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);

        clockBar.OnUpdate(elapseSeconds, realElapseSeconds);

        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                itemPromptBox.HidePromptBox();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                itemPromptBox.HidePromptBox();
            }
        }
    }

    public override void OnPause()
    {
        base.OnPause();

        tipButton.interactable = false;
        playButton.interactable = false;
        closeButton.interactable = false;
    }

    public override void OnResume()
    {
        base.OnResume();

        tipButton.interactable = true;
        playButton.interactable = true;
        closeButton.interactable = true;
    }

#if UNITY_EDITOR
    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        base.OnShow(showSuccessAction, userData);

        GameManager.Sound.PlayMusic("SFX_Voyage_BGM", 0.5f);
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        base.OnHide(hideSuccessAction, userData);

        GameManager.Sound.PlayBgMusic(GameManager.PlayerData.BGMusicName);
    }
#endif

    public void ShowSettlementProcess()
    {
        if (GameManager.Task.BalloonRiseManager.Rank == 1)
        {
            ShowWinSettlementProcess();
        }
        else
        {
            ShowFailSettlementProcess();
        }
    }
    
    private void ShowWinSettlementProcess()
    {
        settlementPanel.chest.transform.position = chest.transform.position;
        settlementPanel.chest.transform.localScale = Vector3.one;
        settlementPanel.chest.sprite = chest.sprite;
        settlementPanel.winRoot.SetActive(true);
        settlementPanel.failRoot.SetActive(false);
        settlementPanel.winEffect.SetActive(false);
        settlementPanel.black.color = new Color(0, 0, 0, 0);
        settlementPanel.winText1.localScale = Vector3.zero;
        settlementPanel.winText2.localScale = Vector3.zero;
        playButton.transform.localScale = Vector3.zero;
        settlementPanel.Init(playerPanels);
        settlementPanel.gameObject.SetActive(true);
        chest.gameObject.SetActive(false);
        settlementPanel.black.DOFade(0.7f, 0.3f);
        Transform chestTrans = settlementPanel.chest.transform;
        chestTrans.transform.DOScale(1.2f, 0.4f);
        chestTrans.DOLocalJump(new Vector3(-3, -215, 0), 700,1, 0.55f);

        GameManager.Task.AddDelayTriggerTask(0.5f, () =>
        {
            float targetScale = 1.2f;
            chestTrans.DOScale(new Vector3(targetScale*1.06f, targetScale*0.94f, 1f), 0.13f).onComplete = () =>
            {
                chestTrans.DOScale(new Vector3(targetScale*0.97f, targetScale*1.03f, 1f), 0.13f).onComplete = () =>
                {
                    chestTrans.DOScale(targetScale, 0.13f);
                };
            };

            settlementPanel.winText1.DOScale(1f, 0.3f).SetEase(Ease.OutBack, 3).SetDelay(0.1f);
            settlementPanel.winText2.DOScale(1f, 0.3f).SetEase(Ease.OutBack,3).SetDelay(0.3f);
            playButton.transform.DOScale(1.1f, 0.2f).SetDelay(0.5f).onComplete = () =>
            {
                playButton.transform.DOScale(1f, 0.2f);
            };
            OnResume();
            tipButton.interactable = false;
        });

        GameManager.Task.AddDelayTriggerTask(0.6f, () =>
        {
            settlementPanel.winEffect.SetActive(true);
        });
    }

    private void ShowFailSettlementProcess()
    {
        settlementPanel.winRoot.SetActive(false);
        settlementPanel.failRoot.SetActive(true);
        settlementPanel.black.color = new Color(0, 0, 0, 0);
        settlementPanel.failText1.localScale = Vector3.zero;
        settlementPanel.failText2.localScale = Vector3.zero;
        playButton.transform.localScale = Vector3.zero;
        settlementPanel.Init(playerPanels);
        settlementPanel.gameObject.SetActive(true);
        settlementPanel.black.DOFade(0.7f, 0.2f);

        GameManager.Task.AddDelayTriggerTask(0.1f, () =>
        {
            settlementPanel.failText1.DOScale(1f, 0.3f).SetEase(Ease.OutBack, 3);
            settlementPanel.failText2.DOScale(1f, 0.3f).SetEase(Ease.OutBack,3).SetDelay(0.2f);
            playButton.transform.DOScale(1.1f, 0.2f).SetDelay(0.4f).onComplete = () =>
            {
                playButton.transform.DOScale(1f, 0.2f);
            };
            OnResume();
            tipButton.interactable = false;
        });
    }
    
    public void OnCloseButtonClick()
    {
        closeButton.interactable = false;
        
        bool checkIsFinishStage = GameManager.Task.BalloonRiseManager.CheckIsFinishStage;
        int rank = GameManager.Task.BalloonRiseManager.Rank;

        if (!checkIsFinishStage || rank != 1) 
            GameManager.UI.HideUIForm(this);
        
        //有人赢
        if (checkIsFinishStage)
        {
            //自己赢
            if (rank == 1)
            {
                int stageNum = GameManager.Task.BalloonRiseManager.Stage;
                BalloonRiseStage stage = GameManager.DataTable.GetDataTable<DTBalloonRiseReward>().Data.GetRewardByStageNum(stageNum);
                RewardManager.Instance.AddNeedGetReward(stage.RewardTypeList, stage.RewardNumList);
                GameManager.DataNode.SetData<Vector3>("BalloonChestPos", settlementPanel.chest.transform.position);
                RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.BalloonRiseChestRewardPanel, false, () =>
                {
                    GameManager.Task.BalloonRiseManager.Upgrade = true;
                    GameManager.Firebase.RecordMessageByEvent("Balloon_Finish", new Parameter("Stage", stageNum));
                    BalloonRiseModel.Instance.ResetStage();

                    UIGroup uiGroup = GameManager.UI.GetUIGroup(UIFormType.LeftUI);
                    MapTopPanelManager mapTopPanelManager = (MapTopPanelManager)GameManager.UI.GetUIForm("MapTopPanelManager");
                    mapTopPanelManager.balloonRiseEntrance.OnInit(uiGroup);

                    GameManager.Process.EndProcess(ProcessType.ShowBalloonRiseRewardProcess);
                });
            }
            //别人赢
            else
            {
                GameManager.Task.BalloonRiseManager.Upgrade = false;
                BalloonRiseModel.Instance.ResetStage();

                UIGroup uiGroup = GameManager.UI.GetUIGroup(UIFormType.LeftUI);
                MapTopPanelManager mapTopPanelManager = (MapTopPanelManager)GameManager.UI.GetUIForm("MapTopPanelManager");
                mapTopPanelManager.balloonRiseEntrance.OnInit(uiGroup);

                GameManager.Process.EndProcess(ProcessType.ShowBalloonRiseRewardProcess);
            }
        }
        //没人赢
        else if (DateTime.Now > GameManager.Task.BalloonRiseManager.StageEndTime)
        {
            GameManager.Task.BalloonRiseManager.Upgrade = false;
            BalloonRiseModel.Instance.ResetStage();

            UIGroup uiGroup = GameManager.UI.GetUIGroup(UIFormType.LeftUI);
            MapTopPanelManager mapTopPanelManager = (MapTopPanelManager)GameManager.UI.GetUIForm("MapTopPanelManager");
            mapTopPanelManager.balloonRiseEntrance.OnInit(uiGroup);

            GameManager.Process.EndProcess(ProcessType.ShowBalloonRiseRewardProcess);
        }
    }

    private void OnTipButtonClick()
    {
        GameManager.UI.ShowUIForm("BalloonRiseRules");
    }

    private void OnPlayButtonClick()
    {
        if (play.activeSelf)
        {
            GameManager.UI.HideUIForm(this);
            GameManager.DataNode.SetData<int>("CurLevelPlayType", (int)LevelPlayType.Play);
            GameManager.UI.ShowUIForm("LevelPlayMenu",UIFormType.PopupUI);
        }
        else
        {
            OnCloseButtonClick();
        }
    }

    private void SetChest(int stage)
    {
        string chestSp = "宝箱1";
        switch (stage)
        {
            case 2:
                chestSp = "宝箱2";
                break;
            case 3:
                chestSp = "宝箱3";
                break;
        }

        chest.sprite = null;
        UnityUtility.UnloadAssetAsync(m_AssetHandle);
        m_AssetHandle = UnityUtility.LoadAssetAsync<Sprite>($"BalloonRise[{chestSp}]", sp =>
        {
            chest.sprite = sp;
            chest.SetNativeSize();
            chest.gameObject.SetActive(true);
        });
    }

    private void OnChestButtonClick()
    {
        int stageNum = GameManager.Task.BalloonRiseManager.Stage;
        BalloonRiseStage stage = GameManager.DataTable.GetDataTable<DTBalloonRiseReward>().Data.GetRewardByStageNum(stageNum);
        itemPromptBox.boxMaxWidth = 600;
        if (stage.RewardTypeList.Count == 6)
            itemPromptBox.boxMaxWidth = 400;
        itemPromptBox.Init(stage.RewardTypeList, stage.RewardNumList);
        itemPromptBox.ShowPromptBox(PromptBoxShowDirection.Down, chest.transform.position);
    }

    private void OnCountdownOver(object sender, CountdownOverEventArgs e)
    {
        //clockBar.timer.timeText.SetText("00:00");
        clockBar.SetFinishState();
        
        if (GameManager.Task.BalloonRiseManager.CheckIsFinishStage)
        {
            if (GameManager.Task.BalloonRiseManager.Rank == 1)
            {
                play.SetActive(false);
                claim.SetActive(true);
                ok.SetActive(false);
                text.SetTerm("Balloon.MainWin");
            }
            else
            {
                play.SetActive(false);
                claim.SetActive(false);
                ok.SetActive(true);
                text.SetTerm("Balloon.MainLose");
            }

            playButton.transform.localScale = Vector3.zero;
            ShowSettlementProcess();
        }
        else
        {
            text.SetTerm("Balloon.MainDes");
            text.SetParameterValue("0", GameManager.Task.BalloonRiseManager.StageTarget.ToString());
            play.SetActive(false);
            claim.SetActive(false);
            ok.SetActive(true);
            playButton.transform.localScale = Vector3.zero;
            ShowFailSettlementProcess();
        }
        
        // OnClose();
        // if (GameManager.Task.BalloonRiseManager.StageEndTime != DateTime.MinValue)
        // {
        //     BalloonRiseModel.Instance.ResetStage();
        //     UIGroup uiGroup = GameManager.UI.GetUIGroup(UIFormType.LeftUI);
        //     MapTopPanelManager mapTopPanelManager = (MapTopPanelManager)GameManager.UI.GetUIForm<MapTopPanelManager>();
        //     mapTopPanelManager.balloonRiseEntrance.OnInit(uiGroup);
        // }
    }
}
