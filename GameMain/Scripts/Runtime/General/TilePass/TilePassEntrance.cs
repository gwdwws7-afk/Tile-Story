using DG.Tweening;
using MySelf.Model;
using System;
using TMPro;
using UnityEngine;

public class TilePassEntrance : EntranceUIForm
{
    public CountdownTimer countdownTimer;
    public Transform mainImageTransform;
    public ParticleSystem reachEffect;
    public GameObject warningSign;
    public TextMeshProUGUI warningText;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        //无排期
        if (TilePassModel.Instance.EndTime == DateTime.MinValue)
        {
            gameObject.SetActive(false);
            return;
        }

        SetTimer();
        RefreshRewardGetWarning();
    }

    public override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
    {
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

        warningSign.transform.DOKill();
        warningSign.transform.localScale = Vector3.one;
    }

    public void SetTimer()
    {
        if (DateTime.Now > TilePassModel.Instance.EndTime)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
            countdownTimer.OnReset();
            countdownTimer.StartCountdown(TilePassModel.Instance.EndTime);
            countdownTimer.CountdownOver += OnCountdownOver;
        }
    }

    private void OnCountdownOver(object sender, CountdownOverEventArgs e)
    {
        gameObject.SetActive(false);
    }

    public override void OnButtonClick()
    {
        GameManager.UI.ShowUIForm("TilePassMainMenu");
    }

    public void FlyReward()
    {
        EntranceFlyObjectManager.Instance.RegisterEntranceFlyEvent("TotalItemAtlas[TilePassTarget]", TilePassModel.Instance.LevelCollectTargetNum, 21, new Vector3(-250f, 0), Vector3.zero, gameObject,
            () =>
            {
                TilePassModel.Instance.TotalTargetNum += TilePassModel.Instance.LevelCollectTargetNum;
                TilePassModel.Instance.LevelCollectTargetNum = 0;
            }, () =>
            {
                warningSign.transform.DOScale(Vector3.zero, 0.15f).SetEase(Ease.OutCubic).onComplete = () =>
                {
                    ShowTilePassMainMenuByUnClaimedRewardNum();
                    RefreshRewardGetWarning();
                    warningSign.transform.DOScale(Vector3.one, 0.15f).onComplete = () =>
                    {
                        EntranceFlyObjectManager.Instance.EndEntranceFlyEvent();
                    };
                };

                mainImageTransform.DOScale(new Vector3(0.85f, 0.85f, 0.85f), 0.15f).SetEase(Ease.OutCubic).onComplete = () =>
                {
                    mainImageTransform.DOScale(new Vector3(1f, 1f, 1f), 0.15f);
                };
                if (reachEffect != null)
                {
                    reachEffect.Play();
                }
            }, false);
    }

    public void RefreshRewardGetWarning()
    {
        int canGetRewardCount = TilePassModel.Instance.CheckUnclaimedRewards();

        if (canGetRewardCount > 0)
        {
            warningText.text = canGetRewardCount.ToString();
            warningSign.gameObject.SetActive(true);
        }
        else
        {
            warningSign.gameObject.SetActive(false);
        }
    }

    private void ShowTilePassMainMenuByUnClaimedRewardNum()
    {
        int canGetRewardCount = TilePassModel.Instance.CheckUnclaimedRewards();
        if (TilePassModel.Instance.IsNeedOpenTilePassPanelByUnClaimedRewardNum(canGetRewardCount))
        {
            //添加弹出界面进程
            GameManager.Process.Unregister(ProcessType.ShowTilePassPanel);
            GameManager.Process.Register(ProcessType.ShowTilePassPanel.ToString(), 1, () =>
             {
                 //自动弹
                 TilePassModel.Instance.RecordLastUnClaimedRewardNum();
                 GameManager.UI.ShowUIForm("TilePassMainMenu",u =>
                 {
                     u.SetHideAction(() =>
                     {
                         GameManager.Process.EndProcess(ProcessType.ShowTilePassPanel);
                     });
                 });
             });
            GameManager.Process.ExecuteProcess();
        }
    }

}
