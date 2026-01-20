using GameFramework.Event;
using MySelf.Model;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    /// <summary>
    /// 合成游戏管理器
    /// </summary>
    public sealed class MergeManager : ActivityManagerBase
    {
        public static MergeManager Instance { get; private set; }

        public static MergeComponent Merge { get; private set; }
        public static DataTableComponent DataTable { get; private set; }
        public static PlayerDataComponent PlayerData { get; private set; }

        public override int ActivityID => 100002;

        [SerializeField]
        private List<MergeActivityBase> m_MergeActivityList = new List<MergeActivityBase>();
        private MergeActivityBase m_CurMergeActivity;
        private int m_NeedLoadCount = 0;

        public MergeTheme Theme => m_CurMergeActivity != null ? m_CurMergeActivity.Theme : MergeTheme.None;

        public DateTime StartTime => m_CurMergeActivity != null ? m_CurMergeActivity.StartTime : Constant.GameConfig.DateTimeMin;

        public DateTime EndTime => m_CurMergeActivity != null ? m_CurMergeActivity.EndTime : Constant.GameConfig.DateTimeMin;

        public int MaxPropId => m_CurMergeActivity != null ? m_CurMergeActivity.MaxPropId : 0;

        public int TileId => m_CurMergeActivity != null ? m_CurMergeActivity.TileId : 0;

        public int TileRewardId => m_CurMergeActivity != null ? m_CurMergeActivity.TileRewardId : 0;

        public int BoardRow => m_CurMergeActivity != null ? m_CurMergeActivity.BoardRow : 0;

        public int BoardCol => m_CurMergeActivity != null ? m_CurMergeActivity.BoardCol : 0;

        public string BgMusicName => m_CurMergeActivity != null ? m_CurMergeActivity.BgMusicName : "SFX_Merge_Bgm";

        #region UNITY_EDITOR
        
        private void OnValidate()
        {
            m_MergeActivityList.Clear();
            GetComponentsInChildren<MergeActivityBase>(true, m_MergeActivityList);
        }
        
        #endregion

        private void Awake()
        {
            Instance = this;

            Merge = GetComponentInChildren<MergeComponent>();
            DataTable = GetComponentInChildren<DataTableComponent>();
            PlayerData = GetComponentInChildren<PlayerDataComponent>();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            GameManager.Event.Subscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            GameManager.Event.Subscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);

            LoadDataTable<DRMergeSchedule>("MergeScheduleData");
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();

            GameManager.Event.Unsubscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            GameManager.Event.Unsubscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);

            m_CurMergeActivity = null;

            foreach (var activity in m_MergeActivityList)
            {
                activity.Shutdown();
            }
        }

        public override bool CheckInitializationComplete()
        {
            foreach (var activity in m_MergeActivityList)
            {
                if (activity.IsInitialized && !activity.CheckInitializationComplete()) 
                    return false;
            }

            return m_NeedLoadCount <= 0 && base.CheckInitializationComplete();
        }

        private void InitializeActivity()
        {
            IDataTable<DRMergeSchedule> dataTable = DataTable.GetDataTable<DRMergeSchedule>();
            int curPeriodId = PlayerData.GetSavedPeriod();
            DRMergeSchedule mergeSchedule = dataTable.GetDataRow(curPeriodId);

            if (mergeSchedule == null)
            {
                DRMergeSchedule[] allMergeSchedule = dataTable.GetAllDataRows();
                foreach (var schedule in allMergeSchedule)
                {
                    if (DateTime.Now >= schedule.StartTime && DateTime.Now < schedule.EndTime)
                    {
                        mergeSchedule = schedule;
                        break;
                    }
                }
            }

            if (mergeSchedule != null)
            {
                foreach (var activity in m_MergeActivityList)
                {
                    if (activity.Theme == mergeSchedule.Theme)
                    {
                        activity.Initialize(mergeSchedule);
                        m_CurMergeActivity = activity;
                        break;
                    }
                }
            }
        }

        public void StartActivity(int periodId)
        {
            PlayerData.ClearAllData();
            PlayerData.SetSavedPeriod(periodId);
            PlayerData.AddMergeEnergyBoxNum(10);

            GameManager.Event.Fire(this, MergeStartEventArgs.Create(periodId));
        }

        public void EndActivity()
        {
            PlayerData.SetSavedPeriod(0);

            if (m_CurMergeActivity != null)
            {
                List<ItemData> rewardDatas = PerformRewardSettlement();

                int periodId = m_CurMergeActivity.PeriodId;
                m_CurMergeActivity.OnEndActivity(rewardDatas);
                m_CurMergeActivity.Shutdown();
                m_CurMergeActivity = null;
                GameManager.Event.Fire(this, MergeEndEventArgs.Create(periodId));

                InitializeActivity();
            }
            else
            {
                Log.Error("CurMergeActivity is null");
            }
        }

        /// <summary>
        /// 确认入口是否可以显示
        /// </summary>
        public bool CheckEntranceCanShow()
        {
            if (GameManager.PlayerData.NowLevel < PlayerData.GetActivityPreviewLevel())
                return false;

            if (m_CurMergeActivity == null) 
                return false;

            return m_CurMergeActivity.CheckEntranceCanShow();
        }

        /// <summary>
        /// 确认活动是否可以开启
        /// </summary>
        public override bool CheckActivityCanStart()
        {
            if (GameManager.PlayerData.NowLevel < PlayerData.GetActivityUnlockLevel())
                return false;

            if (m_CurMergeActivity == null)
                return false;

            return m_CurMergeActivity.CheckActivityCanStart();
        }

        /// <summary>
        /// 确认活动是否已经开启
        /// </summary>
        public override bool CheckActivityHasStarted()
        {
            if (m_CurMergeActivity == null)
                return false;

            return m_CurMergeActivity.CheckActivityHasStarted();
        }

        /// <summary>
        /// 获取活动结束时间
        /// </summary>
        public DateTime GetActivityEndTime()
        {
            if (m_CurMergeActivity != null)
                return m_CurMergeActivity.EndTime;
            else
                return Constant.GameConfig.DateTimeMin;
        }

        /// <summary>
        /// 确认关卡胜利是否可以获取合成能量
        /// </summary>
        public bool CheckLevelWinCanGetTarget()
        {
            if (m_CurMergeActivity == null)
                return false;

            return m_CurMergeActivity.CheckLevelWinCanGetTarget();
        }

        public bool CheckLevelWinGainedTargetNumAffectedByFirstTry()
        {
            if (m_CurMergeActivity == null)
                return false;

            return m_CurMergeActivity.CheckLevelWinGainedTargetNumAffectedByFirstTry();
        }

        public int GetLevelWinCanGetTargetNum(int levelFailTime, int hardIndex)
        {
            if (m_CurMergeActivity == null)
                return 0;

            return m_CurMergeActivity.GetLevelWinCanGetTargetNum(levelFailTime, hardIndex);
        }

        public bool CheckActivityIsComplete()
        {
            if (m_CurMergeActivity == null)
                return false;

            return m_CurMergeActivity.CheckActivityIsComplete();
        }

        public override void ActivityStartProcess()
        {
            if (m_CurMergeActivity == null)
                InitializeActivity();

            if (m_CurMergeActivity != null && CheckActivityCanStart()) 
            {
                m_CurMergeActivity.ActivityStartProcess();
            }
        }

        public override void ActivityEndProcess()
        {
            if (m_CurMergeActivity != null && m_CurMergeActivity.CheckActivityCanEnd()) 
            {
                //如果资源已经不存在，不弹出结束界面，直接关闭活动
                if (!m_CurMergeActivity.CheckHaveAsset())
                {
                    EndActivity();

                    return;
                }

                m_CurMergeActivity.ActivityEndProcess();
            }
        }

        public override void ActivityPreEndProcess()
        {
            if (m_CurMergeActivity != null)
            {
                m_CurMergeActivity.ActivityPreEndProcess();
            }
        }

        public override void ActivityAfterStartProcess()
        {
            if (m_CurMergeActivity != null)
            {
                m_CurMergeActivity.ActivityAfterStartProcess();
            }
        }

        public override void OnLevelWin(int levelFailTime, int hardIndex)
        {
            if (CheckLevelWinCanGetTarget())
            {
                int getBoxNum = GetLevelWinCanGetTargetNum(levelFailTime, hardIndex);

                PlayerData.AddMergeEnergyBoxLevelCollectNum(getBoxNum);
            }
        }

        public override void OnLevelLose()
        {
        }

        public string GetMergeEnergyBoxName()
        {
            return Theme == MergeTheme.None ? "MergeEnergyBox" : "MergeEnergyBox_" + Theme.ToString();
        }

        public string GetMergeMenuName(string menuName)
        {
            return Theme == MergeTheme.None ? menuName : menuName + "_" + Theme.ToString();
        }

        public string GetMergeAtlasName(string atlasName)
        {
            return Theme == MergeTheme.None ? atlasName : atlasName + "_" + Theme.ToString();
        }

        public string GetPropAssetName(string assetName)
        {
            return Theme == MergeTheme.None ? assetName : assetName + "_" + Theme.ToString();
        }

        public string GetMergeDataTableName()
        {
            return Theme == MergeTheme.None ? string.Empty : Theme.ToString();
        }

        public string GetMergeOfferColumnName(string columnName)
        {
            return Theme == MergeTheme.None ? columnName : columnName + "_" + Theme.ToString();
        }

        public string GetMergeLoseTextName()
        {
            if (Theme == MergeTheme.LoveGiftBattle || Theme == MergeTheme.DigTreasure)
                return "Merge.Lose_" + Theme.ToString();
            else
                return "Merge.Lose";
        }

        public string GetMergeGuideName(string guideName)
        {
            if (Theme == MergeTheme.LoveGiftBattle || Theme == MergeTheme.DigTreasure)
                return guideName + Theme.ToString();
            else
                return guideName;
        }

        public PropAttachmentLogic GetPropAttachmentLogic(string attachmentName)
        {
            if (m_CurMergeActivity != null)
            {
                return m_CurMergeActivity.GetPropAttachmentLogic(attachmentName);
            }

            return null;
        }

        #region Merge

        public bool CheckMergeBoardInteractive()
        {
            return m_CurMergeActivity != null ? m_CurMergeActivity.CheckMergeBoardInteractive() : true;
        }

        public bool CheckIsCanClickSpecialProp(int propId)
        {
            return m_CurMergeActivity != null ? m_CurMergeActivity.CheckIsCanClickSpecialProp(propId) : false;
        }

        public Square GetTapBoxGuideTargetSquare()
        {
            return m_CurMergeActivity != null ? m_CurMergeActivity.GetTapBoxGuideTargetSquare() : null;
        }

        #endregion

        #region Christmas

        private IDataTable<DRChristmasBubbleConfig> m_ChristmasBubbleConfig;
        public IDataTable<DRChristmasBubbleConfig> ChristmasBubbleConfig
        {
            get
            {
                if (m_ChristmasBubbleConfig == null)
                {
                    m_ChristmasBubbleConfig = DataTable.GetDataTable<DRChristmasBubbleConfig>();
                }

                return m_ChristmasBubbleConfig;
            }
        }

        private IDataTable<DRChristmasBubbleReward> m_ChristmasBubbleReward;
        public IDataTable<DRChristmasBubbleReward> ChristmasBubbleReward
        {
            get
            {
                if (m_ChristmasBubbleReward == null)
                {
                    m_ChristmasBubbleReward = DataTable.GetDataTable<DRChristmasBubbleReward>();
                }

                return m_ChristmasBubbleReward;
            }
        }

        public int GetRandomBubbleRewardId()
        {
            DRChristmasBubbleReward[] allDatas = ChristmasBubbleReward.GetAllDataRows();
            int totalWeight = 0;
            foreach (var data in allDatas)
            {
                totalWeight += data.WeightRandom;
            }

            int randomWeight = UnityEngine.Random.Range(1, totalWeight + 1);
            foreach (var data in allDatas)
            {
                randomWeight -= data.WeightRandom;
                if (randomWeight <= 0)
                {
                    return data.Id;
                }
            }

            return 1;
        }

        public DRChristmasBubbleReward GetBubbleRewardData(int id)
        {
            return ChristmasBubbleReward.GetDataRow(id);
        }

        public int GetMaxBubbleNum()
        {
            if (ChristmasBubbleConfig != null)
            {
                DRChristmasBubbleConfig config = ChristmasBubbleConfig.GetDataRow(PlayerData.GetChristmasDecorationStage());
                return config != null ? config.MaxBubbleNum : 0;
            }

            return 0;
        }

        public int GetGenerateBubbleNumPerTime()
        {
            if (ChristmasBubbleConfig != null)
            {
                DRChristmasBubbleConfig config = ChristmasBubbleConfig.GetDataRow(PlayerData.GetChristmasDecorationStage());
                return config != null ? config.GenerateNumPerTime : 0;
            }

            return 1;
        }

        public int GetGenerateBubbleMinutes()
        {
            if (ChristmasBubbleConfig != null)
            {
                DRChristmasBubbleConfig config = ChristmasBubbleConfig.GetDataRow(PlayerData.GetChristmasDecorationStage());
                return config != null ? config.GenerateBubbleMinutes : 0;
            }

            return 5;
        }

        #endregion
        
        #region Reward

        private List<ItemData> PerformRewardSettlement()
        {
            List<ItemData> rewardDatas = new List<ItemData>();
            m_CurMergeActivity.PerformRewardSettlement(rewardDatas);

            return rewardDatas;
        }

        #endregion

        #region DataTable

        private void LoadDataTable<T>(string dataTableAssetName, string dataTableName = null) where T : class, IDataRow, new()
        {
            if (dataTableName == null)
                dataTableName = string.Empty;

            if (MergeManager.DataTable.HasDataTable<T>(dataTableName))
            {
                return;
            }

            m_NeedLoadCount++;
            IDataTable<T> rewardTaskDataTable;
            rewardTaskDataTable = MergeManager.DataTable.CreateDataRowDataTable<T>(dataTableName);
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

            if (m_CurMergeActivity == null) 
                InitializeActivity();
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

        #endregion

        #region Notification

        private DateTime activityStartTime => StartTime/*GameManager.Activity.GetCurActivityStartTime()*/;
        private DateTime activityEndTime => EndTime/*GameManager.Activity.GetCurActivityEndTime()*/;
        private int unlockLevel => PlayerData.GetActivityUnlockLevel();

        public void RecordEnterActivity()
        {
            if (!MergeModel.Instance.Data.IsHaveEnterActivity)
            {
                MergeModel.Instance.Data.IsHaveEnterActivity = true;
                MergeModel.Instance.SaveToLocal();
            }
        }

        public void SendNotificationByMerge()
        {
            if (!GameManager.Notification.IsInit)
                return;
            
            //活动期间、活动开启、没有进入活动
            if (GameManager.PlayerData.NowLevel >= unlockLevel
                && DateTime.Now < activityEndTime
                && !MergeModel.Instance.Data.IsHaveEnterActivity)
            {
                //设置活动期间每天都推送 第一条
                //当天早上八点
                DateTime activeStart4Pm = new DateTime(activityStartTime.Year, activityStartTime.Month, activityStartTime.Day, 16, 0, 0);

                int index = 0;
                while (activeStart4Pm < activityEndTime)
                {
                    if (activeStart4Pm > DateTime.Now)
                    {
                        GameManager.Notification.SendNotification(NotificationKey.Merge_1, activeStart4Pm);
                        index++;
                    }

                    activeStart4Pm = activeStart4Pm.AddDays(1);
                }
                if (index > 0) return;
            }

            //参与的玩家，活动倒数1、2、3天 每天早上八点推送 文二
            //参与活动第二天到倒数第四条  每天找事八点推送文 三
            //直接添加
            if (CheckActivityHasStarted()
                && MergeModel.Instance.Data.IsHaveEnterActivity)
            {
                //设置 每天都推送
                DateTime activeStart4Pm = new DateTime(activityStartTime.Year, activityStartTime.Month, activityStartTime.Day, 16, 0, 0);
                DateTime activeEnd4Pm = new DateTime(activityEndTime.Year, activityEndTime.Month, activityEndTime.Day, 16, 0, 0);

                int lastDays = 3;
                while (lastDays > 0)
                {
                    GameManager.Notification.SendNotification(NotificationKey.Merge_2, activeEnd4Pm.AddDays(-lastDays));
                    lastDays--;
                }

                var lastThreeDateTime = activeEnd4Pm.AddDays(-3);
                while (activeStart4Pm < lastThreeDateTime)
                {
                    GameManager.Notification.SendNotification(NotificationKey.Merge_3, activeStart4Pm);
                    activeStart4Pm = activeStart4Pm.AddDays(1);
                }
            }
        }
        #endregion
    }
}
