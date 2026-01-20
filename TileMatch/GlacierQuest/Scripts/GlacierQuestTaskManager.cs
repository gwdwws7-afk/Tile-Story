using GameFramework.Event;
using MySelf.Model;
using System;
using UnityEngine;

public class GlacierQuestTaskManager : TaskManagerBase
{
    // public override bool IsOpen => true;

    public bool IsUnlock => GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockGlacierQuestLevel;

    public bool isInGame => GameManager.Scene.SceneType == SceneType.Game;

    public GlacierQuestState ActivityState
    {
        get => GlacierQuestModel.Instance.ActivityState;
        set => GlacierQuestModel.Instance.ActivityState = value;
    }

    public bool IsFirstOpen
    {
        get => GlacierQuestModel.Instance.IsFirstOpen;
        set => GlacierQuestModel.Instance.IsFirstOpen = value;
    }

    public bool IsFirstStart
    {
        get => GlacierQuestModel.Instance.IsFirstStart;
        set => GlacierQuestModel.Instance.IsFirstStart = value;
    }

    public bool IsFirstPop
    {
        get => GlacierQuestModel.Instance.IsFirstPop;
        set => GlacierQuestModel.Instance.IsFirstPop = value;
    }

    public bool IsShowGuide
    {
        get => GlacierQuestModel.Instance.IsShowGuide;
        set => GlacierQuestModel.Instance.IsShowGuide = value;
    }

    public bool IsCanClaimedReward
    {
        get => GlacierQuestModel.Instance.IsCanClaimedReward;
        set => GlacierQuestModel.Instance.IsCanClaimedReward = value;
    }

    public bool IsShowEntrance
    {
        get => GlacierQuestModel.Instance.IsShowEntrance;
        set => GlacierQuestModel.Instance.IsShowEntrance = value;
    }

    public bool IsRechallenge
    {
        get => GlacierQuestModel.Instance.IsRechallenge;
        set => GlacierQuestModel.Instance.IsRechallenge = value;
    }

    public bool NeedPopStart
    {
        get => GlacierQuestModel.Instance.NeedPopStart;
        set => GlacierQuestModel.Instance.NeedPopStart = value;
    }

    public int CurrentActivityID
    {
        get => GlacierQuestModel.Instance.CurrentActivityID;
        set => GlacierQuestModel.Instance.CurrentActivityID = value;
    }

    public int TodayOpenCount
    {
        get => GlacierQuestModel.Instance.TodayOpenCount;
        set => GlacierQuestModel.Instance.TodayOpenCount = value;
    }

    public int CurLevel
    {
        get => GlacierQuestModel.Instance.CurLevel;
        set => GlacierQuestModel.Instance.CurLevel = value;
    }

    public int OldLevel
    {
        get => GlacierQuestModel.Instance.OldLevel;
        set => GlacierQuestModel.Instance.OldLevel = value;
    }

    public int CurPeople
    {
        get => GlacierQuestModel.Instance.CurPeople;
        set => GlacierQuestModel.Instance.CurPeople = value;
    }

    public int OldPeople
    {
        get => GlacierQuestModel.Instance.OldPeople;
        set => GlacierQuestModel.Instance.OldPeople = value;
    }

    public int SuccessPeopleNum
    {
        get => GlacierQuestModel.Instance.SuccessPeopleNum;
        set => GlacierQuestModel.Instance.SuccessPeopleNum = value;
    }

    //活动次数重置时间
    public DateTime RestartTime
    {
        get => GlacierQuestModel.Instance.RestartTime;
        set => GlacierQuestModel.Instance.RestartTime = value;
    }

    //当期结束时间
    public DateTime EndTime
    {
        get => GlacierQuestModel.Instance.EndTime;
        set => GlacierQuestModel.Instance.EndTime = value;
    }

    // 整体活动的结束时间
    public DateTime ActivityEndTime
    {
        get => GlacierQuestModel.Instance.ActivityEndTime;
        set => GlacierQuestModel.Instance.ActivityEndTime = value;
    }

    public int[] HeadIds
    {
        get => GlacierQuestModel.Instance.HeadIdArray;
    }

    public int[] SuccessHeadIdArray
    {
        get => GlacierQuestModel.Instance.SuccessHeadIdArray;
        set => GlacierQuestModel.Instance.SuccessHeadIdArray = value;
    }

    public int[] CurveArray
    {
        get => GlacierQuestModel.Instance.CurveArray;
    }

    private const int LAVA_DUNGEON_CONTINUOUS_WIN_STAGE_NUM_NEEDED = 7;

    private const int LAVA_FAIL_RESTART_TIME_IN_MINUTES = 5;

    public override void OnInit()
    {
        // if (!IsOpen) return;
        // 当前当期活动未被记录开放过，并且活动状态不属于开放中，且当前时间大于当期的结束时间
        if (CheckActivityIsOpen() && ActivityState != GlacierQuestState.Open && ActivityEndTime != DateTime.MinValue && DateTime.Now > ActivityEndTime)
        {
            Log.Debug("GlacierQuest: 活动不在开放时间");
            return;
        }

        Log.Debug("GlacierQuest: 开始初始化活动");
        DTGlacierQuestScheduleData dataTable = GameManager.DataTable.GetDataTable<DTGlacierQuestScheduleData>().Data;
        int recentActivityID = dataTable.GetNowActiveActivityID();
        if (recentActivityID > 0)
        {
            if (!EverFinishedTargetActivityID(recentActivityID))
            {
                if (IsFirstStart)
                {
                    //初始化数据
                    IsFirstStart = false;
                    CurrentActivityID = recentActivityID;
                    ActivityState = GlacierQuestState.Wait;
                    GlacierQuestModel.Instance.RestartActivity();
                }

                ActivityEndTime = dataTable.GetNowActiveActivityEndTime();
                GameManager.Event.Subscribe(CommonEventArgs.EventId, CommonEvent);
            }
        }

        if (CurrentActivityID > 0)
        {
            UpdateActivityState();
            if (ActivityState == GlacierQuestState.Open)
            {
                ActivityEndTime = dataTable.GetActiveEndTimeByActivityID(CurrentActivityID);
                GameManager.Event.Subscribe(CommonEventArgs.EventId, CommonEvent);
            }
        }
    }

    public override void OnReset()
    {
        GlacierQuestModel.Instance.RestartActivity();
    }

    public override void ConfirmTargetCollection(TaskTargetCollection collection)
    {
        if (IsUnlock && ActivityState == GlacierQuestState.Open && !GameManager.Network.CheckInternetIsNotReachable()) 
        {
            if (CurLevel < LAVA_DUNGEON_CONTINUOUS_WIN_STAGE_NUM_NEEDED)
            {
                int collectNum = collection.GetTargetCollectNum(GetTaskTarget());
                OnGameWin(collectNum);
            }
        }
    }

    public override TaskTarget GetTaskTarget()
    {
        return TaskTarget.LevelPass;
    }

    public void CommonEvent(object sender, GameEventArgs e)
    {
        //var ne = e as CommonEventArgs;
        //if (ne == null) return;
        //switch(ne.Type)
        //{
        //    case CommonEventType.MapToGame:
        //        isInGame = true;
        //        break;
        //    case CommonEventType.GameToMap:
        //        isInGame = false;
        //        break;
        //}
    }

    public bool CheckIsComplete()
    {
        if (!AddressableUtils.IsHaveAsset("GlacierQuestMenu")) return false;

        if (GameManager.Network.CheckInternetIsNotReachable()) return false;

        if (GameManager.Task.GlacierQuestTaskManager.IsCanClaimedReward)
        {
            Log.Debug("GlacierQuest: 奖励补发");
            // 奖励未领取，弹出奖励领取界面
            GameManager.UI.ShowUIForm("GlacierQuestRewardSet",form =>
            {
                (form as GlacierQuestRewardSet).SetClaimEvent(() =>
                {
                    GameManager.UI.HideUIForm("GlacierQuestRewardSet");
                    RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.None, true, () =>
                    {
                        if (CheckActivityIsOpen() && ActivityEndTime != DateTime.MinValue && ActivityEndTime <= DateTime.Now)
                        {
                            GlacierQuestModel.Instance.RecordFinishActivityId(CurrentActivityID);
                            IsFirstStart = true;
                            ActivityState = GlacierQuestState.Close;
                            GlacierQuestModel.Instance.RestartActivity();
                            OnInit();
                            GameManager.Process.EndProcess(ProcessType.GlacierQuest);
                        }
                        else if (CheckActivityIsOpen() && ActivityEndTime > DateTime.Now && ActivityState == GlacierQuestState.Wait)
                        {
                            // 活动正在等待参与，弹出提示框
                            GameManager.UI.ShowUIForm("GlacierQuestStartMenu");
                        }
                        else
                        {
                            GameManager.Process.EndProcess(ProcessType.GlacierQuest);
                        }
                    });
                });
            });
            return true;
        }
        // 判断当期活动是否已结束，并且不属于活动不属于开放状态(开放状态指玩家已经进入了一轮挑战)
        if (CheckActivityIsOpen() && ActivityEndTime != DateTime.MinValue && ActivityEndTime <= DateTime.Now && ActivityState != GlacierQuestState.Open)
        {
            GlacierQuestModel.Instance.RecordFinishActivityId(CurrentActivityID);
            IsFirstStart = true;
            ActivityState = GlacierQuestState.Close;
            GlacierQuestModel.Instance.RestartActivity();
            OnInit();
            return false;
        }
        // 需要增加是否是开放时间的判断条件
        if (CheckActivityIsOpen() && CheckIsOpenTime() && ActivityEndTime > DateTime.Now && ActivityState == GlacierQuestState.Wait &&
            GameManager.Task.GlacierQuestTaskManager.NeedPopStart && !GameManager.Network.CheckInternetIsNotReachable()) 
        {
            // 活动正在等待参与，弹出提示框
            GameManager.UI.ShowUIForm("GlacierQuestStartMenu");
            return true;
        }
        return false;
    }

    public bool CheckIsOpen()
    {
        return ActivityState != GlacierQuestState.Clear && ActivityState != GlacierQuestState.ClearTime && ActivityState != GlacierQuestState.Close;
    }

    public bool CheckIsOpenTime()
    {
        if ((DateTime.Now.DayOfWeek > DayOfWeek.Wednesday && DateTime.Now.DayOfWeek < DayOfWeek.Friday) ||
            (DateTime.Now.DayOfWeek == DayOfWeek.Wednesday && DateTime.Now.Hour >= 15) ||
            (DateTime.Now.DayOfWeek == DayOfWeek.Friday && DateTime.Now.Hour < 15))
            return true;
        return false;
    }

    public bool ChcekIsOpenTime_UnincludedHour()
    {
        return DateTime.Now.DayOfWeek >= DayOfWeek.Wednesday && DateTime.Now.DayOfWeek <= DayOfWeek.Friday;
    }

    public bool CheckIsInActivityOpenDay()
    {
        if (CurrentActivityID <= 0) return false;
        if (GameManager.DataTable.GetDataTable<DTGlacierQuestScheduleData>().Data.GetNowActiveActivityID() ==
            CurrentActivityID)
        {
            // 活动的开放时间为周三的9:00 到周五的15:00
            if (CheckIsOpenTime())
                return true;
            Log.Debug("GlacierQuest: 非周三到周五的活动开放时间");
        }
        else
            Log.Debug($"GlacierQuest: 活动期数不正确  Now--{GameManager.DataTable.GetDataTable<DTGlacierQuestScheduleData>().Data.GetNowActiveActivityID()}  Cur--{CurrentActivityID}");
        return false;
    }

    public bool CheckIsInActivityOpenDay_UnincludedHour()
    {
        if (CurrentActivityID <= 0) return false;
        if (GameManager.DataTable.GetDataTable<DTGlacierQuestScheduleData>().Data.GetNowActiveActivityID() ==
            CurrentActivityID)
        {
            // 活动的开放时间为周三到周五
            if (ChcekIsOpenTime_UnincludedHour())
                return true;
            Log.Debug("GlacierQuest: 非周一到周四的活动开放时间");
        }
        else
            Log.Debug($"GlacierQuest: 活动期数不正确  Now--{GameManager.DataTable.GetDataTable<DTGlacierQuestScheduleData>().Data.GetNowActiveActivityID()}  Cur--{CurrentActivityID}");
        return false;
    }

    /// <summary>
    /// 判断是否是活动开放的最后一天
    /// </summary>
    /// <returns>是否是最后一天的结果</returns>
    public bool CheckIsInLastActivityOpenDay()
    {
        if (DateTime.Now.DayOfWeek == DayOfWeek.Friday || (ActivityEndTime.Year == DateTime.Now.Year &&
                                                           ActivityEndTime.Month == DateTime.Now.Month &&
                                                           ActivityEndTime.Day == DateTime.Now.Day))
        {
            return true;
        }
        return false;
    }

    public void ClearOldData()
    {
        Log.Info("GlacierQuest：清除旧数据");
        OldPeople = 0;
        OldLevel = 0;
    }

    public int GetRewardCoinNum()
    {
        if (SuccessPeopleNum <= 0)
        {
            Log.Error("GlacierQuest : 胜利人数记录错误");
            return 0;
        }
        switch (SuccessPeopleNum)
        {
            case 6:
                return 1670;
            case 8:
                return 1250;
            case 9:
                return 1112;
            case 10:
                return 1000;
            case 11:
                return 910;
            default:
                Log.Error("GlacierQuest: 当前人数错误");
                return 1000;
        }
    }

    public void ReducePeople()
    {
        int cur = CurPeople;
        OldPeople = cur;
        int[] reducePeopleCurve = CurveArray;
        int rangeMinIndex = (CurLevel - 1) * 2 + 1;
        int rangeMaxIndex = (CurLevel - 1) * 2;
        int rangeMin = reducePeopleCurve[rangeMinIndex];
        int rangeMax = reducePeopleCurve[rangeMaxIndex];
        cur = UnityEngine.Random.Range(rangeMin, rangeMax);
        CurPeople = cur;
    }

    public void OnGameWin(int winNum)
    {
        Log.Info($"GlacierQuest：活动胜利{ActivityState}");
        //通关数 +1
        int progress = CurLevel + winNum;
        OldLevel = progress - winNum;
        CurLevel = progress;
        ReducePeople();
        //已完成挑战
        if (progress >= 7)
        {
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.LavaQuest_Challenge_Success);
            TodayOpenCount++;
            IsCanClaimedReward = true;
            SuccessPeopleNum = CurPeople;
            SuccessHeadIdArray = HeadIds;
            // 记录活动的重开时间
            if (TodayOpenCount < Constant.GameConfig.GlacierQuestOpenCountMax)
            {
                ActivityState = GlacierQuestState.ClearTime;
                DateTime oneDayAfterNowTodayZero = DateTime.Now.AddDays(1).Date;
                if (DateTime.Now < oneDayAfterNowTodayZero)
                {
                    RestartTime = DateTime.Now;
                }
                else
                {
                    if (!CheckIsInLastActivityOpenDay())
                        RestartTime = oneDayAfterNowTodayZero;
                    else
                        ActivityState = GlacierQuestState.Clear;
                }
            }
            else
            {
                ActivityState = GlacierQuestState.Clear;
            }
        }
    }

    /// <summary>
    /// 当游戏失败时
    /// </summary>
    public void OnGameFail()
    {
        Log.Info($"GlacierQuest：活动失败{ActivityState}");
        if (ActivityState == GlacierQuestState.Open)
        {
            //活动失败，进入冷却
            if (CheckIsInActivityOpenDay())
                ActivityState = GlacierQuestState.Time;
            else
                ActivityState = GlacierQuestState.Close;
            int progress = CurLevel;
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.LavaQuest_Challenge_Fail,
                new Firebase.Analytics.Parameter("progress", progress));
            OldLevel = progress;
            CurLevel = progress + 1;
            ReducePeople();
            //记录重开时间
            DateTime oneDayAfterNowTodayZero = DateTime.Now.AddDays(1).Date;
            if (DateTime.Now.AddMinutes(LAVA_FAIL_RESTART_TIME_IN_MINUTES) < oneDayAfterNowTodayZero)
            {
                RestartTime = DateTime.Now.AddMinutes(LAVA_FAIL_RESTART_TIME_IN_MINUTES);
            }
            else
            {
                if (!CheckIsInLastActivityOpenDay())
                    RestartTime = oneDayAfterNowTodayZero;
                else
                    ActivityState = GlacierQuestState.Close;
            }
            Log.Error($"GlacierQuest 活动状态：{ActivityState}   活动冷却：{RestartTime}");
        }
    }

    public void UpdateActivityState()
    {
        if (ActivityState == GlacierQuestState.Close)
            return;
        //检验活动是否开启中
        if (ActivityState != GlacierQuestState.Open && ActivityState != GlacierQuestState.Wait)
        {
            //判断冷却时间是否结束
            if ((ActivityState == GlacierQuestState.Time || ActivityState == GlacierQuestState.ClearTime) && DateTime.Now >= RestartTime)
            {
                ActivityState = GlacierQuestState.Wait;
                IsRechallenge = true;
            }
        }
        else
        {
            //进入下一期活动
            if (DateTime.Now >= EndTime)
            {
                if (ActivityState != GlacierQuestState.Wait)
                    NeedPopStart = true;
                ActivityState = GlacierQuestState.Wait;
                IsRechallenge = false;
            }
        }
    }

    //有需求 玩家如果昨天开启 今天打完。今天可以再开一轮
    public void CheckIfNeedSetStateToWaitAgain()
    {
        //EndTime应该一定是开启时 把当前时间加一天加出来的
        //如果这个时间 小于今晚过天 那么应该认为这个活动是昨天开的 现在有这样的需求：在这种情况下，今天还可以再开始一次
        if (ActivityState == GlacierQuestState.Clear)
        {
            if (EndTime < DateTime.Now.AddDays(1) - DateTime.Now.TimeOfDay)
                ActivityState = GlacierQuestState.Wait;
        }
    }

    public bool EverFinishedTargetActivityID(int targetActivityID)
    {
        return GlacierQuestModel.Instance.FinishedActivityIDList.Contains(targetActivityID);
    }

    public bool CheckActivityIsOpen()
    {
        if (EverFinishedTargetActivityID(CurrentActivityID))
        {
            Log.Info($"GlacierQuest: {CurrentActivityID} is Finished");
            return false;
        }
        return CurrentActivityID > 0 && IsUnlock;
    }

    public void NewDayRefresh()
    {
        Log.Info("GlacierQuest: 新的一天刷新数据");
        if (PlayerBehaviorModel.Instance.Data.GlacierQuestDayData.isNewDayRefresh)
        {
            PlayerBehaviorModel.Instance.Data.GlacierQuestDayData.isNewDayRefresh = false;
            TodayOpenCount = 0;
            if (CheckIsInActivityOpenDay_UnincludedHour())
            {
                // 活动处于开放时间
                //活动不处于开发状态，刷新周期
                if (ActivityState != GlacierQuestState.Open && ActivityState != GlacierQuestState.Wait)
                {
                    ActivityState = GlacierQuestState.Wait;
                    IsRechallenge = false;
                    //刷新本次挑战弹出机会
                    NeedPopStart = true;
                }
                else if (ActivityState == GlacierQuestState.Wait)
                {
                    NeedPopStart = true;
                }
            }
            else
            {
                // 活动不处于开放时间，且没有奖励可以领取，关闭活动并刷新活动数据
                if (ActivityState == GlacierQuestState.Wait || ActivityState == GlacierQuestState.Time ||
                    ActivityState == GlacierQuestState.ClearTime ||
                    (ActivityState != GlacierQuestState.Clear && ActivityState != GlacierQuestState.ClearTime && EndTime <= DateTime.Now))
                {
                    ActivityState = GlacierQuestState.Close;
                    GlacierQuestModel.Instance.RestartActivity();
                }
            }
        }
    }

    public void ClearRewardData()
    {
        SuccessPeopleNum = 0;
        IsCanClaimedReward = false;
    }
}
