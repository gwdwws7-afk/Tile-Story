

namespace Merge
{
    /// <summary>
    /// 道具数据表行
    /// </summary>
    public class DRProp : DataRowBase
    {
        private int m_Id;

        /// <summary>
        /// 道具编号
        /// </summary>
        public override int Id { get { return m_Id; } }

        /// <summary>
        /// 资源名称
        /// </summary>
        public string AssetName
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
            AssetName = columnStrings[index++];

            return true;
        }
    }
}