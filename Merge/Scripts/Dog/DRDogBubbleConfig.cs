using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    /// <summary>
    /// 圣诞树气泡配置表行
    /// </summary>
    public class DRDogBubbleConfig : DataRowBase
    {
        private int m_Id;

        /// <summary>
        /// 层级编号
        /// </summary>
        public override int Id { get { return m_Id; } }

        /// <summary>
        /// 金币数量
        /// </summary>
        public int CoinNum
        {
            get;
            private set;
        }

        /// <summary>
        /// 封顶数量
        /// </summary>
        public int MaxBubbleNum
        {
            get;
            private set;
        }

        /// <summary>
        /// 单次增长：个数
        /// </summary>
        public int GenerateNumPerTime
        {
            get;
            private set;
        }

        /// <summary>
        /// 增长频次：分钟
        /// </summary>
        public int GenerateBubbleMinutes
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
            CoinNum = int.Parse(columnStrings[index++]);
            MaxBubbleNum = int.Parse(columnStrings[index++]);
            GenerateNumPerTime = int.Parse(columnStrings[index++]);
            GenerateBubbleMinutes = int.Parse(columnStrings[index++]);
            return true;
        }
    }
}
