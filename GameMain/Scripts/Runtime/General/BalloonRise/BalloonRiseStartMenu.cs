using DG.Tweening;
using MySelf.Model;
using System;
using UnityEngine;

public class BalloonRiseStartMenu : PopupMenuForm
{
    public DelayButton tipButton;
    public DelayButton closeButton;
    public DelayButton startButton;
    public TextMeshProUGUILocalize text;
    public ClockBar clockBar;
    public Transform root;
    public Transform blackBG;
    public Transform guideArrow;
    public GameObject start;
    public GameObject lifeIcon;
    private Sequence m_GuideArrowSequence;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);
        tipButton.OnInit(OnTipButtonClick);
        closeButton.OnInit(OnClose);
        startButton.OnInit(OnStartButtonClick);

        BalloonRiseActivityManager balloonRiseManager = GameManager.Task.BalloonRiseManager;
        int stage = balloonRiseManager.Stage;
        if (balloonRiseManager.Upgrade)
            stage += 1;
        stage = Math.Clamp(stage, 1, 3);
        text.SetTerm($"Balloon.Start{stage}");

        clockBar.OnReset();
        clockBar.StartCountdown(GameManager.Task.BalloonRiseManager.EndTime);
        clockBar.CountdownOver += OnCountdownOver;
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        clockBar.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public override void OnReset()
    {
        base.OnReset();

        tipButton.OnReset();
        closeButton.OnReset();
        startButton.OnReset();
        clockBar.OnReset();

        if (m_GuideArrowSequence != null)
        {
            m_GuideArrowSequence.Kill();
        }
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        base.OnShow(showSuccessAction, userData);

        if (!GameManager.Task.BalloonRiseManager.ShowedStartMenu)
        {
            // if (m_GuideArrowSequence != null)
            // {
            //     m_GuideArrowSequence.Kill();
            // }
            // m_GuideArrowSequence = DOTween.Sequence();
            // Vector3 startPos = guideArrow.position;
            // Vector3 endPos = startPos + new Vector3(0, -0.1f, 0);
            // startButton.transform.SetParent(blackBG);
            // blackBG.gameObject.SetActive(true);
            start.SetActive(false);
            lifeIcon.SetActive(true);
            // m_GuideArrowSequence.Append(guideArrow.DOMove(endPos, 0.6f))
            //     .Append(guideArrow.transform.DOMove(startPos, 0.6f))
            //     .SetLoops(-1);
            // m_GuideArrowSequence.OnComplete(() => m_GuideArrowSequence = null).OnKill(() => m_GuideArrowSequence = null);
        }
        else
        {
            // startButton.transform.SetParent(root);
            // blackBG.gameObject.SetActive(false);
            start.SetActive(true);
            lifeIcon.SetActive(false);
            // if (m_GuideArrowSequence != null)
            // {
            //     m_GuideArrowSequence.Kill();
            // }
        }
    }

    public override void OnClose()
    {
        base.OnClose();
        GameManager.UI.HideUIForm(this);
        GameManager.Process.EndProcess(ProcessType.ShowBalloonRiseStartMenu);
    }

    private void OnTipButtonClick()
    {
        GameManager.UI.ShowUIForm("BalloonRiseRules");
    }

    private void OnStartButtonClick()
    {
        BalloonRiseModel.Instance.InitStage();
        GameManager.Task.AddDelayTriggerTask(0.01f, () =>
        {
            MapTopPanelManager mapTop = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
            mapTop.balloonRiseEntrance.OnInit(null);

            if (!GameManager.Task.BalloonRiseManager.ShowedStartMenu)
            {
                // startButton.transform.SetParent(root);
                // blackBG.gameObject.SetActive(false);
                // if (m_GuideArrowSequence != null)
                // {
                //     m_GuideArrowSequence.Kill();
                // }

                RewardManager.Instance.AddNeedGetReward(TotalItemData.InfiniteLifeTime, 60);
                RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, () =>
                {
                    GameManager.DataNode.SetData<int>("CurLevelPlayType", (int)LevelPlayType.Play);
                    GameManager.UI.ShowUIForm("LevelPlayMenu",UIFormType.PopupUI,
                        form =>
                        {
                            form.m_OnHideCompleteAction = () =>
                            {
                                GameManager.Process.EndProcess(ProcessType.ShowBalloonRiseStartMenu);
                            };
                        }, () => { GameManager.Process.EndProcess(ProcessType.ShowBalloonRiseStartMenu); });
                }, () => { }, () =>
                {
                    GameManager.UI.HideUIForm(this);
                    GameManager.Task.BalloonRiseManager.ShowedStartMenu = true;
                });
            }
            else
            {
                GameManager.UI.HideUIForm(this);
                GameManager.DataNode.SetData<int>("CurLevelPlayType", (int)LevelPlayType.Play);
                GameManager.UI.ShowUIForm("LevelPlayMenu",UIFormType.PopupUI,
                    form =>
                    {
                        form.m_OnHideCompleteAction = () =>
                        {
                            GameManager.Process.EndProcess(ProcessType.ShowBalloonRiseStartMenu);
                        };
                    }, () => { GameManager.Process.EndProcess(ProcessType.ShowBalloonRiseStartMenu); });
            }
        });
    }

    private void OnCountdownOver(object sender, CountdownOverEventArgs e)
    {
        OnClose();
    }
}
