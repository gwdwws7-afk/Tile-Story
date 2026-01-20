using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class GiftProp : GeneratorProp
    {
        public int m_Level;

        protected override void InitializeCanGenerateProps()
        {
            IDataTable<DRMerchantReward> rewardDataTable = MergeManager.DataTable.GetDataTable<DRMerchantReward>(MergeManager.Instance.GetMergeDataTableName());
            var data = rewardDataTable.GetDataRow(m_Level);

            if (data != null)
            {
                m_CanGenerateProps = new List<int>();
                for (int i = 0; i < data.ChestReward.Length; i++)
                {
                    int propId = data.ChestReward[i];
                    m_CanGenerateProps.Add(propId);
                }
            }
        }

        public override void OnClick()
        {
            base.OnClick();

            MergeMainMenuBase mainMenu = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            if (mainMenu != null)
            {
                mainMenu.m_GuideMenu.FinishGuide(GuideTriggerType.Guide_TapMaxReward);
            }
        }

        public override void OnBounceEnd()
        {
            base.OnBounceEnd();

            MergeMainMenuBase mainMenu = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            if (mainMenu != null && !MergeGuideMenu.CheckGuideIsComplete(GuideTriggerType.Guide_TapMaxReward))
            {
                mainMenu.m_GuideMenu.TriggerGuide(Merge.GuideTriggerType.Guide_TapMaxReward, PropLogic);
            }
        }
    }
}
