using GameFramework.Event;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public abstract class MergeActivityBase : MonoBehaviour
    {
        private DRMergeSchedule m_ScheduleData;
        private bool m_IsInitialized = false;
        private int m_NeedLoadCount = 0;
        
        public abstract MergeTheme Theme { get; }
        
        public abstract string GroupName { get; }

        public abstract string AssetName { get; }
        
        [SerializeField]
        private bool IncludeInBuild = true;
        
        public int PeriodId => m_ScheduleData?.Id ?? 0;

        public DateTime StartTime => m_ScheduleData?.StartTime ?? Constant.GameConfig.DateTimeMin;

        public DateTime EndTime => m_ScheduleData?.EndTime ?? Constant.GameConfig.DateTimeMin;

        public int MaxPropId => m_ScheduleData?.MaxPropId ?? 0;

        public int TileId => m_ScheduleData?.TileId ?? 0;

        public int TileRewardId => m_ScheduleData?.TileRewardId ?? 0;

        public bool IsInitialized => m_IsInitialized;

        public virtual int MergeEnergyBoxNum => MergeManager.PlayerData.GetMergeEnergyBoxNum();

        public virtual int BoardRow => 5;

        public virtual int BoardCol => 5;

        public virtual string BgMusicName => "SFX_Merge_Bgm";

        public virtual void Initialize(DRMergeSchedule scheduleData)
        {
            if (m_IsInitialized)
                return;

            if (scheduleData == null || scheduleData.Theme != Theme)
            {
                Log.Error("MergeActivity initialize fail - scheduleData is invalid");
                return;
            }

            m_IsInitialized = true;
            m_ScheduleData = scheduleData;
            m_NeedLoadCount = 0;

            GameManager.Event.Subscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            GameManager.Event.Subscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);
        }

        public virtual void Shutdown()
        {
            if (!m_IsInitialized)
                return;
            m_IsInitialized = false;

            GameManager.Event.Unsubscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            GameManager.Event.Unsubscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);
        }

        public virtual bool CheckInitializationComplete()
        {
            return m_IsInitialized && m_NeedLoadCount <= 0;
        }

        public virtual bool CheckHaveAsset()
        {
            return IncludeInBuild && AddressableUtils.IsHaveAssetSync(AssetName, out long size);
        }

        public virtual bool CheckEntranceCanShow()
        {
            if (!CheckInitializationComplete())
                return false;

            if (!CheckHaveAsset())
                return false;

            if (CheckActivityHasStarted() && DateTime.Now < EndTime)
            {
                return true;
            }
            else
            {
                return GameManager.PlayerData.NowLevel < MergeManager.PlayerData.GetActivityUnlockLevel() && DateTime.Now >= StartTime && DateTime.Now < EndTime;
            }
        }

        public virtual bool CheckActivityCanStart()
        {
            if (GameManager.Network.CheckInternetIsNotReachable())
                return false;

            if (!CheckInitializationComplete())
                return false;

            if (!CheckHaveAsset())
                return false;

            if (CheckActivityHasStarted())
                return false;

            return DateTime.Now >= StartTime && DateTime.Now < EndTime && !CheckActivityIsComplete();
        }

        public virtual bool CheckActivityCanEnd()
        {
            if (GameManager.Network.CheckInternetIsNotReachable())
                return false;

            if (!CheckInitializationComplete())
                return false;

            if (!CheckActivityHasStarted())
                return false;

            return DateTime.Now > EndTime;
        }

        public virtual bool CheckActivityHasStarted()
        {
            return MergeManager.PlayerData.GetSavedPeriod() != 0;
        }

        public virtual bool CheckActivityIsComplete()
        {
            return false;
        }

        public void StartActivity()
        {
            MergeManager.Instance.StartActivity(m_ScheduleData.Id);
        }

        public void EndActivity()
        {
            MergeManager.Instance.EndActivity();
        }

        public virtual void OnEndActivity(List<ItemData> reservedRewardData)
        {
        }

        #region Process

        public virtual void ActivityStartProcess()
        {
            StartActivity();

            GameManager.Process.RegisterAfter(GameManager.Process.CurrentProcessName, ProcessType.ShowMergeStartProcess, () =>
            {
                GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeOfferMenu"), form =>
                {
                    form.m_OnHideCompleteAction = () =>
                    {
                        GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeHowToPlayMenu"), form3 =>
                        {
                            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu"), form2 =>
                            {
                            }, () =>
                            {
                                GameManager.Process.EndProcess(ProcessType.ShowMergeStartProcess);
                            }, true);

                            form3.m_OnHideCompleteAction = () =>
                            {
                                MergeMainMenuBase mainMenu = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
                                if (mainMenu != null) mainMenu.m_GuideMenu.TriggerGuide(GuideTriggerType.Guide_TapBox);
                            };
                        });
                    };
                }, () =>
                {
                    GameManager.Process.EndProcess(ProcessType.ShowMergeStartProcess);
                });
            });
        }

        public virtual void ActivityEndProcess()
        {
            void ActivityPreEndAction()
            {
                if (MergeEnergyBoxNum > 0)
                {
                    GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeLastChanceMenu"));
                }
                else
                {
                    EndActivity();
                }
            }

            if (GameManager.Process.CurrentProcessName != null)
            {
                ProcessType endProcessType = ProcessType.ShowMergeEndProcess;
                if (!GameManager.Process.GetProcessInfo(endProcessType.ToString()).IsValid)
                    GameManager.Process.RegisterAfter(GameManager.Process.CurrentProcessName, endProcessType, ActivityPreEndAction);
                else
                    Log.Warning("Process {0} already registered", endProcessType.ToString());
            }
            else
            {
                ActivityPreEndAction();
            }
        }

        public virtual void ActivityPreEndProcess()
        {
        }

        public virtual void ActivityAfterStartProcess()
        {
            //活动期间，当前所拥有的体力宝箱大于等于30个时，返回home界面时，自动弹一次活动界面
            //弹进活动界面，产生体力消耗，且剩余体力小于30时，需要等到下次满足条件再弹
            //弹进活动界面，未产生体力消耗，则在第三次从game返回home界面时，再自动弹一次

            if (GameManager.Scene.SceneChangeType != SceneChangeType.GameToMap)
                return;

            if (SystemInfoManager.IsSuperLowMemorySize ||
                SystemInfoManager.CheckIsSpecialDeviceCloseLowPriorityPop()) 
                return;
            
            if (CheckActivityHasStarted() && CheckHaveAsset())
            {
                if (GameManager.Network.CheckInternetIsNotReachable())
                    return;

                if (DateTime.Now < EndTime && MergeEnergyBoxNum >= 30)
                {
                    int gameToMapTime = GameManager.DataNode.GetData<int>("MergeGameToMapTime", 0);
                    if (gameToMapTime <= 0)
                    {
                        GameManager.Process.RegisterAfter(GameManager.Process.CurrentProcessName, ProcessType.AutoShowMergeProcess, () =>
                        {
                            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu"));
                        });

                        return;
                    }
                    else
                    {
                        GameManager.DataNode.SetData<int>("MergeGameToMapTime", gameToMapTime - 1);
                    }
                }
            }
        }

        #endregion

        #region Level

        public virtual bool CheckLevelWinCanGetTarget()
        {
            if (!CheckInitializationComplete())
                return false;

            if (GameManager.Network.CheckInternetIsNotReachable() && DateTime.Now >= EndTime)
                return false;

            return CheckActivityHasStarted();
        }

        public abstract bool CheckLevelWinGainedTargetNumAffectedByFirstTry();

        public abstract int GetLevelWinCanGetTargetNum(int levelFailTime, int hardIndex);

        #endregion

        #region Merge

        public virtual bool CheckMergeBoardInteractive()
        {
            return MergeGuideMenu.s_CurGuideId != GuideTriggerType.None && MergeGuideMenu.s_CurGuideId != GuideTriggerType.Guide_DragMerge && MergeGuideMenu.s_CurGuideId != GuideTriggerType.Guide_Web;
        }

        public virtual bool CheckIsCanClickSpecialProp(int propId)
        {
            return propId / 10000 == 3 || propId == 20105 || propId == 40104;
        }

        public virtual Square GetTapBoxGuideTargetSquare()
        {
            return MergeManager.Merge.GetSquare(3, 2);
        }

        public virtual PropAttachmentLogic GetPropAttachmentLogic(string attachmentName)
        {
            switch (attachmentName)
            {
                case "Bubble":
                    return new BubbleLogic();
                case "Web":
                    return new WebLogic();
                case "Packingbox":
                    return new PackingboxLogic();
            }

            return null;
        }

        #endregion

        #region Reward

        public void PerformRewardSettlement(List<ItemData> datas)
        {
            Dictionary<int, int> rewardDic = new Dictionary<int, int>();

            string savedPropDistributedMap = MergeManager.PlayerData.GetSavedPropDistributedMap();
            if (!string.IsNullOrEmpty(savedPropDistributedMap))
            {
                string[] savedPropsString = savedPropDistributedMap.Split('$');
                for (int i = 0; i < savedPropsString.Length; i++)
                {
                    string[] savedPropsSplits = savedPropsString[i].Split('#');
                    int propId = int.Parse(savedPropsSplits[2]);
                    int attachmentId = int.Parse(savedPropsSplits[3]);

                    if (attachmentId == 0)
                    {
                        PropSavedData savedData = null;
                        if(savedPropsSplits.Length > 4)
                        {
                            savedData = GameFramework.ReferencePool.Acquire<PropSavedData>();
                            savedData.Load(savedPropsSplits[4]);
                        }

                        AddReward(propId, savedData, rewardDic);

                        if (savedData != null)
                            GameFramework.ReferencePool.Release(savedData);
                    }
                }
            }

            List<StoredProp> storedPropIds = MergeManager.PlayerData.GetAllStorePropIds();
            if (storedPropIds.Count > 0)
            {
                foreach (var storedProp in storedPropIds)
                {
                    AddReward(storedProp.PropId, storedProp.SavedData, rewardDic);
                }
            }

            foreach (KeyValuePair<int, int> pair in rewardDic)
            {
                datas.Add(new ItemData(TotalItemData.FromInt(pair.Key), pair.Value));
            }

            GameManager.Firebase.RecordMessageByEvent("Merge_Final_Box_Claim_Times", new Firebase.Analytics.Parameter("times", MergeManager.PlayerData.GetFinalRewardTime()));
        }

        protected virtual void AddReward(int rewardId, PropSavedData savedData, Dictionary<int, int> rewardDic)
        {
            IDataTable<DRChestPropReward> dataTable = MergeManager.DataTable.GetDataTable<DRChestPropReward>(MergeManager.Instance.GetMergeDataTableName());

            if (rewardId > 10000)
            {
                int type = rewardId / 10000;
                int rank = rewardId % 100;
                switch (type)
                {
                    case 2://���
                        if (rank != 1 && !rewardDic.ContainsKey(1))
                            rewardDic.Add(1, 0);
                        if (rank == 2)
                            rewardDic[1] += 2;
                        else if (rank == 3)
                            rewardDic[1] += 5;
                        else if (rank == 4)
                            rewardDic[1] += 15;
                        else if (rank == 5)
                            rewardDic[1] += 50;
                        break;
                    case 4://����
                        if (!rewardDic.ContainsKey(7))
                            rewardDic.Add(7, 0);
                        if (rank == 1)
                            rewardDic[7] += 1;
                        else if (rank == 2)
                            rewardDic[7] += 2;
                        else if (rank == 3)
                            rewardDic[7] += 4;
                        else if (rank == 4)
                            rewardDic[7] += 10;
                        break;
                    case 3:
                        DRChestPropReward data = dataTable.GetDataRow(rewardId);
                        if (data != null)
                        {
                            for (int j = 0; j < data.RewardPropIds.Count; j++)
                            {
                                for (int i = 0; i < data.RewardPropNums[j]; i++)
                                {
                                    AddReward(data.RewardPropIds[j], null, rewardDic);
                                }
                            }
                        }
                        break;
                }
            }
            else if(GameManager.DataTable.GetDataTable<DTTotalItemType>().Data.ContainData(rewardId))
            {
                if (rewardDic.ContainsKey(rewardId))
                    rewardDic[rewardId] += 1;
                else
                    rewardDic.Add(rewardId, 1);
            }
        }

        #endregion

        #region DataTable

        protected void LoadDataTable<T>(string dataTableAssetName, string dataTableName = null) where T : class, IDataRow, new()
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

        #if UNITY_EDITOR
        
        public void OnIncludeInBuildChangedInEditor()
        {
            var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("AddressableAssetSettings not found");
                return;
            }
            
            var group = settings.FindGroup(GroupName);
            if (group == null)
            {
                Debug.LogError($"Group not found: {GroupName}");
                return;
            }
            
            var schema = group.GetSchema<UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema>();
            if (schema == null)
            {
                Debug.LogError("BundledAssetGroupSchema missing");
                return;
            }

            schema.IncludeInBuild = IncludeInBuild;

            UnityEditor.EditorUtility.SetDirty(settings);
            UnityEditor.AssetDatabase.SaveAssets();

            Debug.Log($"Group [{GroupName}] IncludeInBuild switched to {IncludeInBuild}");
        }
        
        #endif
    }
}
