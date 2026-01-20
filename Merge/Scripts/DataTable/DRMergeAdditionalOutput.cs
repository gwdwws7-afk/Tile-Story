using System.Collections.Generic;

namespace Merge
{
    /// <summary>
    /// 合成额外产出道具数据表行
    /// </summary>
    public class DRMergeAdditionalOutput : DataRowBase
    {
        private int m_Id;

        /// <summary>
        /// 合成道具等级
        /// </summary>
        public override int Id { get { return m_Id; } }

        /// <summary>
        /// 额外产出道具编号集合
        /// </summary>
        public List<int> AdditionalPropIds
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

            string additionalString = columnStrings[index++];
            if (additionalString != "0")
            {
                string[] splitedPropsId = additionalString.Split('_');
                List<int> tempList = new List<int>();
                for (int i = 0; i < splitedPropsId.Length; i++)
                {
                    tempList.Add(int.Parse(splitedPropsId[i]));
                }
                AdditionalPropIds = tempList;
            }
            else
            {
                AdditionalPropIds = new List<int>();
            }

            return true;
        }
    }
}
