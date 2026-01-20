using GameFramework.Event;
using GameFramework.Fsm;
using GameFramework.Procedure;
using System;
using System.Collections.Generic;
using MySelf.Model;
using UnityEngine;

/// <summary>
/// 资源预加载流程
/// </summary>
public sealed class ProcedureResourcesPreload : ProcedureBase
{
    private TaskPool<ConditionTriggerTask> preloadTaskPool = new TaskPool<ConditionTriggerTask>();
    private SplashLoadingMenuManager splashLoadingMenu;
    private bool loadDatatableComplete = false;
    private int totalTaskCount = 0;
    private int finishedTaskCount = 0;
    private bool isDownloading = false;

    public override string ProcedureName => "ProcedureResourcesPreload";

    public override void OnEnter(IFsm<ProcedureManager> fsm)
    {
        base.OnEnter(fsm);

        GameManager.Event.Subscribe(DownloadSuccessEventArgs.EventId, OnDownloadSuccess);
        GameManager.Event.Subscribe(DownloadFailureEventArgs.EventId, OnDownloadFailure);
        GameManager.Event.Subscribe(DownloadUpdateEventArgs.EventId, OnDownUpdate);

        splashLoadingMenu = GameManager.UI.GetUIForm("SplashLoadingMenuManager") as SplashLoadingMenuManager;

        preloadTaskPool.AddAgent(new ConditionTriggerTaskAgent());
        
        //初始化AppsFlyer
        AddLoadTriggerTask(() =>
        {
            GameManager.AppsFlyer.Init();
        }, () =>
        {
            return false;
        }, OnLoadingTaskComplete, OnLoadingTaskComplete,0.1f);

        //初始化Firebase
        AddLoadTriggerTask(() =>
        {
            GameManager.Firebase.InitFirebaseApp();
        }, () =>
        {
            return GameManager.Firebase.IsInitRemoteConfig;
        }, OnLoadingTaskComplete, OnLoadingTaskComplete, SystemInfoManager.DeviceType <= DeviceType.Normal ? 10 : 5);
        
        //预加载数据表
        PreloadDatatable();

        //初始化广告模块
        AddLoadTriggerTask(() =>
        {
            GameManager.Ads.Init();
        }, () =>
        {
            return GameManager.Ads.IsInitComplete || GameManager.PlayerData.NowLevel < AdsCommon.AdsInitLevel;
        }, OnLoadingTaskComplete, OnLoadingTaskComplete, SystemInfoManager.DeviceType <= DeviceType.Normal ? 5 : 2);

        //初始化购买模块
        AddLoadTriggerTask(() =>
        {
            GameManager.Purchase.Init();
        }, () =>
        {
            return GameManager.Purchase.CheckInitComplete();
        }, OnLoadingTaskComplete, OnLoadingTaskComplete,1f);

        //初始化推送模块
        AddLoadTriggerTask(() =>
        {
            GameManager.Notification.Init();
        }, () =>
        {
            return false;
        }, OnLoadingTaskComplete, OnLoadingTaskComplete,0.1f);
        
        //初始化玩家数据模块
        AddLoadTriggerTask(() =>
        {
            GameManager.PlayerData.Init();
        }, () =>
        {
            return false;
        }, OnLoadingTaskComplete, OnLoadingTaskComplete,0.1f);

        AddLoadTriggerTask(() =>
        {
            //初始化成就系统
            GameManager.Objective.Initialize();
        }, () =>
        {
            return false;
        }, OnLoadingTaskComplete, OnLoadingTaskComplete,0.1f);

        AddLoadTriggerTask(() =>
        {
            GameManager.Activity.Initialize();
        }, () =>
        {
            return GameManager.Activity.CheckInitializationComplete();
        }, OnLoadingTaskComplete, OnLoadingTaskComplete, 0);

        DownloadNewArea();
        
        AddLoadTriggerTask(() =>
        {
            SystemInfoManager.CheckIsSpecialDeviceTurnOffParticleEffect();
        }, () =>
        {
            return false;
        }, OnLoadingTaskComplete, OnLoadingTaskComplete, 0.1f);

        AddLoadTriggerTask(() =>
        {
            SystemInfoManager.CheckIsSpecialDeviceCloseLowPriorityPop();
        }, () =>
        {
            return false;
        }, OnLoadingTaskComplete, OnLoadingTaskComplete, 0.1f);

        AddLoadTriggerTask(() =>
        {
            SystemInfoManager.CheckIsSpecialDeviceOptimizeHeavyWork();
        }, () =>
        {
            return false;
        }, OnLoadingTaskComplete, OnLoadingTaskComplete, 0.1f);

        totalTaskCount = preloadTaskPool.WaitingTaskCount + preloadTaskPool.WorkingAgentCount;
    }

    public override void OnUpate(IFsm<ProcedureManager> fsm, float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpate(fsm, elapseSeconds, realElapseSeconds);

        preloadTaskPool.Update(elapseSeconds, realElapseSeconds);

        if (isDownloading || preloadTaskPool.WorkingAgentCount > 0 || preloadTaskPool.WaitingTaskCount > 0 || !splashLoadingMenu.CheckSliderAnimComplete())
        {
            return;
        }

        fsm.SetData("ProcedureSkipType",ProcedureSkipType.MenuToMap);
        ChangeState<ProcedureMapPreload>(fsm);
    }

    public override void OnLeave(IFsm<ProcedureManager> fsm, bool isShutdown)
    {
        GameManager.Event.Unsubscribe(DownloadSuccessEventArgs.EventId, OnDownloadSuccess);
        GameManager.Event.Unsubscribe(DownloadFailureEventArgs.EventId, OnDownloadFailure);
        GameManager.Event.Unsubscribe(DownloadUpdateEventArgs.EventId, OnDownUpdate);

        preloadTaskPool.Shutdown();
        loadDatatableComplete = false;
        splashLoadingMenu = null;
        totalTaskCount = 0;
        finishedTaskCount = 0;

        base.OnLeave(fsm, isShutdown);
    }

    private void PreloadDatatable()
    {
        QuickAddDTLoadRequest<DTEndlessTreasureData>("DTEndlessTreasureData");
        QuickAddDTLoadRequest<DTEndlessTreasureScheduleData>("DTEndlessTreasureScheduleData");
        QuickAddDTLoadRequest<DTProductID>("DTProductID");
        QuickAddDTLoadRequest<DTShopPackageData>("DTShopPackageData");
        QuickAddDTLoadRequest<DTBGID>("DTBGID");
        QuickAddDTLoadRequest<DTTileID>("DTTileID");
        QuickAddDTLoadRequest<DTDecorateArea>("DTDecorateArea");
        QuickAddDTLoadRequest<DTDecorateItem>("DTDecorateItem");
        QuickAddDTLoadRequest<DTHelp>("DTHelp");
        QuickAddDTLoadRequest<DTLevelID>("DTLevelID");
        QuickAddDTLoadRequest<DTLevelTypeID>("DTLevelTypeID");
        QuickAddDTLoadRequest<DTIdleDialogue>("DTIdleDialogue");
        QuickAddDTLoadRequest<DTPersonRankTaskData>("DTPersonRankTaskData");
        //QuickAddDTLoadRequest<RandomNamesConfig>();
        QuickAddDTLoadRequest<DTNormalTurntable>("DTNormalTurntable");
        QuickAddDTLoadRequest<DTNotificationData>("DTNotificationData");
        QuickAddDTLoadRequest<DTClimbBeanstalkTaskData>("DTClimbBeanstalkTaskData");
        // QuickAddDTLoadRequest<DTClimbBeanstalkScheduleData>();
        QuickAddDTLoadRequest<DTTotalItemType>("DTTotalItemType");
        QuickAddDTLoadRequest<DTCalendarChallengeData>("DTCalendarChallengeData");
        QuickAddDTLoadRequest<DTLevelReward>("DTLevelReward");
        QuickAddDTLoadRequest<DTLoginGift>("DTLoginGift");
        QuickAddDTLoadRequest<DTGoldCollectionData>("DTGoldCollectionData");
        QuickAddDTLoadRequest<DTGoldCollectionScheduleData>("DTGoldCollectionScheduleData");
        QuickAddDTLoadRequest<DTTilePassData>("DTTilePassData");
        QuickAddDTLoadRequest<DTTilePassScheduleData>("DTTilePassScheduleData");
        QuickAddDTLoadRequest<DTBalloonRiseScheduleData>("DTBalloonRiseScheduleData");
        QuickAddDTLoadRequest<DTBalloonRiseReward>("DTBalloonRiseReward");
        QuickAddDTLoadRequest<DTActivityRoutineEventScheduleData>("DTActivityRoutineEventScheduleData");
        QuickAddDTLoadRequest<DTGlacierQuestScheduleData>("DTGlacierQuestScheduleData");
        QuickAddDTLoadRequest<DTKitchenLevelData>("DTKitchenLevelData");
        QuickAddDTLoadRequest<DTKitchenTaskData>("DTKitchenTaskData");
        QuickAddDTLoadRequest<DTHarvestKitchenLevelData>("DTHarvestKitchenLevelData");
        QuickAddDTLoadRequest<DTHarvestKitchenTaskData>("DTHarvestKitchenTaskData");
        QuickAddDTLoadRequest<DTHarvestKitchenChestData>("DTHarvestKitchenChestData");
        QuickAddDTLoadRequest<DTFrogJumpData>("DTFrogJumpData");
        QuickAddDTLoadRequest<DTCardScheduleData>("DTCardScheduleData");
        QuickAddDTLoadRequest<DTCardInfoData>("DTCardInfoData");
        QuickAddDTLoadRequest<DTCardSetData>("DTCardSetData");
        QuickAddDTLoadRequest<DTCardStarRewardData>("DTCardStarRewardData");
        QuickAddDTLoadRequest<DTCardPackData>("DTCardPackData");
    }

    private void OnReadDataSuccess(GameEventMessage e)
    {
        string dataName = e.Items[0].ToString();
        GameFramework.ReferencePool.Release(e);
        loadDatatableComplete = true;

        Log.Info("Read {0} success", dataName);
    }

    private void OnReadDataFailure(GameEventMessage e)
    {
        string dataName = e.Items[0].ToString();
        GameFramework.ReferencePool.Release(e);
        loadDatatableComplete = true;

        Log.Info("Read {0} fail", dataName);
    }

    private void AddLoadTriggerTask(Action startAction, Func<bool> checkCompleteAction, Action completeAction = null, Action failAction = null, float timeOut = 0f)
    {
        preloadTaskPool.AddTask(ConditionTriggerTask.Create(startAction, checkCompleteAction, completeAction, failAction, timeOut));
    }

    /// <summary>
    /// 快捷数据表加载请求
    /// </summary>
    /// <typeparam name="T">数据表类型</typeparam>
    /// <param name="dataTableAssetName">数据表名称</param>
    private void QuickAddDTLoadRequest<T>(string dataTableAssetName) where T : class
    {
        if (GameManager.DataTable.HasDataTable<T>()) return;

        AddLoadTriggerTask(() =>
        {
            IDataTable<T> rewardTaskDataTable = GameManager.DataTable.CreateDataTable<T>();
            rewardTaskDataTable.ReadData(GetNewTableName(dataTableAssetName), OnReadDataSuccess, OnReadDataFailure);
        }, () =>
        {
            return loadDatatableComplete;
        }, () =>
        {
            loadDatatableComplete = false;
            OnLoadingTaskComplete();
        }, () =>
        {
            loadDatatableComplete = false;
            OnLoadingTaskComplete();
        },timeOut:0.1f);
    }
    
    //Level表做ab
    private string GetNewTableName(string name)
    {
        if (name=="DTLevelID")
        {
            bool isUseOtherTableName = GameManager.Firebase.GetBool(Constant.RemoteConfig.If_Use_B_Level_Table_Data,false);
#if AmazonStore
            if(isUseOtherTableName)GameManager.Firebase.RecordMessageByEvent("Use_B_Level_Table_Data");
#endif
            string tableName=isUseOtherTableName ? "DTLevelIDBTest" : "DTLevelID";
            Log.Info($"GetNewTableName:{name}=>{tableName}");
            return tableName;
        }
        else if (name == "DTLevelReward")
        {
            bool increaseRewards = GameManager.Firebase.GetBool(Constant.RemoteConfig.Winpanel_Chest_IncreaseRewards,false);
            string tableName = increaseRewards ? "DTLevelRewardBTest" : "DTLevelReward";

            return tableName;
        }
        
        return name;
    }

    private void OnLoadingTaskComplete()
    {
        finishedTaskCount++;
        splashLoadingMenu.SetTargetValue(finishedTaskCount / (float)totalTaskCount);
    }

    private Dictionary<string, long> needDownloadSizeDict = new Dictionary<string, long>();
    private void DownloadNewArea()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            return;
        }

        List<string> getNeedDownloadList = GameManager.PlayerData.GetNeedDownloadByMenu();
        //getNeedDownloadList.Add($"CardSetMainMenu200002");
        var needDownDict = AddressableUtils.GetNeedDownLoadAssetSize(getNeedDownloadList);
        foreach (var name in needDownDict)
        {
            Log.Info($"DownloadNewArea:needDownDict:{name}");
        }
        if (needDownDict.Count <= 0) return;

        isDownloading = true;
        AddLoadTriggerTask(() =>
        {
            foreach (var item in needDownDict)
            {
                GameManager.Download.AddDownload(item.Key);
            }
        }, () => !isDownloading,
    () =>
    {
        Log.Info($"AddDownload Task Success!");
        isDownloading = false;
        OnLoadingTaskComplete();
    }, () =>
    {
        Log.Info($"AddDownload Task Fail!");
        isDownloading = false;
        OnLoadingTaskComplete();
    }, 15f);
    }

    private void OnDownloadSuccess(object sender, GameEventArgs e)
    {
        CheckNeedDownloadAsset(((DownloadSuccessEventArgs)e).DownloadKey, true);
    }

    private void OnDownloadFailure(object sender, GameEventArgs e)
    {
        CheckNeedDownloadAsset(((DownloadFailureEventArgs)e).DownloadKey, false);
    }

    private void OnDownUpdate(object sender, GameEventArgs e)
    {
        DownloadUpdateEventArgs args = (DownloadUpdateEventArgs)e;
        Log.Info("Downloading...{0}...{1}", args.DownloadKey, args.Percent);
    }

    private void CheckNeedDownloadAsset(string key, bool isSuccess)
    {
        Log.Info($"CheckNeedDownloadAsset:{key}:{(isSuccess ? "Success" : "Fail")}");
        needDownloadSizeDict.Remove(key);
        if (needDownloadSizeDict.Count <= 0)
        {
            isDownloading = false;

            OnLoadingTaskComplete();
        }
    }
}
