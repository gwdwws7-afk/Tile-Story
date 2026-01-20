using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class MergeActivity_LoveGiftBattle : MergeActivityBase
    {
        public override MergeTheme Theme => MergeTheme.LoveGiftBattle;
        
        public override string GroupName => "Merge_LoveGiftBattle";

        public override string AssetName => "MergeMainMenu_LoveGiftBattle";

        public override int BoardRow => 6;

        public override int BoardCol => 7;

        public override string BgMusicName => "SFX_Pinata_Bgm";

        public override void Initialize(DRMergeSchedule scheduleData)
        {
            base.Initialize(scheduleData);

            string themeName = Theme.ToString();
            LoadDataTable<DRProp>("PropData_" + themeName, themeName);
            LoadDataTable<DRAttachment>("AttachmentData_" + themeName, themeName);
            LoadDataTable<DRPropMerge>("PropMergeData_" + themeName, themeName);
            LoadDataTable<DRMergeAdditionalOutput>("MergeAdditionalOutput_" + themeName, themeName);
            LoadDataTable<DRMergeGenerateBubble>("MergeGenerateBubble_" + themeName, themeName);
            LoadDataTable<DRChestPropReward>("ChestPropRewardData_" + themeName, themeName);
            LoadDataTable<DRGeneratePropWeights>("GeneratePropWeights_" + themeName, themeName);
            LoadDataTable<DRMergeFinalChestReward>("MergeFinalChestRewardData_" + themeName, themeName);
            LoadDataTable<DRMergeOffer>("MergeOfferData_" + themeName, themeName);
            LoadDataTable<DRPropDistributedMap>("PropDistributedMap_" + themeName, themeName);
            LoadDataTable<DRMerchantInventory>("MerchantInventoryData_" + themeName, themeName);
            LoadDataTable<DRMerchantReward>("MerchantRewardData_" + themeName, themeName);
            LoadDataTable<DRCanGenerateProp>("CanGenerateProp_" + themeName, themeName);
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
            return false;
        }

        public override int GetLevelWinCanGetTargetNum(int levelFailTime, int hardIndex)
        {
            //首胜获得3个，非首胜获得1个
            //int getBoxNum = levelFailTime == 0 ? 3 : 1;

            //超难关【8个】、难关【5个】、简单关【2个】
            int getBoxNum = 2;
            if (hardIndex == 1)
                getBoxNum = 5;
            else if (hardIndex == 2)
                getBoxNum = 8;

            return getBoxNum;
        }

        public override bool CheckActivityIsComplete()
        {
            return MergeManager.PlayerData.GetLoveGiftRewardStage() > 7;
        }

        public override bool CheckLevelWinCanGetTarget()
        {
            return base.CheckLevelWinCanGetTarget() && !CheckActivityIsComplete();
        }

        public override bool CheckActivityCanEnd()
        {
            if (GameManager.Network.CheckInternetIsNotReachable())
                return false;

            if (!CheckInitializationComplete())
                return false;

            if (!CheckActivityHasStarted())
                return false;

            return DateTime.Now > EndTime || CheckActivityIsComplete();
        }

        public override bool CheckMergeBoardInteractive()
        {
            return MergeGuideMenu.s_CurGuideId != GuideTriggerType.None && MergeGuideMenu.s_CurGuideId != GuideTriggerType.Guide_DragMerge && MergeGuideMenu.s_CurGuideId != GuideTriggerType.Guide_Web && MergeGuideMenu.s_CurGuideId != GuideTriggerType.Guide_MergeBalloon;
        }

        public override bool CheckIsCanClickSpecialProp(int id)
        {
            return (id / 10000 == 2 || id / 10000 == 3 || id / 10000 == 4 || id / 10000 == 9 || id == 10104 || id == 50105 || id == 60105 || id == 70106 || id == 80108) && id != 20101 && id != 40101;
        }

        public override Square GetTapBoxGuideTargetSquare()
        {
            return MergeManager.Merge.GetSquare(3, 3);
        }

        public override PropAttachmentLogic GetPropAttachmentLogic(string attachmentName)
        {
            switch (attachmentName)
            {
                case "Bubble":
                    return new BubbleLogic();
                case "Web":
                    return new WebLogic_LoveGiftBattle();
                case "Packingbox":
                    return new PackingboxLogic_LoveGiftBattle();
            }

            return null;
        }

        public override void ActivityStartProcess()
        {
            StartActivity();

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
                                MergeMainMenu_LoveGiftBattle mainForm = form2 as MergeMainMenu_LoveGiftBattle;
                                mainForm.m_ThiefBoard.m_ThiefSpine.gameObject.SetActive(false);
                            }, () =>
                            {
                                GameManager.Process.EndProcess(ProcessType.ShowMergeStartProcess);
                            }, true);

                            form3.m_OnHideCompleteAction = () =>
                            {
                                MergeMainMenu_LoveGiftBattle mainMenu = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu_LoveGiftBattle;
                                mainMenu.m_ThiefBoard.ShowThiefFirstEnterAnim();
                            };
                        });
                    };
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
                if (MergeEnergyBoxNum > 0)
                {
                    GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeLastChanceMenu"));
                }
                else
                {
                    GameManager.UI.ShowUIForm("MergeEndMenu_Fail");
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

        protected override void AddReward(int propId, PropSavedData savedData, Dictionary<int, int> rewardDic)
        {
            IDataTable<DRChestPropReward> dataTable = MergeManager.DataTable.GetDataTable<DRChestPropReward>(MergeManager.Instance.GetMergeDataTableName());

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
                    case 3:
                        DRChestPropReward data = dataTable.GetDataRow(propId);
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
            else if (GameManager.DataTable.GetDataTable<DTTotalItemType>().Data.ContainData(propId))
            {
                if (rewardDic.ContainsKey(propId))
                    rewardDic[propId] += 1;
                else
                    rewardDic.Add(propId, 1);
            }

            if (propId == 60105 || propId == 70106 || propId == 80108)
            {
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
