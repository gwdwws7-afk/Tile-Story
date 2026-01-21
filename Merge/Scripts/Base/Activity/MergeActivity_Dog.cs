using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class MergeActivity_Dog : MergeActivityBase
    {
        public override MergeTheme Theme => MergeTheme.Dog;

        public override string GroupName => "Merge_Dog";

        public override string AssetName => "MergeMainMenu_Dog";

        public override string BgMusicName => "SFX_Merge_Xmas_Bgm";

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
            LoadDataTable<DRDogBubbleReward>("DogBubbleReward");
            LoadDataTable<DRDogBubbleConfig>("DogBubbleConfig");
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
                        GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeHowToPlayMenu"),
                            form3 =>
                            {
                                GameManager.UI.ShowUIForm(
                                    MergeManager.Instance.GetMergeMenuName("MergeMainMenu"), form2 =>
                                    {
                                    }, () =>
                                    {
                                        GameManager.Process.EndProcess(ProcessType.ShowMergeStartProcess);
                                    }, true);

                                form3.m_OnHideCompleteAction = () =>
                                {
                                    MergeMainMenuBase mainMenu =
                                        GameManager.UI.GetUIForm(
                                                MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as
                                            MergeMainMenuBase;
                                    mainMenu.m_GuideMenu.TriggerGuide(GuideTriggerType.Guide_TapBox);
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
            if (CheckHaveAsset())
            {
                if (reservedRewardData.Count > 0)
                {
                    GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeEndMenu"), showFailAction: () =>
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

        protected override void AddReward(int propId, PropSavedData savedData, Dictionary<int, int> rewardDic)
        {
            base.AddReward(propId, savedData, rewardDic);

            if (propId == MergeManager.Instance.MaxPropId)
            {
                IDataTable<DRMergeFinalChestReward> dt = MergeManager.DataTable.GetDataTable<DRMergeFinalChestReward>(MergeManager.Instance.GetMergeDataTableName());
                int time = MergeManager.PlayerData.GetFinalRewardTime();
                DRMergeFinalChestReward data = dt.GetDataRow(time + 1);
                if (data == null)
                    data = dt.MaxIdDataRow;
                MergeManager.PlayerData.AddGetFinalRewardTime();

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

        public override PropAttachmentLogic GetPropAttachmentLogic(string attachmentName)
        {
            switch (attachmentName)
            {
                case "Bubble":
                    return new BubbleLogic();
                case "Web":
                    return new WebLogic();
                case "Packingbox":
                    return new PackingboxLogic_Dog();
            }

            return null;
        }
    }
}
