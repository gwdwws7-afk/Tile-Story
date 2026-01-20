using System;

namespace Merge
{
    public class DRMergeSchedule : DataRowBase
    {
        private int m_Id;

        /// <summary>
        /// 周期编号
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

        /// <summary>
        /// 主题
        /// </summary>
        public MergeTheme Theme
        {
            get;
            private set;
        }

        /// <summary>
        /// 最大道具编号
        /// </summary>
        public int MaxPropId
        {
            get;
            private set;
        }

        /// <summary>
        /// 大奖棋子编号
        /// </summary>
        public int TileId
        {
            get;
            private set;
        }

        /// <summary>
        /// 大奖棋子奖励编号
        /// </summary>
        public int TileRewardId
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
            Theme = (MergeTheme)int.Parse(columnStrings[index++]);
            MaxPropId = int.Parse(columnStrings[index++]);
            TileId = int.Parse(columnStrings[index++]);
            TileRewardId = int.Parse(columnStrings[index++]);

            return true;
        }
    }
}
