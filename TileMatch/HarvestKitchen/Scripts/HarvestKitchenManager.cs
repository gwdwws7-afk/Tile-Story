using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Analytics;
using GameFramework.Event;
using MySelf.Model;
using UnityEngine;

public class HarvestKitchenManager : ActivityManagerBase
{
    public static HarvestKitchenManager Instance { get; private set; }

    public const string LEVEL_WIN_CHEF_HAT_NUM = "HarvestKitchenLevelWinChefHatNum";
    public const string ENTER_MAIN_MENU_TYPE = "HarvestKitchenEnterMainMenuType";
    
    public override int ActivityID => 100004;
    public const string ASSET_NAME = "HarvestKitchenMainMenu";
    
    private DTHarvestKitchenTaskData taskData;
    private DTHarvestKitchenLevelData levelData;
    private DTHarvestKitchenChestData chestData;
    
    public DTHarvestKitchenTaskData TaskData
    {
        get
        {
            if (taskData == null)
            {
                taskData = GameManager.DataTable.GetDataTable<DTHarvestKitchenTaskData>().Data;
            }

            return taskData;
        }
    }
    
    public DTHarvestKitchenLevelData LevelData
    {
        get
        {
            if (levelData == null)
            {
                levelData = GameManager.DataTable.GetDataTable<DTHarvestKitchenLevelData>().Data;
            }

            return levelData;
        }
    }

    public DTHarvestKitchenChestData ChestData
    {
        get
        {
            if (chestData == null)
            {
                chestData = GameManager.DataTable.GetDataTable<DTHarvestKitchenChestData>().Data;
            }
            
            return chestData;
        }
    }

    public bool IsFirstStart
    {
        get => HarvestKitchenModel.Instance.IsFirstStart;
        set => HarvestKitchenModel.Instance.IsFirstStart = value;
    }

    public bool IsFirstOpenMainMenu
    {
        get => HarvestKitchenModel.Instance.IsFirstOpenMainMenu;
        set => HarvestKitchenModel.Instance.IsFirstOpenMainMenu = value;
    }
    
    public bool IsShowGameGuide
    {
        get => HarvestKitchenModel.Instance.IsShowGameGuide;
        set => HarvestKitchenModel.Instance.IsShowGameGuide = value;
    }
    
    public bool IsShowTopGuide
    {
        get => HarvestKitchenModel.Instance.IsShowTopGuide;
        set => HarvestKitchenModel.Instance.IsShowTopGuide = value;
    }

    public bool IsShowChooseBarGuide
    {
        get => HarvestKitchenModel.Instance.IsShowChooseBarGuide;
        set => HarvestKitchenModel.Instance.IsShowChooseBarGuide = value;
    }
    
    public int CurrentActivityID
    {
        get => HarvestKitchenModel.Instance.CurrentActivityID;
        set => HarvestKitchenModel.Instance.CurrentActivityID = value;
    }
    
    public int CurrentLevel
    {
        get => HarvestKitchenModel.Instance.CurrentLevel;
        set => HarvestKitchenModel.Instance.CurrentLevel = value;
    }

    public int FailedNum
    {
        get => HarvestKitchenModel.Instance.FailedNum;
        set => HarvestKitchenModel.Instance.FailedNum = value;
    }

    public int TaskId
    {
        get => HarvestKitchenModel.Instance.TaskId;
        set => HarvestKitchenModel.Instance.TaskId = value;
    }

    public int ChestId
    {
        get => HarvestKitchenModel.Instance.ChestId;
        set => HarvestKitchenModel.Instance.ChestId = value;
    }

    public int PraiseNum
    {
        get => HarvestKitchenModel.Instance.PraiseNum;
        set => HarvestKitchenModel.Instance.PraiseNum = value;
    }

    public int OldPraiseNum
    {
        get => HarvestKitchenModel.Instance.OldPraiseNum;
        set => HarvestKitchenModel.Instance.OldPraiseNum = value;
    }

    public int BasketNum
    {
        get => GameManager.PlayerData.GetCurItemNum(TotalItemData.KitchenBasket);
    }

    public int ChallengeNum
    {
        get => HarvestKitchenModel.Instance.ChallengeNum;
        set => HarvestKitchenModel.Instance.ChallengeNum = value;
    }

    public void UseBasket(int num)
    {
        GameManager.PlayerData.UseItem(TotalItemData.KitchenBasket, num, false);
    }

    public void AddBasket(int num)
    {
        GameManager.PlayerData.AddItemNum(TotalItemData.KitchenBasket, num);
    }

    public DTHarvestKitchenChestDatas GetCurrentChestData()
    {
        int chestId = ChestId;
        if (chestId < ChestData.KitchenChestDatas.Count)
        {
            return ChestData.KitchenChestDatas[chestId];
        }

        return null;
    }

    public int GetCurrentChestProgress(int chestId)
    {
        int finishedDishNum = TaskId - 1;
        for (int i = 0; i < ChestData.KitchenChestDatas.Count; i++)
        {
            if (ChestData.KitchenChestDatas[i].ID < chestId)
                finishedDishNum -= ChestData.KitchenChestDatas[i].TargetDishNum;
            else
                break;
        }

        return finishedDishNum;
    }
    
    public DateTime StartTime
    {
        get => new DateTime(2025, 11, 15);
    }
    
    public DateTime EndTime
    {
        get => new DateTime(2025, 12, 1);
    }

    public int PeriodId
    {
        get => 1;
    }
    
#region 临时数据，每局游戏开始时需重置

    // 是否开启棋子点击响应
    [HideInInspector]
    public bool canClickTile = false;
    // 当前关是否能看广告接关
    [HideInInspector]
    public bool canWatchAdsContinue = true;
    // 打开GameContinueMenu的方式,0:关卡失败; 1:主动退出
    [HideInInspector]
    public int toContinueMenuType = 1;
    // 接关的次数
    [HideInInspector]
    public int continueOfCoinNum = 0;
    // 棋子上升的速度
    [HideInInspector]
    public float tileItemSpeed = 25f;
    // 棋子icon的存储字典
    private Dictionary<int, Sprite> iconDict = null;
    
#endregion
    
    private void Awake()
    {
        Instance = this;
    }

    // 检测是否能初始化活动
    public override bool CheckActivityCanStart()
    {
        if (!CheckInitializationComplete())
            return false;

        if (GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockHarvestKitchenLevel)
            return false;

        if (GameManager.Network.CheckInternetIsNotReachable())
            return false;

        if (!CheckHaveAsset())
            return false;

        if (CheckActivityHasStarted())
            return false;

        return DateTime.Now >= StartTime && DateTime.Now < EndTime;
    }

    public bool CheckActivityCanEnd()
    {
        return DateTime.Now > EndTime || ChestId >= 5;
    }
    
    public override bool CheckActivityHasStarted()
    {
        // 活动ID大于0，并且活动未被记录为开放过的活动
        return CurrentActivityID > 0 && !HarvestKitchenModel.Instance.EverFinishedTargetActivityID(CurrentActivityID);
    }

    public override void ActivityStartProcess()
    {
        if (CheckActivityCanStart())
        {
            int periodID = PeriodId;
            if (periodID != 0)
            {
                if (!HarvestKitchenModel.Instance.EverFinishedTargetActivityID(periodID))
                {
                    if (CurrentActivityID != periodID)
                    {
                        // 清除剩余的厨师帽
                        if (BasketNum > 0) UseBasket(BasketNum);

                        HarvestKitchenModel.Instance.Init(periodID);
                        AddBasket(5);
                        GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.KitchenInfoChanged));
                        GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.KitchenEntranceUpdate));
                    }
                }
            }
        }
    }

    public override void ActivityEndProcess()
    {
        if (!CheckActivityHasStarted() || !CheckHaveAsset()) return;
        
        if (CheckActivityCanEnd())
        {
            EndActivity();
        }
    }
    
    public void EndActivity()
    {
        HarvestKitchenModel.Instance.RecordFinishedActivityId(CurrentActivityID);
        HarvestKitchenModel.Instance.Init(-1);
        
        GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.KitchenInfoChanged));
    }

    public override void ActivityPreEndProcess()
    {
        if (!CheckActivityHasStarted() || !CheckHaveAsset()) return;
        // 活动结束弹窗
        if (CheckActivityCanEnd())
        {
            GameManager.Process.RegisterAfter(GameManager.Process.CurrentProcessName, ProcessType.CheckHarvestKitchen, () =>
            {
                GameManager.UI.ShowUIForm("HarvestKitchenEndMenu");
            });
            return;
        }
    }

    public override void ActivityAfterStartProcess()
    {
        if (!CheckHaveAsset()) return;

        //弹解锁提示框
        if (GameManager.PlayerData.NowLevel == Constant.GameConfig.UnlockKitchenButtonLevel&&!GameManager.DataNode.GetData("ShowHarvestKitchenLockMenu",false))
        {
            GameManager.DataNode.SetData("ShowHarvestKitchenLockMenu", true);
            GameManager.Process.RegisterAfter(GameManager.Process.CurrentProcessName, ProcessType.ShowHarvestKitchenLockMenu, () =>
            {
                GameManager.UI.ShowUIForm("HarvestKitchenWelcomeMenu");
            });
            return;
        }
        
        // 活动弹窗（活动开启的宣传弹窗，活动每日弹窗）
        if (CheckActivityHasStarted() && IsFirstStart)
        {
            IsFirstStart = false;
            GameManager.PlayerData.IsFirstShowEveryDay_Kitchen = false;
            GameManager.Process.RegisterAfter(GameManager.Process.CurrentProcessName, ProcessType.CheckHarvestKitchen, () =>
            {
                GameManager.UI.ShowUIForm("HarvestKitchenWelcomeMenu");
            });
            return;
        }

        if (!CheckActivityIsUnlock()) return;

        int consumeChefNUm = GetCurrentTaskConsumeBasketNum();
        int currentChallengeNum = BasketNum / consumeChefNUm;
        // 厨师帽数量大于开启关卡的消耗数量 && 当前的挑战次数不等于记录的挑战次数 && 挑战次数为2的倍数
        bool popMenu = BasketNum > consumeChefNUm && currentChallengeNum != ChallengeNum && currentChallengeNum % 5 == 0;
        
        if (popMenu || GameManager.PlayerData.IsFirstShowEveryDay_Kitchen)
        {
            ChallengeNum = currentChallengeNum;
            GameManager.PlayerData.IsFirstShowEveryDay_Kitchen = false;
            GameManager.Process.RegisterAfter(GameManager.Process.CurrentProcessName, ProcessType.CheckHarvestKitchen, () =>
            {
                GameManager.UI.ShowUIForm("HarvestKitchenMainMenu");
            });
            return;
        }
    }

    /// <summary>
    /// 关卡胜利
    /// </summary>
    /// <param name="levelFailTime">关卡失败次数</param>
    /// <param name="hardIndex">关卡难度</param>
    public override void OnLevelWin(int levelFailTime, int hardIndex)
    {
        // 判定 NowLevel 等于 UnlockKitchenLevel 的时候不获取道具是因为在 OnLevelWin 执行时 NowLevel 已经增加了，所以实际的胜利关卡是 NowLevel-1
        if (!CheckActivityIsUnlock() || GameManager.PlayerData.NowLevel <= Constant.GameConfig.UnlockHarvestKitchenLevel) return;
        int num = GetPropNum(hardIndex);
        if (levelFailTime == 0)
            num *= 3;
        GameManager.DataNode.SetData<int>(LEVEL_WIN_CHEF_HAT_NUM, num);
        // 普通关 1；困难关 2；超难关 3
        AddBasket(num);
    }

    /// <summary>
    /// 根据关卡难度判断获取的厨师帽数量
    /// </summary>
    /// <param name="hardIndex">关卡难度</param>
    /// <returns>获得的厨师帽数量</returns>
    public int GetPropNum(int hardIndex)
    {
        switch (hardIndex)
        {
            case 1:
                return 2;
            case 2:
                return 3;
            default:
                return 1;
        }
    }
    
    public override void OnLevelLose()
    {
        //if (!CheckActivityIsUnlock()) return;
    }
    
    public bool UseBasketNumOpenLevel()
    {
        int num = GetCurrentTaskConsumeBasketNum();
        if (BasketNum < num) return false;
        UseBasket(num);
        RefreshChallengeNum(num);
        
        GameManager.DataNode.SetData("HarvestKitchenShowedRV", false);
        
        return true;
    }

    public bool CanOpenGame()
    {
        return BasketNum >= GetCurrentTaskConsumeBasketNum();
    }

    public int GetCurrentChallengeCount()
    {
        return Mathf.Max(BasketNum / GetCurrentTaskConsumeBasketNum(), 0);
    }

    /// <summary>
    /// 活动关卡胜利
    /// </summary>
    public void ActivityLevelWin(int num)
    {
        // 关卡难度增加
        CurrentLevel += 1;
        // 清空失败次数
        FailedNum = 0;
        // 增加点赞数
        OldPraiseNum = PraiseNum;
        PraiseNum += num;
    }

    /// <summary>
    /// 活动关卡失败
    /// </summary>
    public void ActivityLevelLose()
    {
        // 记录关卡失败次数
        FailedNum += 1;
    }
    
    public int GetAddPraise()
    {
        return PraiseNum - OldPraiseNum;
    }

    /// <summary>
    /// 获取当前的关卡数据
    /// </summary>
    /// <returns>当前的关卡数据</returns>
    public DTHarvestKitchenLevelDatas GetCurrentLevelData()
    {
        int level = Mathf.Min(TaskData.GetKitchenTaskIndex(TaskId), LevelData.KitchenLevelDatas.Count - 1);
        Log.Info($"Kitchen: 当前读取的关卡为{level}   当前任务{TaskId}");
        return LevelData.GetKitchenLevelDataByLevel(level);
    }

    /// <summary>
    /// 获取当前的任务数据
    /// </summary>
    /// <returns>任务数据</returns>
    public DTHarvestKitchenTaskDatas GetCurrentTaskDatas()
    {
        if (TaskId <= 0)
        {
            return AccpetNextTaskDatas();
        }
        return TaskData.GetKitchenTaskDataById(TaskId);
    }

    /// <summary>
    /// 获取当前任务，开启一局游戏需要消耗的厨师帽数量
    /// </summary>
    /// <returns>消耗的厨师帽数量</returns>
    public int GetCurrentTaskConsumeBasketNum()
    {
        DTHarvestKitchenTaskDatas data = GetCurrentTaskDatas();
        if(data != null)
            return data.ChallengeToolNumber;
        return 3;
    }

    /// <summary>
    /// 接取任务
    /// </summary>
    /// <returns>任务数据</returns>
    public DTHarvestKitchenTaskDatas AccpetNextTaskDatas()
    {
        int index = 0;
        if (TaskId > 0)
        {
            index = TaskData.GetKitchenTaskIndex(TaskId);
            index++;
        }
        if (TaskData.KitchenTaskDatas.Count > index)
        {
            TaskId = TaskData.KitchenTaskDatas[index].ID;
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Kitchen_Match_Task_Stage,
                new Parameter("stage", TaskId));
            return TaskData.KitchenTaskDatas[index];
        }
        return null;
    }

    public HarvestKitchenStageArea GetStageArea()
    {
        var uiForm = GameManager.UI.GetUIForm("HarvestKitchenMainMenu") as HarvestKitchenMainMenu;
        if (uiForm != null)
        {
            return uiForm.stageArea;
        }

        return null;
    }
    
    // 活动是否解锁
    public bool CheckActivityIsUnlock()
    {
        if (!CheckInitializationComplete())
            return false;
        return CheckActivityHasStarted() && GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockHarvestKitchenLevel;
    }

    // 是否显示按钮
    public bool CheckCanShowEntrance()
    {
        if (!CheckInitializationComplete())
            return false;

        if (GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockKitchenButtonLevel)
            return false;

        if (!CheckHaveAsset())
            return false;

        if (CheckActivityHasStarted())
        {
            return DateTime.Now < EndTime;
        }
        else if (GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockHarvestKitchenLevel) 
        {
            return DateTime.Now >= StartTime && DateTime.Now < EndTime;
            //return CheckActivityCanStart();
        }

        return false;
    }

    // 检测活动是否能获取道具
    public bool CheckLevelWinCanGetTarget()
    {
        if (!CheckInitializationComplete())
            return false;

        if (GameManager.Network.CheckInternetIsNotReachable() & DateTime.Now >= EndTime)
            return false;

        return CheckActivityHasStarted() && GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockHarvestKitchenLevel &&
               CheckHaveAsset();
    }

    public Sprite GetTileSpriteById(int tileId)
    {
        if (iconDict == null)
            iconDict = new Dictionary<int, Sprite>();
        if (iconDict.TryGetValue(tileId, out Sprite s))
            return s;
        Sprite sp = AddressableUtils.LoadAsset<Sprite>($"HarvestKitchenIcon[{tileId}]");
        iconDict.Add(tileId, sp);
        return sp;
    }

    // 离开游戏界面时，重置每局的临时数据
    public void ResetTemporaryData()
    {
        canClickTile = false;
        canWatchAdsContinue = true;
        toContinueMenuType = 1;
        continueOfCoinNum = 0;
        tileItemSpeed = 25f;
        iconDict.Clear();
    }

    public Dictionary<TotalItemData, int> GetCanClaimmReward()
    {
        int usePraise = 0;
        Dictionary<TotalItemData, int> rewardDict = new Dictionary<TotalItemData, int>();
        for (int i = 0; i < TaskData.KitchenTaskDatas.Count; i++)
        {
            DTHarvestKitchenTaskDatas data = TaskData.KitchenTaskDatas[i];
            if (data.ID < TaskId) continue;
            if (PraiseNum - usePraise < data.TargetPraise)
                break;
                
            for (int j = 0; i < data.RewardsList.Count; i++)
            {
                if (rewardDict.ContainsKey(data.RewardsList[j]))
                    rewardDict[data.RewardsList[j]] += data.RewardsNumList[j];
                else
                    rewardDict.Add(data.RewardsList[j], data.RewardsNumList[j]);
            }

            usePraise += data.TargetPraise;
        }

        return rewardDict;
    }

    public float GetCurrentTaskProgress()
    {
        if (CheckActivityHasStarted())
            return (float)PraiseNum / GetCurrentTaskDatas().TargetPraise;
        return 0;
    }

    public int GetContinueNeedCoin()
    {
        return continueOfCoinNum == 0 ? 900 : 1800;
    }

    /// <summary>
    /// 消耗厨师帽或任务变更时更新挑战次数
    /// </summary>
    /// <param name="consumeNum">挑战一次消耗的厨师帽</param>
    public void RefreshChallengeNum(int consumeNum)
    {
        ChallengeNum = BasketNum / consumeNum;
    }
    
    #region Download

    public HarvestKitchenManager.DownloadStatus IsDownloadComplete { get; private set; }

    public bool CheckHaveAsset()
    {
        return AddressableUtils.IsHaveAssetSync(ASSET_NAME, out long size);
    }

    public void DownloadAsset()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            IsDownloadComplete = HarvestKitchenManager.DownloadStatus.Fail;
            return;
        }

        if (GameManager.Download.IsDownloading(ASSET_NAME))
        {
            IsDownloadComplete = HarvestKitchenManager.DownloadStatus.Downloading;
            return;
        }

        GameManager.Download.AddDownload(ASSET_NAME);
    }

    private void OnDownloadSuccess(object sender, GameEventArgs e)
    {
        DownloadSuccessEventArgs ne = e as DownloadSuccessEventArgs;
        if (ne != null && ne.DownloadKey == ASSET_NAME)
        {
            IsDownloadComplete = HarvestKitchenManager.DownloadStatus.Success;
        }
    }

    private void OnDownloadFailure(object sender, GameEventArgs e)
    {
        DownloadFailureEventArgs ne = e as DownloadFailureEventArgs;
        if (ne != null && ne.DownloadKey == ASSET_NAME)
        {
            IsDownloadComplete = HarvestKitchenManager.DownloadStatus.Fail;
        }
    }

    public enum DownloadStatus
    {
        None,
        Success,
        Fail,
        Downloading,
    }

    #endregion
}
