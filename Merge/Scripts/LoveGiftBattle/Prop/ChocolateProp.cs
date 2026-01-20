using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class ChocolateProp : GeneratorProp
    {
        public override void OnGeneratedByMerge()
        {
            base.OnGeneratedByMerge();

            int propId = PropLogic.PropId;
            int stage = propId % 80100 - 1;
            bool isFirstGenerated = false;
            if (stage > MergeManager.PlayerData.GetCurrentMaxMergeStage())
            {
                isFirstGenerated = true;
                MergeManager.PlayerData.SetCurrentMaxMergeStage(stage);
                //GameManager.Event.Fire(this, MaxStageUpgradeEventArgs.Create(propId, PropLogic, transform.position));
            }

            GenerateAdditionalOutput(stage + 1, isFirstGenerated);

            if (propId == 80108)
            {
                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Merge_Get_Final_Box);
            }
        }

        private void GenerateAdditionalOutput(int level, bool isFirstGenerated)
        {
            IDataTable<DRMergeAdditionalOutput> additionDataTable = MergeManager.DataTable.GetDataTable<DRMergeAdditionalOutput>(MergeManager.Instance.GetMergeDataTableName());
            var data = additionDataTable.GetDataRow(level);
            if (data != null)
            {
                if (data.AdditionalPropIds.Count > 0)
                {
                    foreach (int id in data.AdditionalPropIds)
                    {
                        //星星道具只会在首次合成更高级的普通元素时，才会生成一次
                        if (id == 40101 && !isFirstGenerated)
                            continue;

                        Square randomSquare = MergeManager.Merge.GetNearestEmptySquare(PropLogic.Square);
                        if (randomSquare != null)
                        {
                            MergeManager.Merge.GenerateProp(id, 0, transform.position, randomSquare, PropMovementState.Bouncing);
                        }
                        else
                        {
                            MergeMainMenu_LoveGiftBattle mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu_LoveGiftBattle;
                            if (mainBoard != null)
                            {
                                mainBoard.StoreProp(id);

                                mainBoard.ShowShiningFlyEffect(transform.position, mainBoard.m_SupplyButton.transform.position, 0.3f, null, false);
                            }
                        }
                    }
                }
            }
        }
    }
}
