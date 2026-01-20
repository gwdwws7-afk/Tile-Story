using MySelf.Model;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 爬藤活动管理器
/// </summary>
public sealed class ClimbBeanstalkManager : ActivityManagerBase
{
    public static ClimbBeanstalkManager Instance { get; private set; }

    public override int ActivityID => 20240919;

    //1：复活节元素
    public int ActivityTypeIndex = 1;

    private DTClimbBeanstalkTaskData dataTable;

    public DTClimbBeanstalkTaskData DataTable
    {
        get
        {
            if (dataTable == null)
            {
                dataTable = GameManager.DataTable.GetDataTable<DTClimbBeanstalkTaskData>().Data;
                dataTable.FilterTaskDatas(CurrentActivityID, CurrentPhaseID);
            }

            return dataTable;
        }
    }

    public int CurrentActivityID
    {
        get => ClimbBeanstalkModel.Instance.CurrentActivityID;
        set => ClimbBeanstalkModel.Instance.CurrentActivityID = value;
    }

    public int CurrentPhaseID
    {
        get => ClimbBeanstalkModel.Instance.CurrentPhaseID;
        set => ClimbBeanstalkModel.Instance.CurrentPhaseID = value;
    }

    public int CurrentWinStreak
    {
        get => ClimbBeanstalkModel.Instance.CurrentWinStreak;
        set => ClimbBeanstalkModel.Instance.CurrentWinStreak = value;
    }

    public int HighestClaimRewardNum
    {
        get => ClimbBeanstalkModel.Instance.HighestClaimRewardNum;
        set => ClimbBeanstalkModel.Instance.HighestClaimRewardNum = value;
    }

    public int LastWinStreakNum
    {
        get => ClimbBeanstalkModel.Instance.LastWinStreakNum;
        set => ClimbBeanstalkModel.Instance.LastWinStreakNum = value;
    }

    public bool NeedFlyReward
    {
        get => ClimbBeanstalkModel.Instance.NeedFlyReward;
        set => ClimbBeanstalkModel.Instance.NeedFlyReward = value;
    }

    public bool EverShowedFirstTimeIntro
    {
        get => ClimbBeanstalkModel.Instance.EverShowedFirstTimeIntro;
        set => ClimbBeanstalkModel.Instance.EverShowedFirstTimeIntro = value;
    }

    public bool EverShowedIntro
    {
        get => ClimbBeanstalkModel.Instance.EverShowedIntro;
        set => ClimbBeanstalkModel.Instance.EverShowedIntro = value;
    }

    private void Awake()
    {
        Instance = this;
    }

    public bool CheckEntranceCanShow()
    {
        if (!CheckInitializationComplete())
            return false;

        if (GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockClimbBeanstalkEventLevel)
            return false;

        if (CheckActivityHasStarted())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public override bool CheckActivityCanStart()
    {
        if (!CheckInitializationComplete())
            return false;

        if (GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockClimbBeanstalkEventLevel)
            return false;

        if (CheckActivityHasStarted())
            return false;

        return GameManager.Activity.CheckActivityCanStart(ActivityID);
    }

    public override bool CheckActivityHasStarted()
    {
        return CurrentActivityID > 0 && GameManager.Activity.GetCurActivityID() == ActivityID;
    }

    /// <summary>
    /// 确认关卡胜利是否可以获取目标
    /// </summary>
    public bool CheckLevelWinCanGetTarget()
    {
        if (!CheckInitializationComplete())
            return false;

        return CheckActivityHasStarted();
    }

    public override void ActivityStartProcess()
    {
        if (CheckActivityCanStart())
        {
            int periodID = GameManager.Activity.StartActivity(ActivityID);
            if (periodID != 0)
            {
                ClimbBeanstalkModel.Instance.Init(ActivityID, 1);

                GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.ClimbBeanstalkInfoChanged));

                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.WinStreak_Open, new Firebase.Analytics.Parameter("ActivityID", CurrentActivityID));
            }
        }
    }

    public override void ActivityEndProcess()
    {
        if (CheckActivityHasStarted())
        {
            //活动时间结束或者所有阶段全部完成
            if (DateTime.Now > GameManager.Activity.GetCurActivityEndTime() || (ThisPhaseIsFinished() && !HasNextPhaseToEnter()))
            {
                ClimbBeanstalkTaskDatas firstUnclaimedTask = null;
                if (HasRewardToClaim(out firstUnclaimedTask))
                {
                    EndActivity();

                    List<TotalItemData> rewardIDs = firstUnclaimedTask.RewardTypeList;
                    List<int> rewardNums = firstUnclaimedTask.RewardNumList;
                    for (int i = 0; i < rewardIDs.Count; ++i)
                    {
                        RewardManager.Instance.AddNeedGetReward(rewardIDs[i], rewardNums[i]);
                    }

                    void ActivityEnd()
                    {
                        RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, () =>
                        {
                            GameManager.Process.EndProcess(ProcessType.ShowClimbBeanstalkEndProcess);
                        }, null, () =>
                        {
                            GameManager.UI.HideUIForm("GlobalMaskPanel");
                        });
                    }

                    if (GameManager.Process.CurrentProcessName != null)
                    {
                        GameManager.Process.RegisterAfter(GameManager.Process.CurrentProcessName, ProcessType.ShowClimbBeanstalkEndProcess, ActivityEnd);
                    }
                    else
                    {
                        ActivityEnd();
                    }
                }
                else
                {
                    EndActivity();
                }
            }
        }
    }

    public void EndActivity()
    {
        GameManager.Activity.EndActivity(CurrentActivityID);
        ClimbBeanstalkModel.Instance.Init(-1, 1);
    }

    public override void ActivityPreEndProcess()
    {
        if (!CheckActivityHasStarted())
            return;

        //如果爬藤有奖励可以领取时
        //最后会触发 ClimbBeanstalkGO.ClaimReward()
        ClimbBeanstalkTaskDatas firstUnclaimedTask = null;
        if (HasRewardToClaim(out firstUnclaimedTask))
        {
            GameManager.PlayerData.IsFirstShowEveryday = false;

            GameManager.Process.RegisterAfter(GameManager.Process.CurrentProcessName, ProcessType.CheckClimbBeanstalk, () =>
            {
                ShowClimbBeanstalkMenu("ClimbBeanstalkMenu");
            });

            return;
        }

        //如果玩家掉落了
        if (CurrentWinStreak < LastWinStreakNum)
        {
            GameManager.PlayerData.IsFirstShowEveryday = false;

            GameManager.Process.RegisterAfter(GameManager.Process.CurrentProcessName, ProcessType.CheckClimbBeanstalk, () =>
            {
                ShowClimbBeanstalkMenu("ClimbBeanstalkMenu");
            });

            return;
        }
    }

    public override void ActivityAfterStartProcess()
    {
        if (!CheckActivityHasStarted())
            return;

        //如果当前阶段已经完成 且有下一阶段 那么进入下一阶段
        //没有的话 就完成该活动了
        if (ThisPhaseIsFinished())
        {
            if (HasNextPhaseToEnter())
            {
                EnterNextPhase();

                GameManager.PlayerData.IsFirstShowEveryday = false;

                GameManager.Process.RegisterAfter(GameManager.Process.CurrentProcessName, ProcessType.CheckClimbBeanstalk, () =>
                {
                    ShowClimbBeanstalkMenu("ClimbBeanstalkMenu");
                });
            }

            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.WinStreak_StageReward, new Firebase.Analytics.Parameter("ActivityID", CurrentActivityID), new Firebase.Analytics.Parameter("StageID", CurrentPhaseID));

            return;
        }

        // 活动已开启并解锁，每日第一次登录强制弹出活动界面
        if (GameManager.PlayerData.IsFirstShowEveryday &&
            GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockClimbBeanstalkEventLevel)
        {
            GameManager.PlayerData.IsFirstShowEveryday = false;

            GameManager.Process.RegisterAfter(GameManager.Process.CurrentProcessName, ProcessType.CheckClimbBeanstalk, () =>
            {
                ShowClimbBeanstalkMenu("ClimbBeanstalkWelcomeMenu");
            });

            return;
        }
    }

    public override void OnLevelWin(int levelFailTime, int hardIndex)
    {
        if (CheckLevelWinCanGetTarget())
        {
            int collectNum = 1;
            if (LastWinStreakNum > CurrentWinStreak)
                LastWinStreakNum = CurrentWinStreak;
            CurrentWinStreak += collectNum;
            NeedFlyReward = true;
        }
    }

    public override void OnLevelLose()
    {
        if (CheckActivityHasStarted())
        {
            //玩家从0->1 但未进入界面 这种情况下再输掉 其实也希望能播1->0掉落
            //另外再避免玩家重玩后再输掉的情况
            if (CurrentWinStreak >= LastWinStreakNum)
            {
                LastWinStreakNum = CurrentWinStreak;
            }
            CurrentWinStreak = 0;
        }
    }

    public bool HasRewardToClaim(out ClimbBeanstalkTaskDatas firstUnclaimedTask)
    {
        firstUnclaimedTask = null;
        if (!CheckActivityHasStarted())
            return false;

        List<ClimbBeanstalkTaskDatas> dataList = DataTable.GetRecentClimbBeanstalkTaskDatas();
        for (int i = 0; i < dataList.Count; ++i)
        {
            bool isActuallyAchieved = dataList[i].TargetNum <= CurrentWinStreak;
            bool hasNotBeenObserved = dataList[i].TargetNum > LastWinStreakNum;
            bool hasNotEverBeenClaimed = dataList[i].TargetNum > HighestClaimRewardNum;
            bool doContainReward = dataList[i].RewardTypeList.Count > 0;
            if (isActuallyAchieved && hasNotBeenObserved && hasNotEverBeenClaimed && doContainReward)
            {
                firstUnclaimedTask = dataList[i];
                return true;
            }
        }
        return false;
    }

    public string GetClimbBeanstalkMenuName(string menuName)
    {
        switch (ActivityTypeIndex)
        {
            case 1:
                menuName = $"{menuName}1";
                break;
            default:
                break;
        }

        return menuName;
    }

    public void ShowClimbBeanstalkMenu(string menuName)
    {
        GameManager.UI.ShowUIForm(GetClimbBeanstalkMenuName(menuName));
    }

    private bool ThisPhaseIsFinished()
    {
        if (!CheckActivityHasStarted())
            return false;

        List<ClimbBeanstalkTaskDatas> dataList = DataTable.GetRecentClimbBeanstalkTaskDatas();
        int highestTargetNum = -1;
        for (int i = 0; i < dataList.Count; ++i)
        {
            if (dataList[i].TargetNum > highestTargetNum)
                highestTargetNum = dataList[i].TargetNum;
        }

        return HighestClaimRewardNum >= highestTargetNum;
    }

    private bool HasNextPhaseToEnter()
    {
        bool nextPhaseExist = dataTable.CheckTargetActivityIDAndPhaseIDExist(CurrentActivityID, CurrentPhaseID + 1);
        return nextPhaseExist;
    }

    private void EnterNextPhase()
    {
        ClimbBeanstalkModel.Instance.Init(CurrentActivityID, CurrentPhaseID + 1);
        dataTable.FilterTaskDatas(CurrentActivityID, CurrentPhaseID);
    }

    public bool CheckActivityStateNeedToLockProcess()
    {
        //如果爬藤有奖励可以领取时
        ClimbBeanstalkTaskDatas firstUnclaimedTask = null;
        if (HasRewardToClaim(out firstUnclaimedTask))
            return true;

        //玩家在本次登录过程中 到达了解锁等级的话
        //并且真的可能会开启活动时
        if (GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockClimbBeanstalkEventLevel && !EverShowedFirstTimeIntro)
        {
            if (GameManager.Activity.CheckActivityCanStart(ActivityID))
                return true;
        }

        //如果玩家掉落了
        if (CurrentWinStreak < LastWinStreakNum)
            return true;

        //如果当前阶段已经完成 且有下一阶段
        if (ThisPhaseIsFinished() && HasNextPhaseToEnter())
        {
            return true;
        }
        return false;
    }
}
