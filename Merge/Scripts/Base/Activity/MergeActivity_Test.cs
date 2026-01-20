using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class MergeActivity_Test : MergeActivityBase
    {
        public override MergeTheme Theme => MergeTheme.Test;

        public override string GroupName => "Merge_Test";

        public override string AssetName => "MergeMainMenu_Test";

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
        }

        public override bool CheckLevelWinGainedTargetNumAffectedByFirstTry()
        {
            return true;
        }

        public override int GetLevelWinCanGetTargetNum(int levelFailTime, int hardIndex)
        {
            int getBoxNum = levelFailTime == 0 ? 3 : 1;
            return getBoxNum;
        }
    }
}
