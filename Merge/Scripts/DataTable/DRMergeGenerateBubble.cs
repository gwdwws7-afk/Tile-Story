using System.Collections.Generic;

namespace Merge
{
    /// <summary>
    /// 生成气泡数据表行
    /// </summary>
    public class DRMergeGenerateBubble : DataRowBase
    {
        private int m_Id;

        /// <summary>
        /// 道具编号
        /// </summary>
        public override int Id { get { return m_Id; } }

        /// <summary>
        /// 生成气泡道具编号
        /// </summary>
        public int GenerateBubble
        {
            get;
            private set;
        }

        /// <summary>
        /// 产生气泡概率
        /// </summary>
        public int GenerateBubbleProbability
        {
            get;
            private set;
        }

        /// <summary>
        /// 气泡消耗金币
        /// </summary>
        public int BubbleCostCoinNum
        {
            get;
            private set;
        }

        /// <summary>
        /// 是否可以看广告打破
        /// </summary>
        public bool CanBreakByAds
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

            GenerateBubble = int.Parse(columnStrings[index++]);
            GenerateBubbleProbability = int.Parse(columnStrings[index++]);
            BubbleCostCoinNum = int.Parse(columnStrings[index++]);
            CanBreakByAds = int.Parse(columnStrings[index++]) == 1 ? true : false;

            return true;
        }
    }
}
