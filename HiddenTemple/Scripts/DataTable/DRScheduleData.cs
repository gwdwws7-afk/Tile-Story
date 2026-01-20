using System;

namespace HiddenTemple
{
    /// <summary>
    /// 活动周期数据表行
    /// </summary>
    public class DRScheduleData : DataRowBase
    {
        private int m_Id;

        /// <summary>
        /// 期数编号
        /// </summary>
        public override int Id { get { return m_Id; } }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime
        {
            get;
            private set;
        }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime
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
            StartTime = DateTime.ParseExact(columnStrings[index++], "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            EndTime = DateTime.ParseExact(columnStrings[index++], "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

            return true;
        }
    }
}
