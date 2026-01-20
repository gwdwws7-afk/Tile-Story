using Firebase.Analytics;
using GameFramework.Event;
using GameFramework.Fsm;
using GameFramework.Procedure;
using MySelf.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ProcedureSkipType
{
    None,
    MenuToMap,
    MenuToGame,
    GameToMap,
}

/// <summary>
/// 地图场景开始流程
/// </summary>
public sealed class ProcedureExecuteProcess : ProcedureBase
{
    public override string ProcedureName => "ProcedureExecuteProcess";

    private bool isLevelWinBack = false;
    private bool isToGame = false;

    public override void OnEnter(IFsm<ProcedureManager> fsm)
    {
        GameManager.Event.Subscribe(CommonEventArgs.EventId, GoToGame);

        isLevelWinBack = false;
        ShowProcedureRegisterProcess(fsm);
        MapTopPanelManager topPanel = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
        if (topPanel != null) topPanel.OnPause();
        base.OnEnter(fsm);
    }

    public override void OnLeave(IFsm<ProcedureManager> fsm, bool isShutdown)
    {
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId, GoToGame);

        isToGame = false;

        base.OnLeave(fsm, isShutdown);
    }

    public override void OnUpate(IFsm<ProcedureManager> fsm, float elapseSeconds, float realElapseSeconds)
    {
        if (isToGame)
        {
            isToGame = false;
            ChangeState<ProcedureGame>(fsm);
            return;
        }

        if (GameManager.Process.Count <= 0)
        {
            ChangeProcedure(fsm);
        }

        base.OnUpate(fsm, elapseSeconds, realElapseSeconds);
    }

    private void ChangeProcedure(IFsm<ProcedureManager> fsm)
    {
        ProcedureSkipType skipType = fsm.GetData<ProcedureSkipType>("ProcedureSkipType");

        fsm.SetData("ProcedureSkipType", ProcedureSkipType.None);
        switch (skipType)
        {
            case ProcedureSkipType.None:
            case ProcedureSkipType.MenuToMap:
            case ProcedureSkipType.GameToMap:
                ChangeState<ProcedureMap>(fsm);
                break;
            case ProcedureSkipType.MenuToGame:
                ChangeState<ProcedureGame>(fsm);
                break;
            default:
                ChangeState<ProcedureMap>(fsm);
                break;
        }
    }

    private void ShowProcedureRegisterProcess(IFsm<ProcedureManager> fsm)
    {
        ProcedureSkipType skipType = fsm.GetData<ProcedureSkipType>("ProcedureSkipType");

        switch (skipType)
        {
            case ProcedureSkipType.MenuToMap:
                //记录一个关卡数
                GameManager.DataNode.SetData("NowLevel", GameManager.PlayerData.NowLevel);
                RegisterMenuToMapProcess();
                break;
            case ProcedureSkipType.GameToMap:
                PiggyBankModel.Instance.RecordGameToMapCountWithFullStatus();
                GameManager.PlayerData.RecordGameToMapCountByDay();

                RegisterGameToMapProcess();
                break;
        }
        
    }

    private void RegisterMenuToMapProcess()
    {
        RegisterProcess(ProcessType.ShowPkGame, 1, ShowPkGame);
        RegisterProcess(ProcessType.CheckDecorationAreaCompleteReward, 5, CheckDecorationAreaCompleteReward);
        RegisterProcess(ProcessType.ShowActivityPackWhenBackToMap, 2, RegisterActivityPackAutoPop);
        RegisterProcess(ProcessType.PersonRankStart, 2, ShowPersonRankStartMenu);
        RegisterProcess(ProcessType.EndlessChestAutoOpen, 3, RegisterAutoEndlessChestPanel);
        RegisterProcess(ProcessType.PersonRankFinish, 20, SendPersonRankRewards);
        RegisterProcess(ProcessType.ShowPersonRankChangeName, 21, PersonRankChangeName);
        RegisterProcess(ProcessType.ShowCalendarChallengeMenu, 22, ShowCalendarChallengeMenu);
        RegisterProcess(ProcessType.ShowCalendarChallengeGuide, 29, ShowCalendarChallengeGuide);
        RegisterProcess(ProcessType.GlacierQuest, 30, CheckGlacierQuest);
        RegisterProcess(ProcessType.ShowGoldCollectionStartPanel, 19, ShowGoldCollectionStartPanel);
        RegisterProcess(ProcessType.ShowGoldCollectionEndPanel, 20, ShowGoldCollectionEndPanel);
        RegisterProcess(ProcessType.ResetGoldCollectionData, 21, ResetGoldCollectionData);
        RegisterProcess(ProcessType.ShowTilePassStartMenu, 23, ShowTilePassStartMenu);
        RegisterProcess(ProcessType.ShowTilePassEndProcess, 24, ShowTilePassEndProcess);
        RegisterProcess(ProcessType.ResetTilePassData, 25, ResetTilePassData);
        RegisterProcess(ProcessType.ShowBalloonRiseStartMenu, 19, RegisterBalloonRiseStartProcess);
        RegisterProcess(ProcessType.ShowBalloonRiseRewardProcess, 20, RegisterBalloonRiseRewardProcess);
        
        RegisterProcess(ProcessType.ShowPiggyBankMenuProcess, 30, AutoShowPiggyBankMenuByFirstEnterGame);
        
        RegisterProcess(ProcessType.BackPayCardPack, 100, BackPayCardPack);
        RegisterProcess(ProcessType.ShowCardSetPreviewPanel, 21, ShowCardSetPreviewPanel);
        RegisterProcess(ProcessType.ShowCardSetStartPanel, 21, ShowCardSetStartPanel);
        RegisterProcess(ProcessType.ShowCardSetCountDownPanel, 22, ShowCardSetCountDownPanel);
        RegisterProcess(ProcessType.ShowCardSetEndPanel, 20, ShowCardSetEndPanel);
        
        RegisterProcess(ProcessType.ActivityAfterStartProcess, 38, ActivityAfterStartProcess);
        RegisterProcess(ProcessType.ActivityStartProcess, 39, ActivityStartProcess);
        RegisterProcess(ProcessType.ActivityEndProcess, 40, ActivityEndProcess);
        RegisterProcess(ProcessType.ActivityPreEndProcess, 41, ActivityPreEndProcess);
        RegisterProcess(ProcessType.EntranceFlyObjects, 99, RegisterEntranceObjectsFlyProcess);
        bool isLock = true; //GoldCollectionEntrance.IsHaveRewardByGoldCollection();
        GameManager.Process.RegisterAfter(
            ProcessType.EntranceFlyObjects.ToString(), 
            ProcessType.ShowGoldCollectionReward, 
            RegisterGoldCollectionSliderAnim, 
            null, 
            () => isLock);
        
        RegisterProcess(ProcessType.EndlessChest, 100, RegisterEndlessChestRewardPanelByMenuToMap);
        RegisterProcess(ProcessType.ShowGameToMapReward, 101, ShowGameToMapReward);

        GameManager.Process.ExecuteProcess();
    }

    private void RegisterGameToMapProcess()
    {
        RegisterProcess(ProcessType.MapLevelBtnUpdateAnim, GetLevelBtnUpdateAnimPriority(), MapLevelBtnUpdateAnim);
//#if !AmazonStore && !UNITY_IOS
//        RegisterProcess(ProcessType.ShowLoginGuide,1,RegisterLoginGuide);
//#endif
        
        RegisterProcess(ProcessType.ShowPkGame, 1, ShowPkGame2);
        RegisterProcess(ProcessType.ShowActivityPackWhenBackToMap, 2, RegisterActivityPackAutoPop);
        RegisterProcess(ProcessType.PersonRankStart, 2, ShowPersonRankStartMenu);
        RegisterProcess(ProcessType.EndlessChestAutoOpen, 3, RegisterAutoEndlessChestPanel);
        RegisterProcess(ProcessType.ShowDaliyGift, 5, ShowLoginGift);
        RegisterProcess(ProcessType.ShowRemoveAdsMenuWhenBackToMap, 11, RegisterShowRemovePopupAdsMenu);
        RegisterProcess(ProcessType.PersonRankFinish, 20, SendPersonRankRewards);
        RegisterProcess(ProcessType.ShowPersonRankChangeName, 21, PersonRankChangeName);
        RegisterProcess(ProcessType.ShowCalendarChallengeMenu, 22, ShowCalendarChallengeMenu);
        RegisterProcess(ProcessType.ShowCalendarChallengeGuide, 29, ShowCalendarChallengeGuide);
        RegisterProcess(ProcessType.GlacierQuest, 30, CheckGlacierQuest);
        RegisterProcess(ProcessType.ShowGoldCollectionStartPanel, 19, ShowGoldCollectionStartPanel);
        RegisterProcess(ProcessType.ShowGoldCollectionEndPanel, 20, ShowGoldCollectionEndPanel);
        RegisterProcess(ProcessType.ResetGoldCollectionData, 21, ResetGoldCollectionData);
        RegisterProcess(ProcessType.ShowTilePassStartMenu, 23, ShowTilePassStartMenu);
        RegisterProcess(ProcessType.ShowTilePassEndProcess, 24, ShowTilePassEndProcess);
        RegisterProcess(ProcessType.ResetTilePassData, 25, ResetTilePassData);
        RegisterProcess(ProcessType.ShowBalloonRiseStartMenu, 19, RegisterBalloonRiseStartProcess);
        RegisterProcess(ProcessType.ShowBalloonRiseRewardProcess, 20, RegisterBalloonRiseRewardProcess);
        
        RegisterProcess(ProcessType.ShowPiggyBankMenuProcess, 30, AutoShowPiggyBankMenu);
        
        RegisterProcess(ProcessType.BackPayCardPack, 100, BackPayCardPack);
        RegisterProcess(ProcessType.ShowCardSetPreviewPanel, 21, ShowCardSetPreviewPanel);
        RegisterProcess(ProcessType.ShowCardSetStartPanel, 21, ShowCardSetStartPanel);
        RegisterProcess(ProcessType.ShowCardSetCountDownPanel, 22, ShowCardSetCountDownPanel);
        RegisterProcess(ProcessType.ShowCardSetEndPanel, 20, ShowCardSetEndPanel);
        
        RegisterProcess(ProcessType.ActivityAfterStartProcess, 38, ActivityAfterStartProcess);
        RegisterProcess(ProcessType.ActivityStartProcess, 39, ActivityStartProcess);
        RegisterProcess(ProcessType.ActivityEndProcess, 40, ActivityEndProcess);
        RegisterProcess(ProcessType.ActivityPreEndProcess, 41, ActivityPreEndProcess);
        RegisterProcess(ProcessType.ShowRate, 98, ShowRate);
        RegisterProcess(ProcessType.EntranceFlyObjects, 99, RegisterEntranceObjectsFlyProcess);
        bool isLock = GoldCollectionEntrance.IsHaveRewardByGoldCollection();
        GameManager.Process.RegisterAfter(ProcessType.EntranceFlyObjects.ToString(), ProcessType.ShowGoldCollectionReward, RegisterGoldCollectionSliderAnim, null, () => isLock);
        RegisterProcess(ProcessType.ShowGameToMapReward, 100, ShowGameToMapReward);

        GameManager.Process.ExecuteProcess();
    }

    private void RegisterProcess(ProcessType processType, int priority, Action action)
    {
        GameManager.Process.Register(
            processType, 
            priority, 
            action, 
            () => IsLockProcess(processType));
    }

    private int GetLevelBtnUpdateAnimPriority()
    {
        return (IsLockProcess(ProcessType.ShowObjectiveGuide) ||
                IsLockProcess(ProcessType.CheckClimbBeanstalk))
            ? 1
            : 1000;
    }

    private bool IsLockProcess(ProcessType type)
    {
        switch (type)
        {
            case ProcessType.CheckClimbBeanstalk:
                return ClimbBeanstalkManager.Instance.CheckActivityStateNeedToLockProcess();
            case ProcessType.ShowRate:
                return IsShowRate();
            case ProcessType.EntranceFlyObjects:
            case ProcessType.ShowGoldCollectionReward:
            case ProcessType.ShowGameToMapReward:
                return true;
            default:
                return false;
        }
    }

    private void RegisterPersonRankFlyMedal()
    {
        GameManager.Task.PersonRankManager.CheckIsOpen();
        if (GameManager.Task.PersonRankManager.TaskState == PersonRankState.Playing ||
            GameManager.Task.PersonRankManager.TaskState == PersonRankState.None)
        {
            var mapTop = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
            mapTop.personRankEntrance.FlyMedal();
        }
    }

    private void RegisterClimbBeanstalkFlyReward()
    {
        if (ClimbBeanstalkManager.Instance.CheckActivityHasStarted())
        {
            MapTopPanelManager mapTop = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
            mapTop.climbBeanstalkEntrance.FlyReward();
        }
    }

    private void RegisterKitchenFlyReward()
    {
        if (KitchenManager.Instance.CheckActivityHasStarted())
        {
            MapTopPanelManager mapTop = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
            mapTop.kitchenEntrance.FlyReward();
        }
    }
    
    private void RegisterHarvestKitchenFlyReward()
    {
        if (HarvestKitchenManager.Instance.CheckActivityHasStarted())
        {
            MapTopPanelManager mapTop = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
            mapTop.harvestKitchenEntrance.FlyReward();
        }
    }

    private bool IsShowRate()
    {
        return GameManager.PlayerData.IsShowRateByLevel(GameManager.PlayerData.NowLevel, false) ||
               GameManager.PlayerData.IsShowRateByWinCount(false) ||
               GameManager.PlayerData.IsShowRateByUnlockBG(false);
    }

    private void ShowRate()
    {
        GameManager.UI.HideUIForm("GlobalMaskPanel");
        if ((GameManager.PlayerData.IsShowRateByLevel(GameManager.PlayerData.NowLevel,true) ||
            GameManager.PlayerData.IsShowRateByWinCount(true) ||
            GameManager.PlayerData.IsShowRateByUnlockBG(true)))
        {
            if (GameManager.Firebase.GetBool(Constant.RemoteConfig.If_Show_Rate_Us_Menu, true))
            {
                GameManager.UI.ShowUIForm("RateUsMenu",(u) =>
                {
                    u.SetHideAction(() => { GameManager.Process.EndProcess(ProcessType.ShowRate); });
                });
            }
            else
            {
                GameManager.UI.ShowUIForm("EvaluationMenu",(u) =>
                {
                    u.SetHideAction(() => { GameManager.Process.EndProcess(ProcessType.ShowRate); });
                });
            }
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.ShowRate);
        }
    }

    private void ShowGameToMapReward()
    {
        void ShowGetLevelReward()
        {
            int levelWinCoin = PlayerPrefs.GetInt("LevelWinCoin", 0);
            int levelWinStar = PlayerPrefs.GetInt("LevelWinStar", 0);
            if (levelWinCoin > 0 || levelWinStar > 0)
            {
                isLevelWinBack = true;
                RewardManager.Instance.AddNeedGetReward(TotalItemData.Coin, levelWinCoin);
                RewardManager.Instance.AddNeedGetReward(TotalItemData.Star, levelWinStar);
                PlayerPrefs.SetInt("LevelWinCoin", 0);
                PlayerPrefs.SetInt("LevelWinStar", 0);

                RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.None, true, () =>
                {
                    GameManager.Process.EndProcess(ProcessType.ShowGameToMapReward);
                    SyncDataToServer();
                    ((MapTopPanelManager)GameManager.UI.GetUIForm("MapTopPanelManager")).UpdateHelpBtn();
                });
            }
            else
            {
                GameManager.Process.EndProcess(ProcessType.ShowGameToMapReward);
                ((MapTopPanelManager)GameManager.UI.GetUIForm("MapTopPanelManager")).UpdateHelpBtn();
            }
        }

        int levelRewardId = PlayerPrefs.GetInt("CanGetLevelRewardId", 0);
        if (levelRewardId > 0)
        {
            DTLevelReward levelRewardDataTable = GameManager.DataTable.GetDataTable<DTLevelReward>().Data;
            LevelRewardData levelRewardData = levelRewardDataTable.GetLevelReward(levelRewardId);
            if (levelRewardData != null)
            {
                List<ItemData> datas = levelRewardData.GetRewardDatas();
                PlayerPrefs.SetInt("CanGetLevelRewardId", 0);
                GameManager.UI.HideUIForm("GlobalMaskPanel");

                for (int i = 0; i < datas.Count; i++)
                {
                    RewardManager.Instance.AddNeedGetReward(datas[i].type, datas[i].num);
                }
                RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.ChestRewardPanel, false, () =>
                {
                    ShowGetLevelReward();
                });
            }
            else
            {
                ShowGetLevelReward();
            }
        }
        else
        {
            ShowGetLevelReward();
        }
    }

    private void MapLevelBtnUpdateAnim()
    {
        GameManager.Process.EndProcess(ProcessType.MapLevelBtnUpdateAnim);

        if (GameManager.DataNode.GetData<int>("NowLevel", GameManager.PlayerData.NowLevel) ==
            GameManager.PlayerData.NowLevel)
        {
            return;
        }

        var mapToTop = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
        if (mapToTop != null)
        {
            mapToTop.ShowLevelBtnAnim(null);
        }
    }
    
    private void ShowCalendarChallengeGuide()
    {
        if (GameManager.PlayerData.NowLevel>= Constant.GameConfig.UnlockCalendarChallengeLevel && !GameManager.Task.CalendarChallengeManager.HasShowedCalendarChallengeGuide)
        {
            var mapToTop=GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
            GameManager.UI.HideUIForm("GlobalMaskPanel");
            if (mapToTop.calendarChallengeEntranceBtn) mapToTop.calendarChallengeEntranceBtn.ShowGuide();
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.ShowCalendarChallengeGuide);
        }
    }

    private void ShowLoginGift()
    {
        bool isUnlock = GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockLoginGiftLevel;
        bool isShowedLoginGift = GameManager.DataNode.GetData<bool>("IsShowedLoginGift", false);
        bool isAutoOpen = isUnlock && !isShowedLoginGift && GameManager.PlayerData.NeedShowLoginGiftByToday();
        if (isAutoOpen)
        {
            GameManager.DataNode.SetData<bool>("IsShowedLoginGift", true);
            GameManager.UI.ShowUIForm("LoginGiftPanel",(u) =>
            {
                u.SetHideAction(() => { GameManager.Process.EndProcess(ProcessType.ShowDaliyGift); });
            });
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.ShowDaliyGift);
        }
    }
    
    #region PiggyBank

    private void AutoShowPiggyBankMenu()
    {
        if (PiggyBankModel.Instance.IsShowAutoOpenPanelByGameToMap)
        {
            //展示
            PiggyBankModel.Instance.Data.IsMapShowPigPanel = false;
            GameManager.UI.ShowUIForm("PiggyBankMenu",u =>
            {
                u.SetHideAction(() =>
                {
                    GameManager.Process.EndProcess(ProcessType.ShowPiggyBankMenuProcess);
                });
            }, () =>
            {
                GameManager.Process.EndProcess(ProcessType.ShowPiggyBankMenuProcess);
            });
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.ShowPiggyBankMenuProcess);
        }
    }
    
    
    private void AutoShowPiggyBankMenuByFirstEnterGame()
    {
        if (PiggyBankModel.Instance.IsShowAutoOpenPanelByMenuToMap)
        {
            //展示
            PiggyBankModel.Instance.Data.DateTimeUTC = DateTime.Now;
            GameManager.UI.ShowUIForm("PiggyBankMenu",u =>
            {
                u.SetHideAction(() =>
                {
                    GameManager.Process.EndProcess(ProcessType.ShowPiggyBankMenuProcess);
                });
            }, () =>
            {
                GameManager.Process.EndProcess(ProcessType.ShowPiggyBankMenuProcess);
            });
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.ShowPiggyBankMenuProcess);
        }
    }

    #endregion

    private void SendPersonRankRewards()
    {
        if (GameManager.Task.PersonRankManager.CheckIsOpen() &&
            GameManager.Task.PersonRankManager.TaskState == PersonRankState.Finished
            && !GameManager.Network.CheckInternetIsNotReachable())
        {
            GameManager.UI.ShowUIForm("PersonRankFinishedMenu");
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.PersonRankFinish);
        }
    }

    private void PersonRankChangeName()
    {
        if (GameManager.Task.PersonRankManager.CheckIsOpen() &&
            GameManager.Task.PersonRankManager.TaskState == PersonRankState.Playing &&
            !GameManager.PlayerData.RecordSetPlayerName &&
            Application.internetReachability != NetworkReachability.NotReachable)
        {
            GameManager.UI.ShowUIForm("PersonRankMenu");
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.PersonRank_NamingGuide);
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.ShowPersonRankChangeName);
        }
    }

    private void RegisterEntranceObjectsFlyProcess()
    {
        RegisterPiggyBankFlyReward();

        RegisterPersonRankFlyMedal();
        RegisterClimbBeanstalkFlyReward();
        RegisterKitchenFlyReward();
        RegisterGoldCollectionFlyReward();
        RegisterTilePassFlyReward();
        RegisterBalloonRiseFlyReward();
        RegisterHiddenTempleFlyReward();
        RegisterMergeFlyReward();
        RegisterPkItemFlyReward();
        RegisterHarvestKitchenFlyReward();
        if (EntranceFlyObjectManager.Instance != null && EntranceFlyObjectManager.Instance.Count > 0)
        {
            EntranceFlyObjectManager.Instance.Init();
            EntranceFlyObjectManager.Instance.ShowEntranceFlyObjectsAnim();
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.EntranceFlyObjects);
        }
    }

    private void RegisterLoginGuide()
    {
        if (GameManager.PlayerData.NowLevel < 22)
        {
            GameManager.Process.EndProcess(ProcessType.ShowLoginGuide);
            return;
        }

        if (GameManager.PlayerData.NowLevel > 22)
        {
            if (!GameManager.PlayerData.IsCanShowGuideByLevel())
            {
                GameManager.Process.EndProcess(ProcessType.ShowLoginGuide);
                return;
            }
        }

        if (GameManager.PlayerData.IsShowLoginGuide||
            GameManager.PlayerData.IsHaveShowSaveDataGuide)
        {
            GameManager.Process.EndProcess(ProcessType.ShowLoginGuide);
        }
        else
        {
            GameManager.Task.AddDelayTriggerTask(0.5f, () =>
            {
                GameManager.UI.ShowUIForm("CommonGuideMenu",form =>
                {
                    GameManager.PlayerData.IsShowLoginGuide = true;
                    //记录展示guide
                    form.gameObject.SetActive(false);
                    var guideMenu = form.GetComponent<CommonGuideMenu>();
                    var maptop = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
            
                    var originParent = maptop.SettingBtn.transform.parent;
                    maptop.SettingBtn.interactable = true;
                    maptop.SettingBtn.transform.SetParent(form.transform);
                    guideMenu.SetText("Setting.You can save your game data in Settings!");
                    guideMenu.tipBox.SetOkButton(false);
                    var position = maptop.SettingBtn.transform.position;
                    guideMenu.ShowGuideArrow(position-new Vector3(0,0.22f,0),position-new Vector3(0,0.30f,0),PromptBoxShowDirection.Up);

                    guideMenu.tipBox.transform.position = new Vector3(0, position.y - 0.65f, 0);
                    guideMenu.OnShow(null, null);
            
                    void ClickAction()
                    {
                        PlayerBehaviorModel.Instance.RecordHelpFirstGuide();
                        maptop.SettingBtn.transform.SetParent(originParent);
                        GameManager.UI.HideUIForm(form);
                        guideMenu.guideImage.onTargetAreaClick = null;
                        maptop.SettingBtn.onClick.RemoveListener(ClickAction);
                        GameManager.Process.EndProcess(ProcessType.ShowLoginGuide);
                    }
                    maptop.SettingBtn.onClick.AddListener(ClickAction);
                });
            });
        }
    }

    private void CheckDecorationAreaCompleteReward()
    {
        //意味着上一次启动 可能有被阻断的动画
        int tryToOverrideDecorationAreaID = DecorationModel.Instance.GetTryToOverrideDecorationAreaID();
        if (tryToOverrideDecorationAreaID == GameManager.PlayerData.DecorationAreaID + 1)
        {
            DecorationModel.Instance.SetDecorationAreaID(tryToOverrideDecorationAreaID);
            GameManager.Process.EndProcess(ProcessType.CheckDecorationAreaCompleteReward);
            return;
        }

        //补发装修完毕的奖励
        int recentAreaID = GameManager.PlayerData.DecorationAreaID;
        if (GameManager.PlayerData.CheckTargetAreaIsComplete(recentAreaID) &&
            !GameManager.PlayerData.GetTargetAreaGetReward(recentAreaID))
        {
            if (!GameManager.PlayerData.inAreaCompleteAnim)
            {
                GameManager.PlayerData.inAreaCompleteAnim = true;
                GameManager.UI.ShowUIForm("DecorationAreaCompleteMenu");
            }
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.CheckDecorationAreaCompleteReward);
        }
    }

    private void ShowPersonRankStartMenu()
    {
        if (GameManager.Network.CheckInternetIsNotReachable())
        {
            GameManager.Process.EndProcess(ProcessType.PersonRankStart);
            return;
        }

        GameManager.Task.PersonRankManager.CheckIsOpen();
        var flag2 = GameManager.Task.PersonRankManager.TaskState == PersonRankState.None;
        if (flag2 &&
            GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockPersonRankGameLevel &&
            Application.internetReachability != NetworkReachability.NotReachable &&
            !GameManager.Task.PersonRankManager.HasShownTodayWelcome)
        {
            GameManager.Task.PersonRankManager.HasShownTodayWelcome = true;
            GameManager.UI.ShowUIForm("PersonRankWelcomeMenu");
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.PersonRank_Start);
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.PersonRankStart);
        }
    }

    private void ShowCalendarChallengeMenu()
    {
        if (GameManager.Task.CalendarChallengeManager.NeedToShowAnim || GameManager.Task.CalendarChallengeManager.LastFailDate!=DateTime.MinValue)
        {
            GameManager.UI.ShowUIForm("CalendarChallengeMenu");
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.ShowCalendarChallengeMenu);
        }
    }
    
    private void CheckGlacierQuest()
    {
        try
        {
            if (!GameManager.Task.GlacierQuestTaskManager.CheckIsComplete())
            {
                GameManager.Process.EndProcess(ProcessType.GlacierQuest);
            }
        }
        catch (Exception e)
        {
            Log.Error($"GlacierQuest Exception {e.Message} {e.StackTrace}");
            GameManager.Process.EndProcess(ProcessType.GlacierQuest);
        }
    }

    private void RegisterShowRemovePopupAdsMenu()
    {
        if (GameManager.PlayerData.IsRemoveAds)
        {
            GameManager.Process.EndProcess(ProcessType.ShowRemoveAdsMenuWhenBackToMap);
            return;
        }

        if (GameManager.PlayerData.ShowRemoveAdsMenuWhenBackToMap)
        {
            GameManager.UI.ShowUIForm("RemoveAdsMenu",(f) =>
            {
                GameManager.PlayerData.TodayEverShowedNoAdsPanel = true;
                GameManager.PlayerData.LifeTimeEverShowedNoAdsPanel = true;
                GameManager.PlayerData.ShowRemoveAdsMenuWhenBackToMap = false;
                f.m_OnHideCompleteAction = () =>
                {
                    GameManager.Process.EndProcess(ProcessType.ShowRemoveAdsMenuWhenBackToMap);
                };
            },
                () =>
            {
                GameManager.Process.EndProcess(ProcessType.ShowRemoveAdsMenuWhenBackToMap);
            });
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.ShowRemoveAdsMenuWhenBackToMap);
        }
    }

    private void SyncDataToServer()
    {
        if (!GameManager.Ads.IsInitComplete || GameManager.Ads.IsRequestingAd) 
            return;

        //条件先随便写了个很粗野的 可以随便改
        if (GameManager.PlayerData.GetGameToMapCountByDay() % 3 == 1)
        {
            bool isSignIn = !string.IsNullOrEmpty(GameManager.PlayerData.UserID);
            if (isSignIn)
                FirebaseServiceUtil.SaveToServiceInOneDoc(null);
        }
    }

    #region 金块
    private void ShowGoldCollectionStartPanel()
    {
        if (!GameManager.Task.GoldCollectionTaskManager.ShowedFirstMenu
            && GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockGoldCollectionLevel)
        {
            MapTopPanelManager mapTop = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
            mapTop.goldCollectionEntrance.SetTimer();
            if (GameManager.Task.GoldCollectionTaskManager.CurrentIndex == 0)
            {
                GameManager.Process.EndProcess(ProcessType.ShowGoldCollectionStartPanel);
                return;
            }
            GameManager.UI.ShowUIForm("GoldCollectionStartPanel",showSuccessAction =>
            {
                if (GameManager.PlayerData.NowLevel != Constant.GameConfig.UnlockElementIceLevel && 
                    GameManager.PlayerData.NowLevel != Constant.GameConfig.UnlockElementGlueLevel && 
                    GameManager.PlayerData.NowLevel != Constant.GameConfig.UnlockElementFireworksLevel) 
                    GameManager.DataTable.GetDataTable<DTLevelID>().Data.SetAsSpecialGoldTile(GameManager.PlayerData.NowLevel);
                GameManager.Task.GoldCollectionTaskManager.ShowedFirstMenu = true;
                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Lucky_Collect_Activity_Open);
            }, () =>
            {
                GameManager.Process.EndProcess(ProcessType.ShowGoldCollectionStartPanel);
            });
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.ShowGoldCollectionStartPanel);
        }
    }

    private void ShowGoldCollectionEndPanel()
    {
        GoldCollectionTaskManager taskManager = GameManager.Task.GoldCollectionTaskManager;
        //如果周期内提前完成所有任务展示过结束界面，倒计时结束时直接重置数据
        // if (taskManager.ShowedLastMenu && taskManager.EndTime < DateTime.Now)
        // {
        //     taskManager.OnReset();
        // }
        //如果周期内提前完成所有任务或活动倒计时结束，先展示结束界面
        if (!taskManager.ShowedLastMenu && taskManager.CurrentIndex > 0 &&
            (taskManager.CheckAllTaskComplete() || taskManager.EndTime < DateTime.Now))
        {
            GameManager.UI.ShowUIForm("GoldCollectionEndPanel", showSuccessAction =>
            {
                GameManager.Task.GoldCollectionTaskManager.ShowedLastMenu = true;
                //如果倒计时结束，重置数据
                // if (taskManager.EndTime < DateTime.Now)
                //     taskManager.OnReset();
            }, () =>
            {
                GameManager.Process.EndProcess(ProcessType.ShowGoldCollectionEndPanel);
            });
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.ShowGoldCollectionEndPanel);
        }
    }
    
    private void ResetGoldCollectionData()
    {
        GoldCollectionTaskManager taskManager = GameManager.Task.GoldCollectionTaskManager;
        if (taskManager.EndTime < DateTime.Now && (taskManager.CurrentIndex <= 0 || taskManager.ShowedLastMenu))
        {
            taskManager.OnReset();
        }
        GameManager.Process.EndProcess(ProcessType.ResetGoldCollectionData);
    }
    
    private void RegisterGoldCollectionFlyReward()
    {
        if (GameManager.Task.GoldCollectionTaskManager.LevelCollectNum > 0)
        {
            MapTopPanelManager mapTop = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
            mapTop.goldCollectionEntrance.FlyReward();
        }
    }

    private void RegisterGoldCollectionSliderAnim()
    {
        if (GameManager.Task.GoldCollectionTaskManager.CurrentTask != null)
        {
            MapTopPanelManager mapTop = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;

            if (mapTop.goldCollectionEntrance.gameObject.activeInHierarchy) mapTop.goldCollectionEntrance.ShowBarPopupAnim();
            else
            {
                GameManager.Process.EndProcess(ProcessType.ShowGoldCollectionReward);
            }
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.ShowGoldCollectionReward);
        }
    }
    #endregion

    private void RegisterActivityPackAutoPop()
    {
        bool isNeedShow = false;

        if (!isNeedShow)
        {
            if (isLevelWinBack && GameManager.Scene.SceneChangeType == SceneChangeType.GameToMap && (GameManager.PlayerData.NowLevel - 1) % 50 == 0) 
            {
                isNeedShow = true;
                GameManager.UI.ShowUIForm("CeremonyPackMenu", (u) =>
                {
                    u.SetHideAction(() => { GameManager.Process.EndProcess(ProcessType.ShowActivityPackWhenBackToMap); });
                });
            } 
        }
        
        if (!isNeedShow)
        {
            isNeedShow = ShowPackProcess("AnimalPackMenu", Constant.GameConfig.UnlockPackLevel, AnimalPackMenu.GiftPackStartTime, AnimalPackMenu.GiftPackEndTime, 3);
        }
        
        if (!isNeedShow)
        {
            isNeedShow = ShowPackProcess("SweetMomentsPackThreeColumnPackMenu", Constant.GameConfig.UnlockPackLevel, SweetMomentsPackThreeColumnPackMenu.GiftPackStartTime, SweetMomentsPackThreeColumnPackMenu.GiftPackEndTime, 3);
        }
        
        if (!isNeedShow)
        {
            if (GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockWeekendPack && (DateTime.Now.DayOfWeek == DayOfWeek.Sunday || DateTime.Now.DayOfWeek >= DayOfWeek.Friday))
            {
                DateTime signDateTime = new DateTime(2024, 5, 31);
                if (PlayerPrefs.GetInt("WeekendPicnicPeriod", 0) != (DateTime.Now - signDateTime).Days / 7 + 1 ||
                    (DateTime.Now.DayOfWeek == DayOfWeek.Friday && DateTime.Now.Hour >= 15))
                {
                    isNeedShow = ShowPackProcess("WeekendPicnicPackMenu", Constant.GameConfig.UnlockWeekendPack, DateTime.Now, DateTime.Now.DayOfWeek == DayOfWeek.Sunday ? DateTime.Now.AddDays(1).Date : DateTime.Now.AddDays(7 - (int)DateTime.Now.DayOfWeek + 1).Date, 0, "WeekendTodayShowTime");
                }
            }
        }

        if (!isNeedShow) GameManager.Process.EndProcess(ProcessType.ShowActivityPackWhenBackToMap);
    }

    private bool ShowPackProcess(string packName, int openLevel, DateTime startTime, DateTime endTime, int gameToMapAutoPopCount, string todayShowTimeKey = null)
    {
        if (GameManager.PlayerData.NowLevel < openLevel || DateTime.Now < startTime || DateTime.Now >= endTime) 
        {
            return false;
        }

        bool isFirstUnlock = PlayerPrefs.GetInt("IsFristUnlock_" + packName, 0) == 0;
        bool levelIsEnough = GameManager.PlayerData.NowLevel >= openLevel;
        int gameToMapCount = GameManager.PlayerData.GetGameToMapCountByDay();
        int todayPopCount = todayShowTimeKey != null ? PlayerPrefs.GetInt(todayShowTimeKey, 0) : GameManager.PlayerData.GetTodayActivityPackShowTimes();

        bool isAutoOpen = (todayPopCount <= 0) && (levelIsEnough && (isFirstUnlock || gameToMapCount >= gameToMapAutoPopCount));
        if (isAutoOpen)
        {
            todayPopCount++;
            if (todayShowTimeKey != null)
                PlayerPrefs.SetInt(todayShowTimeKey, todayPopCount);
            else
                GameManager.PlayerData.RecordTodayActivityPackShowTimes(todayPopCount);
            GameManager.UI.ShowUIForm(packName, (u) =>
            {
                u.SetHideAction(() => { GameManager.Process.EndProcess(ProcessType.ShowActivityPackWhenBackToMap); });
            }, userData: endTime);

            if (isFirstUnlock)
                PlayerPrefs.SetInt("IsFristUnlock_" + packName, 1);

            return true;
        }
        else
        {
            return false;
        }
    }
    
    #region 通行证
    private void ShowTilePassStartMenu()
    {
        if (GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockTilePassLevel
            && !TilePassModel.Instance.ShowedStartMenu)
        {
            TilePassModel.Instance.EndTime = GameManager.DataTable.GetDataTable<DTTilePassScheduleData>().Data.GetNowActiveActivityEndTime();
            //活动开始，初始化数据表
            if (TilePassModel.Instance.EndTime != DateTime.MinValue)
            {
                GameManager.DataTable.GetDataTable<DTTilePassData>().Data.GetDataTable();
            }
            //无排期，结束进程
            else
            {
                GameManager.Process.EndProcess(ProcessType.ShowTilePassStartMenu);
                return;
            }

            GameManager.UI.ShowUIForm("TilePassStartMenu",showSuccessAction =>
            {
                TilePassModel.Instance.ShowedStartMenu = true;
                MapTopPanelManager mapTop = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
                mapTop.tilePassEntrance.OnInit(null);
            }, () =>
            {
                GameManager.Process.EndProcess(ProcessType.ShowTilePassStartMenu);
            });
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.ShowTilePassStartMenu);
        }
    }

    private void RegisterTilePassFlyReward()
    {
        if (TilePassModel.Instance.LevelCollectTargetNum > 0)
        {
            MapTopPanelManager mapTop = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
            mapTop.tilePassEntrance.FlyReward();
        }
    }

    private void ShowTilePassEndProcess()
    {
        //无排期，结束进程
        if (TilePassModel.Instance.EndTime == DateTime.MinValue)
        {
            GameManager.Process.EndProcess(ProcessType.ShowTilePassEndProcess);
            return;
        }
        //未购买通行证的玩家
        if (!TilePassModel.Instance.IsVIP)
        {
            // DTTilePassData TilePassData = GameManager.DataTable.GetDataTable<DTTilePassData>().Data;

            //时间结束
            if (DateTime.Now > TilePassModel.Instance.EndTime && TilePassModel.Instance.EndTime != DateTime.MinValue)
            {
                //弹出LastChance
                GameManager.UI.ShowUIForm("TilePassLastChanceMenu",showSuccessAction =>
                {

                }, () =>
                {
                    GameManager.Process.EndProcess(ProcessType.ShowTilePassEndProcess);
                });
            }
            //提前打关结束
            //else if (TilePassModel.Instance.TotalTargetNum >= TilePassData.GetTotalTargetNum(TilePassData.CurrentTilePassDatas.Count - 1))
            //{
            //    MapTopPanelManager mapTop = GameManager.UI.GetUIForm<MapTopPanelManager>().GetComponent<MapTopPanelManager>();
            //    //奖励未领完，不处理
            //    if (mapTop.tilePassEntrance.warningSign.activeSelf)
            //    {
            //        GameManager.Process.EndProcess(ProcessType.ShowTilePassEndProcess);
            //    }
            //    //奖励全领完，弹出LastChance
            //    else
            //    {
            //        GameManager.UI.ShowUIForm<TilePassLastChanceMenu>(showSuccessAction =>
            //        {

            //        }, () =>
            //        {
            //            GameManager.Process.EndProcess(ProcessType.ShowTilePassEndProcess);
            //        });
            //    }
            //}
            else
            {
                GameManager.Process.EndProcess(ProcessType.ShowTilePassEndProcess);
            }
        }
        //已购买通行证的玩家
        else
        {
            //时间结束
            if (DateTime.Now > TilePassModel.Instance.EndTime && TilePassModel.Instance.EndTime != DateTime.MinValue)
            {
                //弹出结束界面
                GameManager.UI.ShowUIForm("TilePassEndMenu",showSuccessAction =>
                {

                }, () =>
                {
                    GameManager.Process.EndProcess(ProcessType.ShowTilePassEndProcess);
                });
            }
            else
            {
                GameManager.Process.EndProcess(ProcessType.ShowTilePassEndProcess);
            }
        }
    }

    private void ResetTilePassData()
    {
        if (TilePassModel.Instance.ShowedEndMenu)
        {
            TilePassModel.Instance.ResetData();
        }
        GameManager.Process.EndProcess(ProcessType.ResetTilePassData);
    }
    #endregion

    #region Activity

    private void ActivityStartProcess()
    {
        GameManager.Activity.ActivityStartProcess();

        GameManager.Process.EndProcess(ProcessType.ActivityStartProcess);
    }

    private void ActivityEndProcess()
    {
        GameManager.Activity.ActivityEndProcess();

        GameManager.Process.EndProcess(ProcessType.ActivityEndProcess);
    }

    private void ActivityPreEndProcess()
    {
        GameManager.Activity.ActivityPreEndProcess();

        GameManager.Process.EndProcess(ProcessType.ActivityPreEndProcess);
    }

    private void ActivityAfterStartProcess()
    {
        GameManager.Activity.ActivityAfterStartProcess();

        GameManager.Process.EndProcess(ProcessType.ActivityAfterStartProcess);
    }

    #endregion

    #region HiddenTemple

    private void RegisterHiddenTempleFlyReward()
    {
        if (HiddenTemple.HiddenTempleManager.PlayerData.GetPickaxeLevelCollectNum() > 0)
        {
            MapTopPanelManager mapTop = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
            mapTop.hiddenTempleEntrance.ShowGetFlyReward();
        }
    }

    #endregion
    
    #region 气球竞速

    private void RegisterBalloonRiseStartProcess()
    {
        if (GameManager.Network.CheckInternetIsNotReachable())
        {
            GameManager.Process.EndProcess(ProcessType.ShowBalloonRiseStartMenu);
            return;
        }

        if (SystemInfoManager.CheckIsSpecialDeviceCloseLowPriorityPop())
        {
            GameManager.Process.EndProcess(ProcessType.ShowBalloonRiseStartMenu);
            return;
        }

        if (GameManager.Task.BalloonRiseManager.CheckIsOpen &&
            !GameManager.Task.BalloonRiseManager.PoppedStartMenu &&
            GameManager.Task.BalloonRiseManager.Stage == 0)
        {
            GameManager.Task.BalloonRiseManager.PoppedStartMenu = true;
            GameManager.UI.ShowUIForm("BalloonRiseStartMenu", UIFormType.PopupUI);
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.ShowBalloonRiseStartMenu);
        }
    }

    private void RegisterBalloonRiseRewardProcess()
    {
        if (GameManager.Network.CheckInternetIsNotReachable())
        {
            GameManager.Process.EndProcess(ProcessType.ShowBalloonRiseRewardProcess);
            return;
        }

        if (GameManager.Task.BalloonRiseManager.CheckIsFinishStage || 
            (GameManager.Task.BalloonRiseManager.StageEndTime != DateTime.MinValue && GameManager.Task.BalloonRiseManager.StageEndTime < DateTime.Now))
        {
            GameManager.UI.ShowUIForm("BalloonRiseMainMenu", UIFormType.CenterUI);
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.ShowBalloonRiseRewardProcess);
        }
    }
    
    private void RegisterBalloonRiseFlyReward()
    {
        if (GameManager.Task.BalloonRiseManager.ScoreChange)
        {
            MapTopPanelManager mapTop = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
            mapTop.balloonRiseEntrance.FlyReward();
        }
    }

    #endregion

    #region Merge

    private void RegisterMergeFlyReward()
    {
        if (Merge.MergeManager.PlayerData.GetMergeEnergyBoxLevelCollectNum() > 0)
        {
            MapTopPanelManager mapTop = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
            mapTop.mergeEntrance.ShowGetFlyReward();
        }
    }

    #endregion
    
    #region PiggyBank
    private void RegisterPiggyBankFlyReward()
    {
        if (PiggyBankModel.Instance.Data.AddCoinByCurLevel > 0)
        {
            MapTopPanelManager mapTop = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
            mapTop.piggyBankEntrance.ShowGetFlyReward();
        }
    }
    #endregion

    #region 无尽宝箱
    //注册补发行为
    private void RegisterEndlessChestRewardPanelByMenuToMap()
    {
        int lastActivityId = EndlessChestModel.Instance.Data.LastActivityId > 0
            ? EndlessChestModel.Instance.Data.LastActivityId
            : EndlessChestModel.Instance.Data.ActivityId;
        var lastData= GameManager.DataTable.GetDataTable<DTEndlessTreasureScheduleData>().Data.GetEndlessDataByActivityId(lastActivityId);
        //有活动并且活动结束了，补发
        if (lastData != null&&!EndlessChestModel.Instance.IsHaveOverById(lastData.ActivityID))
        {
            if (lastData.IsOver)
            {
                //补发
                if (EndlessChestModel.Instance.Data.RecordFreeRewardDict != null &&
                    EndlessChestModel.Instance.Data.RecordFreeRewardDict.Count > 0)
                {
                    var rewards = EndlessChestModel.Instance.GetRcordFreeRewards();
                    //展示补发界面
                    RewardManager.Instance.AddNeedGetReward(rewards.Keys.ToList(),rewards.Values.ToList());
                    RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.EndlessChestRewardPanel,false, () =>
                    {
                        GameManager.Process.EndProcess(ProcessType.EndlessChest);
                    },null, () =>
                    {
                        GameManager.UI.HideUIForm("GlobalMaskPanel");
                    });
                    //结算前面endless活动数据
                }else
                    GameManager.Process.EndProcess(ProcessType.EndlessChest);
                EndlessChestModel.Instance.Balance();
            }else
                GameManager.Process.EndProcess(ProcessType.EndlessChest);
        }
        else
            GameManager.Process.EndProcess(ProcessType.EndlessChest);
    }

    private void RegisterAutoEndlessChestPanel()
    {
        if (SystemInfoManager.CheckIsSpecialDeviceCloseLowPriorityPop())
        {
            GameManager.Process.EndProcess(ProcessType.EndlessChestAutoOpen);
            return;
        }

        bool isUnlock = EndlessChestModel.Instance.IsEndlessUnlock;//是否解锁
        if (isUnlock)
        {
            //是否某一个无尽宝箱处于这个周期内  并且没有自动弹出过
            var data=GameManager.DataTable.GetDataTable<DTEndlessTreasureScheduleData>().Data.GetEndlessDataByDateTimeNow();

            if (data != null&&!data.IsOver&&!EndlessChestModel.Instance.Data.IsAutoOpenPanel)
            {
                var isHaveOver = EndlessChestModel.Instance.IsHaveOverById(data.ActivityID);
                if (!isHaveOver)
                {
                    EndlessChestModel.Instance.RecordAutoOpen();
                    //自动弹出
                    GameManager.UI.ShowUIForm("EndlessChestPanel",UIFormType.CenterUI,u =>
                    {
                        u.SetHideAction(()=> GameManager.Process.EndProcess(ProcessType.EndlessChestAutoOpen));
                    },userData:data.EndDateTime);
                    return;
                }
            }
        }
        GameManager.Process.EndProcess(ProcessType.EndlessChestAutoOpen);
    }

    #endregion

    #region PkGame
    private void RegisterPkItemFlyReward()
    {
        if (PkGameModel.Instance.IsActivityOpen&&PkGameModel.Instance.PkItemFlyNum>0)
        {
            MapTopPanelManager mapTop = GameManager.UI.GetUIForm("MapTopPanelManager")as MapTopPanelManager;
            mapTop.PkGameEntrance.FlyReward();
        }
    }
    
    private void ShowPkGame2()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            GameManager.Process.EndProcess(ProcessType.ShowPkGame);
            return;
        }

        bool isCanOpen = !PkGameModel.Instance.IsActivityOpen &&
                         PkGameModel.Instance.IsCanOpenByLevel &&
                         PkGameModel.Instance.IsCanOpenByTime;
        bool isOver = PkGameModel.Instance.IsActivityOpen &&
                      PkGameModel.Instance.PkGameStatus == PkGameStatus.Over;
        bool isAutoOpen = !PkGameModel.Instance.Data.IsHaveAutoOpenPanel;
        
        if ((isCanOpen&&isAutoOpen)||isOver)
        {
            PkGameModel.Instance.Data.IsHaveAutoOpenPanel = true;
            PkGameModel.Instance.SaveToLocal();
            ShowPkGame();
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.ShowPkGame);
        }
    }

    private void ShowPkGame()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            GameManager.Process.EndProcess(ProcessType.ShowPkGame);
            return;
        }

        bool isCanOpen = (PkGameModel.Instance.IsCanOpenByTime&&PkGameModel.Instance.IsCanOpenByLevel)||PkGameModel.Instance.IsActivityOpen;
        if (isCanOpen)
        {
            //over
            if (PkGameModel.Instance.PkGameStatus == PkGameStatus.Over)
            {
                PkGameModel.Instance.RemoveListen();

                switch (PkGameModel.Instance.PkGameOverStatus)
                {
                    case PkGameOverStatus.Win:
                        GameManager.UI.ShowUIForm("PkGameWinPanel",UIFormType.PopupUI,u =>
                        {
                            u.SetHideAction(() => { GameManager.Process.EndProcess(ProcessType.ShowPkGame);});
                        });
                        break;
                    case PkGameOverStatus.Lose:
                        GameManager.UI.ShowUIForm("PkGameFailPanel",UIFormType.PopupUI,u =>
                        {
                            u.SetHideAction(() => { GameManager.Process.EndProcess(ProcessType.ShowPkGame);});
                        });
                        break;
                    case PkGameOverStatus.Deuce:
                        GameManager.UI.ShowUIForm("PkGameDeucePanel",UIFormType.PopupUI,u =>
                        {
                            u.SetHideAction(() => { GameManager.Process.EndProcess(ProcessType.ShowPkGame);});
                        });
                        break;
                }
            }
            else
            {
                if (!SystemInfoManager.CheckIsSpecialDeviceCloseLowPriorityPop())
                {
                    if (!PkGameModel.Instance.IsActivityOpen)
                    {
                        //自动弹出
                        GameManager.Firebase.RecordMessageByEvent("PKMatch_Show",
                            new Parameter("State", 1));
                    }

                    GameManager.UI.ShowUIForm("PkGameStartPanel",UIFormType.PopupUI, u =>
                    {
                        u.SetHideAction(() =>
                        {
                            GameManager.Process.EndProcess(ProcessType.ShowPkGame);
                        });
                    });
                }
                else
                {
                    GameManager.Process.EndProcess(ProcessType.ShowPkGame);
                }
            }
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.ShowPkGame);
        }
    }
    #endregion

    #region 卡牌

    private void BackPayCardPack()
    {
        if (!CardModel.Instance.IsInCardActivity)
        {
            GameManager.Process.EndProcess(ProcessType.BackPayCardPack);
            return;
        }

        int cardPack1Num = GameManager.PlayerData.GetItemNum(TotalItemData.CardPack1);
        if (cardPack1Num > 0)
        {
            GameManager.PlayerData.UseItem(TotalItemData.CardPack1, cardPack1Num);
            if (CardModel.Instance.IsInCardActivity)
                RewardManager.Instance.AddNeedGetReward(TotalItemData.CardPack1, cardPack1Num);
        }

        int cardPack2Num = GameManager.PlayerData.GetItemNum(TotalItemData.CardPack2);
        if (cardPack2Num > 0)
        {
            GameManager.PlayerData.UseItem(TotalItemData.CardPack2, cardPack2Num);
            if (CardModel.Instance.IsInCardActivity)
                RewardManager.Instance.AddNeedGetReward(TotalItemData.CardPack2, cardPack2Num);
        }

        int cardPack3Num = GameManager.PlayerData.GetItemNum(TotalItemData.CardPack3);
        if (cardPack3Num > 0)
        {
            GameManager.PlayerData.UseItem(TotalItemData.CardPack3, cardPack3Num);
            if (CardModel.Instance.IsInCardActivity)
                RewardManager.Instance.AddNeedGetReward(TotalItemData.CardPack3, cardPack3Num);
        }

        int cardPack4Num = GameManager.PlayerData.GetItemNum(TotalItemData.CardPack4);
        if (cardPack4Num > 0)
        {
            GameManager.PlayerData.UseItem(TotalItemData.CardPack4, cardPack4Num);
            if (CardModel.Instance.IsInCardActivity)
                RewardManager.Instance.AddNeedGetReward(TotalItemData.CardPack4, cardPack4Num);
        }

        int cardPack5Num = GameManager.PlayerData.GetItemNum(TotalItemData.CardPack5);
        if (cardPack5Num > 0)
        {
            GameManager.PlayerData.UseItem(TotalItemData.CardPack5, cardPack5Num);
            if (CardModel.Instance.IsInCardActivity)
                RewardManager.Instance.AddNeedGetReward(TotalItemData.CardPack5, cardPack5Num);
        }

        if (RewardManager.Instance.NeedGetRewardCount > 0)
        {
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false,
                () =>
                {
                    GameManager.UI.ShowUIForm("GlobalMaskPanel");
                    GameManager.Process.EndProcess(ProcessType.BackPayCardPack);
                },
                onCreatePanelSuccess: () => GameManager.UI.HideUIForm("GlobalMaskPanel"));
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.BackPayCardPack);
        }
    }

    private void ShowCardSetPreviewPanel()
    {
        if (GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockCardSetLevel ||
            !CardModel.Instance.IsHaveCardAsset)
        {
            GameManager.Process.EndProcess(ProcessType.ShowCardSetPreviewPanel);
            return;
        }
        if (!CardModel.Instance.ShowedPreviewPanel &&
            DateTime.Now < CardModel.Instance.CardStartTime &&
            CardModel.Instance.CardStartTime.Subtract(DateTime.Now).TotalHours <= 24)
        {
            GameManager.UI.ShowUIForm($"CardSetStartPanel{CardModel.Instance.CardActivityID}", form =>
            {
                CardModel.Instance.ShowedPreviewPanel = true;
                
                MapTopPanelManager mapTopPanel = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
                mapTopPanel?.cardSetEntrance.OnInit(null);
                
                form.SetHideAction(() => GameManager.Process.EndProcess(ProcessType.ShowCardSetPreviewPanel));
            }, 
                () => GameManager.Process.EndProcess(ProcessType.ShowCardSetPreviewPanel));
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.ShowCardSetPreviewPanel);
        }
    }
    
    private void ShowCardSetStartPanel()
    {
        if (GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockCardSetLevel||
            !CardModel.Instance.IsHaveCardAsset)
        {
            GameManager.Process.EndProcess(ProcessType.ShowCardSetStartPanel);
            return;
        }
        if (!CardModel.Instance.ShowedStartPanel &&
            DateTime.Now >= CardModel.Instance.CardStartTime &&
            DateTime.Now < CardModel.Instance.CardEndTime)
        {
            GameManager.UI.ShowUIForm($"CardSetStartPanel{CardModel.Instance.CardActivityID}", form =>
            {
                CardModel.Instance.ShowedPreviewPanel = true;
                // CardModel.Instance.ShowedStartPanel = true;
                
                MapTopPanelManager mapTopPanel = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
                mapTopPanel?.cardSetEntrance.OnInit(null);
                
                // form.SetHideAction(() => GameManager.Process.EndProcess(ProcessType.ShowCardSetStartPanel));
            }, 
                () => GameManager.Process.EndProcess(ProcessType.ShowCardSetStartPanel));
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.ShowCardSetStartPanel);
        }
    }
    
    private void ShowCardSetCountDownPanel()
    {
        if (!CardModel.Instance.IsHaveCardAsset)
        {
            GameManager.Process.EndProcess(ProcessType.ShowCardSetCountDownPanel);
            return;
        }
        if (CardModel.Instance.ShowedStartPanel &&
            !CardModel.Instance.ShowedCountdownPanel &&
            DateTime.Now < CardModel.Instance.CardEndTime &&
            CardModel.Instance.CardEndTime.Subtract(DateTime.Now).TotalHours <= 48)
        {
            GameManager.UI.ShowUIForm($"CardSetEndPanel{CardModel.Instance.CardActivityID}", form =>
            {
                CardModel.Instance.ShowedCountdownPanel = true;
                form.SetHideAction(() => GameManager.Process.EndProcess(ProcessType.ShowCardSetCountDownPanel));
            }, 
                () => GameManager.Process.EndProcess(ProcessType.ShowCardSetCountDownPanel));
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.ShowCardSetCountDownPanel);
        }
    }

    private void ShowCardSetEndPanel()
    {
        if (CardModel.Instance.CardEndTime != DateTime.MinValue &&
            DateTime.Now >= CardModel.Instance.CardEndTime)
        {
            if (CardModel.Instance.ShowedStartPanel && !CardModel.Instance.ShowedEndPanel)
            {
                GameManager.UI.ShowUIForm($"CardSetEndPanel{CardModel.Instance.CardActivityID}", form =>
                {
                    // form.SetHideAction(() =>
                    // {
                    //     CardModel.Instance.ShowedEndPanel = true;
                    //     CardModel.Instance.ResetData();
                    //     GameManager.Process.EndProcess(ProcessType.ShowCardSetEndPanel);
                    // });
                }, () =>
                {
                    CardModel.Instance.ResetData();
                    GameManager.Process.EndProcess(ProcessType.ShowCardSetEndPanel);
                });
            }
            else
            {
                CardModel.Instance.ResetData();
                GameManager.Process.EndProcess(ProcessType.ShowCardSetEndPanel);
            }
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.ShowCardSetEndPanel);
        }
    }

    #endregion
    
    private void GoToGame(object sender, GameEventArgs e)
    {
        CommonEventArgs ne = (CommonEventArgs)e;

        if (ne.Type == CommonEventType.MapToGame)
        {
            isToGame = true;
        }
    }
}