using System.Collections.Generic;

namespace HiddenTemple
{
    /// <summary>
    /// 宝箱奖励数据表行
    /// </summary>
    public class DRChestData : DataRowBase
    {
        private int m_Id;

        /// <summary>
        /// 编号
        /// </summary>
        public override int Id { get { return m_Id; } }

        /// <summary>
        /// 所含奖励的道具编号集合
        /// </summary>
        public List<TotalItemData> RewardsId
        {
            get;
            private set;
        }

        /// <summary>
        /// 所含奖励的道具数量集合
        /// </summary>
        public List<int> RewardsNum
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
            List<TotalItemData> tempList = new List<TotalItemData>();
            for (int i = 0; i < splitedPropsId.Length; i++)
            {
                tempList.Add(TotalItemData.FromInt(int.Parse(splitedPropsId[i])));
            }
            RewardsId = tempList;

            string numString = columnStrings[index++];
            string[] splitedNums = numString.Split('_');
            List<int> temp2List = new List<int>();
            for (int i = 0; i < splitedNums.Length; i++)
            {
                temp2List.Add(int.Parse(splitedNums[i]));
            }
            RewardsNum = temp2List;

            return true;
        }
    }
}
