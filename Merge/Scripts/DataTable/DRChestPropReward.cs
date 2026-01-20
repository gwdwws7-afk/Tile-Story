
using System.Collections.Generic;

namespace Merge
{
    /// <summary>
    /// 宝箱道具内奖励数据表行
    /// </summary>
    public class DRChestPropReward : DataRowBase
    {
        private int m_Id;

        /// <summary>
        /// 合成道具等级
        /// </summary>
        public override int Id { get { return m_Id; } }

        /// <summary>
        /// 所含奖励的道具编号集合
        /// </summary>
        public List<int> RewardPropIds
        {
            get;
            private set;
        }

        /// <summary>
        /// 所含奖励的道具数量集合
        /// </summary>
        public List<int> RewardPropNums
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

            string idString = columnStrings[index++];
            string[] splitedPropsId = idString.Split('_');
            List<int> tempList = new List<int>();
            for (int i = 0; i < splitedPropsId.Length; i++)
            {
                tempList.Add(int.Parse(splitedPropsId[i]));
            }
            RewardPropIds = tempList;

            string numString = columnStrings[index++];
            string[] splitedNums = numString.Split('_');
            List<int> temp2List = new List<int>();
            for (int i = 0; i < splitedNums.Length; i++)
            {
                temp2List.Add(int.Parse(splitedNums[i]));
            }
            RewardPropNums = temp2List;

            return true;
        }
    }
}
