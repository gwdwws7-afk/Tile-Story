using System.Collections.Generic;

namespace Merge
{
    /// <summary>
    /// 道具合成数据表行
    /// </summary>
    public class DRPropMerge : DataRowBase
    {
        private int m_Id;
        private string m_RouteName;
        private List<int> m_MergeRoute = new List<int>();

        /// <summary>
        /// 合成类型编号
        /// </summary>
        public override int Id { get { return m_Id; } }

        /// <summary>
        /// 道具合成路线
        /// </summary>
        public List<int> MergeRoute { get { return m_MergeRoute; } }

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

            m_MergeRoute.Clear();
            while (index < columnStrings.Length)
            {
                string columnString = columnStrings[index++];
                if (!string.IsNullOrEmpty(columnString))
                {
                    m_MergeRoute.Add(int.Parse(columnString));
                }
            }
            return true;
        }
    }
}