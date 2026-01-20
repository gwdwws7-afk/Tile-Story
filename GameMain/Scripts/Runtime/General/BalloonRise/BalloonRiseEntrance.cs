using DG.Tweening;
using GameFramework.Event;
using Spine.Unity;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BalloonRiseEntrance : EntranceUIForm
{
    public CountdownTimer countdownTimer;
    public Transform mainImageTransform;
    public ParticleSystem reachEffect;
    public TextMeshProUGUI rankText;
    public GameObject startBanner;
    public GameObject clockBanner;
    public GameObject previewBanner;
    public Image mainBg;
    public SkeletonGraphic spine;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        if (GameManager.Task.BalloonRiseManager.CheckIsOpen)
        {
            OnUnlocked();
            gameObject.SetActive(true);
        }
        else if (GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockPreviewBalloonRiseLevel && GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockBalloonRiseLevel && GameManager.DataTable.GetDataTable<DTBalloonRiseScheduleData>().Data.GetNowActiveActivityEndTime() != DateTime.MinValue)  
        {
            OnLocked();
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
            return;
        }
        SetTimer();

        GameManager.Task.BalloonRiseManager.UpdateRobotScore();
        GameManager.Event.Subscribe(OnRobotsScoreUpdateEventArgs.eventId, OnRobotsScoreUpdate);
        RefreshRankText();
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        countdownTimer.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public override void OnRelease()
    {
        base.OnRelease();

        countdownTimer.OnReset();
        GameManager.Event.Unsubscribe(OnRobotsScoreUpdateEventArgs.eventId, OnRobotsScoreUpdate);
    }

    public void SetTimer()
    {
        countdownTimer.OnReset();
        //没开stage显示start
        if (GameManager.Task.BalloonRiseManager.StageEndTime == DateTime.MinValue)
        {
            if(GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockBalloonRiseLevel)
            {
                countdownTimer.StartCountdown(GameManager.Task.BalloonRiseManager.EndTime);
                countdownTimer.CountdownOver += (sender, args) => { gameObject.SetActive(false); };
                clockBanner.SetActive(false);
                startBanner.SetActive(true);
            }
        }
        //开stage显示stage倒计时
        else
        {
            countdownTimer.StartCountdown(GameManager.Task.BalloonRiseManager.StageEndTime);
            countdownTimer.CountdownOver += (sender, args) =>
            {
                countdownTimer.timeText.SetText("00:00");
                //if (GameManager.Procedure.CurrentProcedure.ProcedureName == "ProcedureMap" && GameManager.Process.Count <= 0 && !GameManager.Network.CheckInternetIsNotReachable()) 
                //{
                //    var popupUIGroup = GameManager.UI.GetUIGroup("PopupUI");
                //    if (popupUIGroup != null && popupUIGroup.CurrentUIForm != null)
                //    {
                //        return;
                //    }

                //    MapTopPanelManager mapTop = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
                //    if (mapTop != null && mapTop.gameObject.activeInHierarchy) 
                //        GameManager.UI.ShowUIForm("BalloonRiseMainMenu");
                //}
            };
            startBanner.SetActive(false);
            clockBanner.SetActive(true);
        }
    }

    private void OnRobotsScoreUpdate(object sender, GameEventArgs e)
    {
        var ne = (OnRobotsScoreUpdateEventArgs)e;
        if (ne.RobotType != RobotType.BalloonRiseRobot) return;
        RefreshRankText();
    }

    public override void OnButtonClick()
    {
        if(GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockBalloonRiseLevel)
        {
            ShowUnlockPromptBox(Constant.GameConfig.UnlockBalloonRiseLevel);
            return;
        }

        if (startBanner.activeSelf)
        {
            GameManager.UI.ShowUIForm("BalloonRiseStartMenu");
        }
        else
        {
            GameManager.UI.ShowUIForm("BalloonRiseMainMenu");
        }
    }

    public override void OnLocked()
    {
        if (IsLocked)
            return;

        previewBanner.SetActive(true);

        mainBg.color = Color.gray;
        spine.color = Color.gray;
        spine.freeze = true;

        base.OnLocked();
    }

    public override void OnUnlocked()
    {
        if (!IsLocked)
            return;

        previewBanner.SetActive(false);

        mainBg.color = Color.white;
        spine.color = Color.white;
        spine.freeze = false;

        base.OnUnlocked();
    }

    public override void OnOnline()
    {
        base.OnOnline();

        rankText.text = GameManager.Task.BalloonRiseManager.Rank > 0 ? GameManager.Task.BalloonRiseManager.Rank.ToString() : "";
    }

    public override void OnOffline()
    {
        base.OnOffline();

        rankText.text = "";
    }

    private void RefreshRankText()
    {
        if (GameManager.Network.CheckInternetIsNotReachable())
        {
            rankText.text = "";
        }
        else
        {
            rankText.text = GameManager.Task.BalloonRiseManager.Rank > 0 ? GameManager.Task.BalloonRiseManager.Rank.ToString() : "";
        }
    }

    public void FlyReward()
    {
        EntranceFlyObjectManager.Instance.RegisterEntranceFlyEvent("TotalItemAtlas[Balloon]", 1, 21, new Vector3(250f, 0), Vector3.zero, gameObject,
            () =>
            {
                GameManager.Task.BalloonRiseManager.ScoreChange = false;
            }, () =>
            {
                EntranceFlyObjectManager.Instance.EndEntranceFlyEvent();

                mainImageTransform.DOScale(0.85f, 0.15f).SetEase(Ease.OutCubic).onComplete = () =>
                {
                    mainImageTransform.DOScale(1f, 0.15f);
                };
                if (reachEffect != null)
                {
                    reachEffect.Play();
                }
            });
    }
}
