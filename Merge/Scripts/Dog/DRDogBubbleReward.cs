using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class DRDogBubbleReward : DataRowBase
    {
        private int m_Id;

        /// <summary>
        /// 奖励编号
        /// </summary>
        public override int Id { get { return m_Id; } }

        /// <summary>
        /// 奖励类型
        /// </summary>
        public int RewardId
        {
            get;
            private set;
        }

        /// <summary>
        /// 奖励数量
        /// </summary>
        public int RewardNum
        {
            get;
            private set;
        }

        /// <summary>
        /// 随机权重
        /// </summary>
        public int WeightRandom
        {
            get;
            private set;
        }

        /// <summary>
        /// 冷却时间（分钟）
        /// </summary>
        public int CD
        {
            get;
            private set;
        }

        /// <summary>
        /// 是否可看RV
        /// </summary>
        public bool EnableAd
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
            RewardId = int.Parse(columnStrings[index++]);
            RewardNum = int.Parse(columnStrings[index++]);
            WeightRandom = int.Parse(columnStrings[index++]);
            CD = int.Parse(columnStrings[index++]);
            EnableAd = int.Parse(columnStrings[index++]) == 1 ? true : false;
            return true;
        }
    }
}
