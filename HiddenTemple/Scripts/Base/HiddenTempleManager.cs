using GameFramework.Event;
using System;
using UnityEngine;

namespace HiddenTemple
{
    /// <summary>
    /// 遗迹寻宝管理器
    /// </summary>
    public sealed class HiddenTempleManager : ActivityManagerBase
    {
        public static HiddenTempleManager Instance { get; private set; }
        public static DataTableComponent DataTable { get; private set; }
        public static PlayerDataComponent PlayerData { get; private set; }

        public override int ActivityID => 100001;
        public const string AssetName = "HiddenTempleMainMenu";

        private int m_NeedLoadCount = 0;

        private void Awake()
        {
            Instance = this;

            DataTable = GetComponentInChildren<DataTableComponent>();
            PlayerData = GetComponentInChildren<PlayerDataComponent>();
        }

        protected override void OnInitialize()
        {
            GameManager.Event.Subscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            GameManager.Event.Subscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);
            GameManager.Event.Subscribe(DownloadSuccessEventArgs.EventId, OnDownloadSuccess);
            GameManager.Event.Subscribe(DownloadFailureEventArgs.EventId, OnDownloadFailure);

            m_NeedLoadCount = 0;
            LoadDataTable<DRGemData>("GemData");
            LoadDataTable<DRChestData>("ChestData");
            LoadDataTable<DRLevelData>("LevelData");
        }

        protected override void OnShutdown()
        {
            GameManager.Event.Unsubscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            GameManager.Event.Unsubscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);
            GameManager.Event.Unsubscribe(DownloadSuccessEventArgs.EventId, OnDownloadSuccess);
            GameManager.Event.Unsubscribe(DownloadFailureEventArgs.EventId, OnDownloadFailure);
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                GameManager.UI.ShowUIForm("HiddenTempleEditorMenu");
            }
        }
#endif

        public override bool CheckInitializationComplete()
        {
            return m_NeedLoadCount <= 0 && base.CheckInitializationComplete();
        }

        /// <summary>
        /// 确认入口是否可以显示
        /// </summary>
        public bool CheckEntranceCanShow()
        {
            if (!CheckInitializationComplete())
                return false;

            if (GameManager.PlayerData.NowLevel < PlayerData.GetActivityUnlockLevel())
                return false;

            //玩家提前完成所有石门，再次返回主界面时，入口消失
            if (PlayerData.GetCurrentStage() > PlayerData.GetMaxStage() && GetUnclaimedChestNum() == 0)
                return false;

            if (!CheckHaveAsset())
                return false;

            if (CheckActivityHasStarted())
            {
                return true;
            }
            else
            {
                return false;
                //return GameManager.Activity.CheckActivityCanStart(ActivityID);
            }
        }

        /// <summary>
        /// 确认活动是否可以开启
        /// </summary>
        public override bool CheckActivityCanStart()
        {
            if (!CheckInitializationComplete())
                return false;

            if (GameManager.PlayerData.NowLevel < PlayerData.GetActivityUnlockLevel())
                return false;

            if (!CheckHaveAsset())
                return false;

            if (CheckActivityHasStarted())
                return false;

            return GameManager.Activity.CheckActivityCanStart(ActivityID);
        }

        /// <summary>
        /// 确认活动是否已经开启
        /// </summary>
        public override bool CheckActivityHasStarted()
        {
            return GameManager.Activity.CheckActivityHasStarted(ActivityID, PlayerData.GetCurActivityPeriod());
        }

        /// <summary>
        /// 确认关卡胜利是否可以获取稿子
        /// </summary>
        public bool CheckLevelWinCanGetPickaxe()
        {
            if (!CheckInitializationComplete())
                return false;

            return CheckActivityHasStarted() && PlayerData.GetCurrentStage() <= PlayerData.GetMaxStage();
        }

        /// <summary>
        /// 获取未领取的宝箱数量
        /// </summary>
        public int GetUnclaimedChestNum()
        {
            int num = 0;
            int curStage = PlayerData.GetCurrentStage();
            for (int i = 1; i < curStage; i++)
            {
                if (!PlayerData.GetChestIsClaimed(i))
                    num++;
            }

            return num;
        }

        public override void ActivityStartProcess()
        {
            if (CheckActivityCanStart())
            {
                int periodID = GameManager.Activity.StartActivity(ActivityID);
                if (periodID != 0)
                {
                    PlayerData.ClearAllData();
                    PlayerData.SetCurActivityPeriod(periodID);
                    PlayerData.AddOpenActivityTime();
                    GameManager.Event.Fire(this, HiddenTempleStartEventArgs.Create(periodID));

                    GameManager.Process.RegisterAfter(GameManager.Process.CurrentProcessName, ProcessType.ShowHiddenTempleStartProcess, () =>
                    {
                        GameManager.UI.ShowUIForm("HiddenTempleStartMenu",form =>
                        {
                        }, () =>
                        {
                            GameManager.Process.EndProcess(ProcessType.ShowHiddenTempleStartProcess);
                        }, false);
                    });
                }
            }
        }

        public override void ActivityEndProcess()
        {
            if (CheckActivityHasStarted() && CheckHaveAsset())
            {
                bool isFinishedAll = PlayerData.GetCurrentStage() > PlayerData.GetMaxStage() && GetUnclaimedChestNum() == 0;

                if (DateTime.Now > GameManager.Activity.GetCurActivityEndTime() || isFinishedAll)
                {
                    void ActivityEnd()
                    {
                        if (!isFinishedAll && PlayerData.GetPickaxeNum() > 0)
                        {
                            GameManager.UI.ShowUIForm("HiddenTempleMainMenu");
                        }
                        else
                        {
                            EndActivity();
                        }
                    }

                    if (GameManager.Process.CurrentProcessName != null)
                    {
                        ProcessType endProcessType = ProcessType.ShowHiddenTempleEndProcess;
                        if (!GameManager.Process.GetProcessInfo(endProcessType.ToString()).IsValid)
                            GameManager.Process.RegisterAfter(GameManager.Process.CurrentProcessName, endProcessType, ActivityEnd);
                        else
                            Log.Warning("{0} already registered", endProcessType.ToString());
                    }
                    else
                    {
                        ActivityEnd();
                    }
                }
            }
        }

        public void EndActivity()
        {
            int periodId = GameManager.Activity.GetCurPeriodID();
            PlayerData.SetCurActivityPeriod(0);
            GameManager.Activity.EndActivity(ActivityID);

            GameManager.Event.Fire(this, HiddenTempleEndEventArgs.Create(periodId));

            GameManager.UI.ShowUIForm("HiddenTempleEndMenu");
        }

        public override void ActivityPreEndProcess()
        {
        }

        public override void ActivityAfterStartProcess()
        {
            //1、当玩家获取锄头时，判断锄头拥有数量>=20个，则弹出活动界面
            //2、最后12小时，每日玩家返回主界面弹出最后机会界面
            //两弹窗流程互斥

            if (!CheckActivityHasStarted() || PlayerData.GetCurrentStage() > PlayerData.GetMaxStage() || !CheckHaveAsset()) 
            {
                return;
            }

            if (!PlayerData.GetTodayShowedLastChanceMenu())
            {
                if (CheckActivityHasStarted())
                {
                    DateTime endTime = GameManager.Activity.GetCurActivityEndTime();
                    if (DateTime.Now < endTime && DateTime.Now >= endTime.AddHours(-12))
                    {
                        PlayerData.SetTodayShowedLastChanceMenu(true);

                        GameManager.Process.RegisterAfter(GameManager.Process.CurrentProcessName, ProcessType.ShowHiddenTempleLastChance, () =>
                        {
                            GameManager.UI.ShowUIForm("HiddenTempleLastChanceMenu");
                        });

                        return;
                    }
                }
            }

            if (GameManager.Scene.SceneChangeType != SceneChangeType.GameToMap)
                return;

            if (PlayerData.GetPickaxeNum() >= PlayerData.CanAutoShowMenuPickaxeNum()
                && GameManager.DataNode.GetData<int>("HiddenTempleNextAutoShowBackTime", 0) <= 0)
            {
                GameManager.DataNode.SetData<int>("HiddenTempleNextAutoShowBackTime", 3);

                GameManager.Process.RegisterAfter(GameManager.Process.CurrentProcessName, ProcessType.ShowHiddenTempleLastChance, () =>
                {
                    GameManager.UI.ShowUIForm("HiddenTempleMainMenu");
                });
            }
            else
            {
                GameManager.DataNode.SetData<int>("HiddenTempleNextAutoShowBackTime", GameManager.DataNode.GetData<int>("HiddenTempleNextAutoShowBackTime", 0) - 1);
            }
        }

        public override void OnLevelWin(int levelFailTime, int hardIndex)
        {
            if (CheckLevelWinCanGetPickaxe())
            {
                int stage = PlayerData.GetPickaxeWinStreakStage();
                if (stage > 0)
                {
                    PlayerData.AddPickaxeLevelCollectNum(stage);

                    GameManager.Firebase.RecordMessageByEvent(AnalyticsEvent.LostTemple_Pickaxes_Level_Get, new Firebase.Analytics.Parameter("Level", GameManager.PlayerData.NowLevel), new Firebase.Analytics.Parameter("Get", stage));
                }

                if (stage < 3)
                {
                    stage++;
                    PlayerData.SetPickaxeWinStreakStage(stage);
                }
            }
        }

        public override void OnLevelLose()
        {
            if (CheckActivityHasStarted())
            {
                PlayerData.ClearPickaxeWinStreakStage();
            }
        }

        private void LoadDataTable<T>(string dataTableAssetName, string dataTableName = null) where T : class, IDataRow, new()
        {
            if (dataTableName == null)
                dataTableName = string.Empty;

            if (DataTable.HasDataTable<T>(dataTableName))
            {
                return;
            }

            m_NeedLoadCount++;
            IDataTable<T> rewardTaskDataTable;
            rewardTaskDataTable = DataTable.CreateDataRowDataTable<T>(dataTableName);
            rewardTaskDataTable.ReadData(dataTableAssetName, this);
        }

        private void OnLoadDataTableSuccess(object sender, GameEventArgs e)
        {
            LoadDataTableSuccessEventArgs ne = (LoadDataTableSuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            m_NeedLoadCount--;
            Log.Info("Load data table '{0}' Success", ne.DataTableAssetName);
        }

        private void OnLoadDataTableFailure(object sender, GameEventArgs e)
        {
            LoadDataTableFailureEventArgs ne = (LoadDataTableFailureEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            m_NeedLoadCount--;
            Log.Error("Can not load data table '{0}' from '{1}' with error message '{2}'", ne.DataTableAssetName, ne.DataTableAssetName, ne.ErrorMessage);
        }

        #region Download

        public DownloadStatus IsDownloadComplete { get; private set; }

        public bool CheckHaveAsset()
        {
            return AddressableUtils.IsHaveAssetSync(AssetName, out long size);
        }

        public void DownloadAsset()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable) 
            {
                IsDownloadComplete = DownloadStatus.Fail;
                return;
            }

            if (GameManager.Download.IsDownloading(AssetName))
            {
                IsDownloadComplete = DownloadStatus.Downloading;
                return;
            }

            GameManager.Download.AddDownload(AssetName);
        }

        private void OnDownloadSuccess(object sender, GameEventArgs e)
        {
            DownloadSuccessEventArgs ne = e as DownloadSuccessEventArgs;
            if (ne != null && ne.DownloadKey == AssetName)
            {
                IsDownloadComplete = DownloadStatus.Success;
            }
        }

        private void OnDownloadFailure(object sender, GameEventArgs e)
        {
            DownloadFailureEventArgs ne = e as DownloadFailureEventArgs;
            if (ne != null && ne.DownloadKey == AssetName)
            {
                IsDownloadComplete = DownloadStatus.Fail;
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
}
