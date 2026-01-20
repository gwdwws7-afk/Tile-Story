using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class MergeActivity_DigTreasure : MergeActivityBase
    {
        public override MergeTheme Theme => MergeTheme.DigTreasure;
        
        public override string GroupName => "Merge_DigTreasure";

        public override string AssetName => "MergeMainMenu_DigTreasure";

        public override int BoardRow => 8;

        public override int BoardCol => 6;

        public override string BgMusicName => SoundType.SFX_DigTreasure_BGM.ToString();

        public override void Initialize(DRMergeSchedule scheduleData)
        {
            base.Initialize(scheduleData);

            string themeName = Theme.ToString();
            LoadDataTable<DRProp>("PropData_" + themeName, themeName);
            LoadDataTable<DRAttachment>("AttachmentData_" + themeName, themeName);
            LoadDataTable<DRPropMerge>("PropMergeData_" + themeName, themeName);
            LoadDataTable<DRMergeAdditionalOutput>("MergeAdditionalOutput_" + themeName, themeName);
            LoadDataTable<DRMergeGenerateBubble>("MergeGenerateBubble_" + themeName, themeName);
            LoadDataTable<DRMergeOffer>("MergeOfferData_" + themeName, themeName);
            LoadDataTable<DRPropDistributedMap>("PropDistributedMap_" + themeName, themeName);
            LoadDataTable<DRCanGenerateProp>("CanGenerateProp_" + themeName, themeName);
            LoadDataTable<DRMergeFinalChestReward>("MergeFinalChestRewardData_" + themeName, themeName);
            LoadDataTable<DRDigOutput>("DigOutputData_" + themeName, themeName);
            LoadDataTable<DRMergeStageReward>("MergeStageRewardData_" + themeName, themeName);
        }

        public override bool CheckEntranceCanShow()
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

        public override bool CheckLevelWinGainedTargetNumAffectedByFirstTry()
        {
            return true;
        }

        public override int GetLevelWinCanGetTargetNum(int levelFailTime, int hardIndex)
        {
            //首胜获得3个，非首胜获得1个
            int getBoxNum = levelFailTime == 0 ? 3 : 1;

            //超难关【8个】、难关【5个】、简单关【2个】
            //int getBoxNum = 2;
            //if (hardIndex == 1)
            //    getBoxNum = 5;
            //else if (hardIndex == 2)
            //    getBoxNum = 8;

            return getBoxNum;
        }

        public override PropAttachmentLogic GetPropAttachmentLogic(string attachmentName)
        {
            switch (attachmentName)
            {
                case "Bubble":
                    return new BubbleLogic();
                case "Web":
                    return new WebLogic();
                case "Packingbox":
                    return new PackingboxLogic();
                case "Clod":
                    return new ClodLogic();
            }

            return null;
        }

        public override bool CheckMergeBoardInteractive()
        {
            return MergeGuideMenu.s_CurGuideId != GuideTriggerType.None && MergeGuideMenu.s_CurGuideId != GuideTriggerType.Guide_DragMerge && MergeGuideMenu.s_CurGuideId != GuideTriggerType.Guide_Web && MergeGuideMenu.s_CurGuideId != GuideTriggerType.Guide_DigDialog2 &&
                MergeGuideMenu.s_CurGuideId != GuideTriggerType.Guide_DigDialog3;
        }

        public override bool CheckIsCanClickSpecialProp(int propId)
        {
            return (propId == 20105 || propId == 40105 || propId == 10112 || propId == 30101 || propId == 60104 || propId == 70104 || propId == 80104 || propId == 90104 || propId / 10000 == 10 || propId == 120103) && propId != 20101 && propId != 40101;
        }

        protected override void AddReward(int propId, PropSavedData savedData, Dictionary<int, int> rewardDic)
        {
            if (propId > 10000)
            {
                int type = propId / 10000;
                int rank = propId % 100;
                switch (type)
                {
                    case 2://金币
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
                    case 4://星星
                        if (rank != 1 && !rewardDic.ContainsKey(7)) 
                            rewardDic.Add(7, 0);
                        if (rank == 2)
                            rewardDic[7] += 1;
                        else if (rank == 3)
                            rewardDic[7] += 2;
                        else if (rank == 4)
                            rewardDic[7] += 4;
                        else if (rank == 5)
                            rewardDic[7] += 10;
                        break;
                    case 12://无限生命
                        if (!rewardDic.ContainsKey(18))
                            rewardDic.Add(18, 0);
                        rewardDic[18] += 15;
                        break;
                }
            }
            else if (GameManager.DataTable.GetDataTable<DTTotalItemType>().Data.ContainData(propId))
            {
                if (rewardDic.ContainsKey(propId))
                    rewardDic[propId] += 1;
                else
                    rewardDic.Add(propId, 1);
            }

            if (propId == 30101 || (propId > 10000 && propId / 10000 == 10) || propId == 60104)   
            {
                //确认宝箱是否解锁
                if (propId == 30101)
                {
                    if (savedData != null && savedData.HasData("IsLock"))
                    {
                        string savedString = savedData.GetData("IsLock");
                        if (!string.IsNullOrEmpty(savedString) && int.TryParse(savedString, out int result))
                        {
                            if (result != 1)
                                return;
                        }
                    }
                }

                if (savedData != null && savedData.HasData("CanGenerateProps"))
                {
                    string savedString = savedData.GetData("CanGenerateProps");
                    string[] splitedStrings = savedString.Split("+");
                    for (int j = 0; j < splitedStrings.Length; j++)
                    {
                        if (!string.IsNullOrEmpty(splitedStrings[j]) && int.TryParse(splitedStrings[j], out int result))
                        {
                            AddReward(result, null, rewardDic);
                        }
                    }
                }
                else
                {
                    IDataTable<DRCanGenerateProp> rewardDataTable = MergeManager.DataTable.GetDataTable<DRCanGenerateProp>(MergeManager.Instance.GetMergeDataTableName());
                    var data = rewardDataTable.GetDataRow(propId);

                    if (data != null)
                    {
                        for (int i = 0; i < data.CanGeneratePropIds.Count; i++)
                        {
                            int id = data.CanGeneratePropIds[i];
                            for (int j = 0; j < data.CanGeneratePropNum[i]; j++)
                            {
                                AddReward(id, null, rewardDic);
                            }
                        }
                    }
                }
            }

            if (propId == 10112)
            {
                IDataTable<DRMergeFinalChestReward> dt = MergeManager.DataTable.GetDataTable<DRMergeFinalChestReward>(MergeManager.Instance.GetMergeDataTableName());
                DRMergeFinalChestReward data = dt.MaxIdDataRow;

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
            }
        }

        public override void ActivityStartProcess()
        {
            StartActivity();

            MergeManager.PlayerData.ClearPropIsUnlock();

            GameManager.Process.RegisterAfter(GameManager.Process.CurrentProcessName, ProcessType.ShowMergeStartProcess, () =>
            {
                GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeStartMenu"), form =>
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
                                MergeMainMenu_DigTreasure mainMenu = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu_DigTreasure;
                                mainMenu.m_GuideMenu.TriggerGuide(GuideTriggerType.Guide_DigDialog1);
                            };
                        });
                    };

                    GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.DigTreasure_Merge_Layer_Unlock, new Firebase.Analytics.Parameter("Stage", 8));

                }, () =>
                {
                    GameManager.Process.EndProcess(ProcessType.ShowMergeStartProcess);
                });
            });
        }

        public override void ActivityEndProcess()
        {
            void ActivityPreEndAction()
            {
                GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeEndMenu"));
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

        public override void OnEndActivity(List<ItemData> reservedRewardData)
        {
            base.OnEndActivity(reservedRewardData);

            if (CheckHaveAsset())
            {
                if (reservedRewardData.Count > 0)
                {
                    GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeReservedRewardMenu"), showFailAction: () =>
                    {
                        GameManager.Process.EndProcess(ProcessType.ShowMergeEndProcess);
                    }, userData: new List<ItemData>(reservedRewardData));
                }
                else
                {
                    GameManager.Process.EndProcess(ProcessType.ShowMergeEndProcess);
                }
            }
            else
            {
                GameManager.Process.EndProcess(ProcessType.ShowMergeEndProcess);
            }
        }
    }
}
