using System.Collections.Generic;

namespace HiddenTemple
{
    /// <summary>
    /// 随机关卡数据表行
    /// </summary>
    public class DRLevelData : DataRowBase
    {
        private int m_Id;

        /// <summary>
        /// 寻宝阶段
        /// </summary>
        public override int Id { get { return m_Id; } }

        /// <summary>
        /// 可随机的关卡编号
        /// </summary>
        public List<int> LevelsId
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
            LevelsId = tempList;

            return true;
        }
    }
}
