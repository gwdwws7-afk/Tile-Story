using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class DRMerchantReward : DataRowBase
    {
        private int m_Id;

        /// <summary>
        /// Merchant level
        /// </summary>
        public override int Id { get { return m_Id; } }

        /// <summary>
        /// Love gift reward array
        /// </summary>
        public int[] ChestReward
        {
            get;
            private set;
        }

        /// <summary>
        /// Unit reward array
        /// </summary>
        public int[] UnitReward
        {
            get;
            private set;
        }

        public override bool ParseDataRow(string dataRowString)
        {
            string[] columnStrings = dataRowString.Split(DataTableExtension.DataSplitSeparators);
            for (int i = 0; i < columnStrings.Length; i++)
            {
                columnStrings[i] = columnStrings[i].Trim(DataTableExtension.DataTrimSeparators);
            }

            int index = 0;
            index++;
            m_Id = int.Parse(columnStrings[index++]);
            index++;

            string[] chestIdString = columnStrings[index++].Split('_');
            var ChestRewardId = new int[chestIdString.Length];
            for (int i = 0; i < chestIdString.Length; i++)
            {
                ChestRewardId[i] = int.Parse(chestIdString[i]);
            }

            int chestTotalNum = 0;
            string[] chestNumString = columnStrings[index++].Split('_');
            var ChestRewardNum = new int[chestNumString.Length];
            for (int i = 0; i < chestNumString.Length; i++)
            {
                ChestRewardNum[i] = int.Parse(chestNumString[i]);
                chestTotalNum += ChestRewardNum[i];
            }

            ChestReward = new int[chestTotalNum];
            int chestIndex = 0;
            for (int i = 0; i < ChestRewardId.Length; i++)
            {
                for (int j = 0; j < ChestRewardNum[i]; j++)
                {
                    ChestReward[chestIndex + j] = ChestRewardId[i];
                }

                chestIndex += ChestRewardNum[i];
            }


            string[] unitIdString = columnStrings[index++].Split('_');
            var UnitRewardId = new int[unitIdString.Length];
            for (int i = 0; i < unitIdString.Length; i++)
            {
                UnitRewardId[i] = int.Parse(unitIdString[i]);
            }

            int unitTotalNum = 0;
            string[] unitNumString = columnStrings[index++].Split('_');
            var UnitRewardNum = new int[unitNumString.Length];
            for (int i = 0; i < unitNumString.Length; i++)
            {
                UnitRewardNum[i] = int.Parse(unitNumString[i]);
                unitTotalNum += UnitRewardNum[i];
            }

            UnitReward = new int[unitTotalNum];
            int unitIndex = 0;
            for (int i = 0; i < UnitRewardId.Length; i++)
            {
                for (int j = 0; j < UnitRewardNum[i]; j++)
                {
                    UnitReward[unitIndex + j] = UnitRewardId[i];
                }

                unitIndex += UnitRewardNum[i];
            }

            return true;
        }
    }
}
