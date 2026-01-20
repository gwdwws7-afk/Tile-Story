using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Firebase.Analytics;
using GameFramework.Event;
using MySelf.Model;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class PkGameEntrance : UIForm
{
    [SerializeField] private CountdownTimer Timer;
    [SerializeField] private DelayButton Btn;
    [SerializeField] private GameObject RedPoint;
    [SerializeField] private ParticleSystem ParticleSystem;
    [SerializeField] private GameObject PreviewBanner;
    [SerializeField] private Image MainImage;
    [SerializeField] private SkeletonGraphic Spine;
    [SerializeField] private Transform LockIcon;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        //先做一次期数判断去调整
        GameManager.Event.Subscribe(CommonEventArgs.EventId, CommonEvent);
        BlanceLastPkGame();
        SetActive();
        SetTimer();
        SetBtnEvent();
        //添加监听
        PkGameModel.Instance.AddListen();
        if(Timer.gameObject.activeInHierarchy)Timer.OnUpdate(0,0);
        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        if(gameObject.activeInHierarchy)CheckOffline();
        if(Timer.gameObject.activeInHierarchy)Timer.OnUpdate(elapseSeconds,realElapseSeconds);
        base.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public override void OnRelease()
    {
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId, CommonEvent);
        base.OnRelease();
    }
    public override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
    {
    }

    private void BlanceLastPkGame()
    {
        //1、如果上一期结束
        //2、如果上一期没有对手
        bool isLastOver = PkGameModel.Instance.Data.EndDateTime<DateTime.Now;
        bool isLastNoTargetPeople = PkGameModel.Instance.Data.TargetPlayerPkData.PlayerName == "???"&&
                                    PkGameModel.Instance.Data.TargetPlayerPkData.ItemNum==0;
        if(isLastOver&&isLastNoTargetPeople)
            PkGameModel.Instance.SetLastOpenPkDateTime();
    }

    private void SetActive()
    {
        bool isActivity = PkGameModel.Instance.IsActivityOpen;
        bool isCanOpen = PkGameModel.Instance.IsCanOpenByLevel&&
                         PkGameModel.Instance.IsCanOpenByTime;
        bool isCanPreview = PkGameModel.Instance.IsCanPreviewByLevel &&
                         PkGameModel.Instance.IsCanOpenByTime;

        gameObject.SetActive(isActivity || isCanOpen || isCanPreview);

        if (isActivity&&isCanOpen)
        {
            //如果是开启状态，切激活状态 ，做一次判断处理，重置段位
            PkGameModel.Instance.SetLastOpenPkDateTime();
        }

        bool isShowRedPoint = !PkGameModel.Instance.IsActivityOpen&&isCanOpen;
        if(RedPoint!=null)RedPoint.gameObject.SetActive(isShowRedPoint);

        if (isCanPreview && !isCanOpen)
        {
            PreviewBanner.SetActive(true);
            MainImage.color = Color.gray;
            Spine.color = Color.gray;
            Spine.freeze = true;
        }
        else
        {
            PreviewBanner.SetActive(false);
            MainImage.color = Color.white;
            Spine.color = Color.white;

            if(!SystemInfoManager.IsSuperLowMemorySize)
                Spine.freeze = false;
        }
    }

    private void SetTimer()
    {
        Timer.OnReset();

        DateTime endDateTime = PkGameModel.Instance.Data.EndDateTime != DateTime.MinValue
            ? PkGameModel.Instance.LocalEndDateTime
            : PkGameModel.Instance.CurActivityEndDateTime;

        Timer.StartCountdown(endDateTime);
        
        if(endDateTime<=DateTime.Now)return;
        Timer.CountdownOver += 
       (a,b)=>
        {
            if(!gameObject.activeInHierarchy)return;
            //是否需要结算
            if (!PkGameModel.Instance.IsActivityOpen)
            {
                PkGameModel.Instance.BalanceActivity();
                SetActive();
                SetTimer();
            }
            else if(PkGameModel.Instance.IsActivityOpen&&PkGameModel.Instance.PkGameStatus==PkGameStatus.Over)
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)return;
         
                //弹出结算框
                if (GameManager.Process.IsContainProcess(ProcessType.ShowPkGameOver)
                    || GameManager.Process.IsContainProcess(ProcessType.ShowPkGame)) return;

                var popupUIGroup = GameManager.UI.GetUIGroup(UIFormType.PopupUI);
                if (popupUIGroup != null && popupUIGroup.CurrentUIForm != null)
                    return;
                
                GameManager.Process.Unregister(ProcessType.ShowPkGame);
                GameManager.Process.Unregister(ProcessType.ShowPkGameOver);
                GameManager.Process.Register(ProcessType.ShowPkGameOver,0, () =>
                {
                    PkGameModel.Instance.RemoveListen();
                    Action finishAction = () =>
                    {
                        GameManager.Process.EndProcess(ProcessType.ShowPkGameOver);
                    };
                    switch (PkGameModel.Instance.PkGameOverStatus)
                    {
                        case PkGameOverStatus.Win:
                            GameManager.UI.ShowUIForm("PkGameWinPanel",u =>
                            {
                                u.SetHideAction(finishAction);
                            });
                            break;
                        case PkGameOverStatus.Deuce:
                            GameManager.UI.ShowUIForm("PkGameDeucePanel",u =>
                            {
                                u.SetHideAction(finishAction);
                            });
                            break;
                        case PkGameOverStatus.Lose:
                            GameManager.UI.ShowUIForm("PkGameFailPanel",u =>
                            {
                                u.SetHideAction(finishAction);
                            });
                            break;
                    }
                });
                GameManager.Task.AddDelayTriggerTask(1f, () =>
                {
                    GameManager.Process.ExecuteProcess();
                });
            }
        };
    }

    private void SetBtnEvent()
    {
        Btn.SetBtnEvent(() =>
        {
            if (!PkGameModel.Instance.IsCanOpenByLevel)
            {
                MapTopPanelManager mapTop = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
                if (mapTop != null && mapTop.gameObject.activeInHierarchy)
                {
                    LockIcon.DOShakePosition(0.2f, new Vector3(5, 0, 0), 20, 90, false, false, ShakeRandomnessMode.Harmonic);
                    mapTop.ShowUnlockPromptBox(PkGameModel.OpenPkGameLevel, UIFormType, transform.position);
                }
                return;
            }

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                //提示需要联网
                GameManager.UI.ShowWeakHint("PersonRank.Please check your internet connection",Vector3.zero);
            }
            else
            {
                if (PkGameModel.Instance.PkGameStatus == PkGameStatus.Over)
                {
                    PkGameModel.Instance.RemoveListen();
                    switch (PkGameModel.Instance.PkGameOverStatus)
                    {
                        case PkGameOverStatus.Win:
                            GameManager.UI.ShowUIForm("PkGameWinPanel");
                            break;
                        case PkGameOverStatus.Lose:
                            GameManager.UI.ShowUIForm("PkGameFailPanel");
                            break;
                        case PkGameOverStatus.Deuce:
                            GameManager.UI.ShowUIForm("PkGameDeucePanel");
                            break;
                    }
                }else
                {
                    //如果
                    if (!PkGameModel.Instance.IsActivityOpen)
                    {
                        //手动点击
                        GameManager.Firebase.RecordMessageByEvent("PKMatch_Show",
                            new Parameter("State",0));
                    }

                    GameManager.UI.ShowUIForm("PkGameStartPanel",UIFormType.PopupUI);
                }
            }
        });
    }

    private void CheckOffline()
    {
        if(Time.frameCount%30==0)
        Timer.SetOffline(Application.internetReachability==NetworkReachability.NotReachable);
    }

    private void CommonEvent(object sender, GameEventArgs e)
    {
        CommonEventArgs ne = (CommonEventArgs)e;
        if (ne.Type == CommonEventType.PkGameStart)
        {
            //游戏开始
            SetActive();
            SetTimer();
        }else if(ne.Type==CommonEventType.PkGameOver)
        {
            //重新刷新按钮状态
            SetActive();
            SetTimer();
            SetBtnEvent();
        }
    }
    
    public void FlyReward()
    {
        int flyNum = PkGameModel.Instance.PkItemFlyNum;
        PkGameModel.Instance.PkItemFlyNum = 0;
        if (PkGameModel.Instance.IsActivityOpen)
        {
            string key = "PkItem";
            EntranceFlyObjectManager.Instance.RegisterEntranceFlyEvent($"TotalItemAtlas[{key}]", flyNum, 22,
                new Vector3(230f, 0),
                Vector3.zero, gameObject,
                null, () =>
                {
                    if(ParticleSystem!=null)ParticleSystem.Play();
                    gameObject.transform.DOPunchScale(new Vector3(-0.1f, -0.1f), 0.1f, 1).onComplete = () =>
                    {
                        gameObject.transform.localScale = Vector3.one;
                        EntranceFlyObjectManager.Instance.EndEntranceFlyEvent();
                    };

                }, false);
        }
    }
}
