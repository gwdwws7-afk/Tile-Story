using System;

namespace Merge
{
    /// <summary>
    /// 道具分布表行
    /// </summary>
    public class DRPropDistributedMap : DataRowBase
    {
        private int m_Row;
        private int[] m_PropsId;
        private int[] m_AttachmentsId;

        public override int Id { get { return m_Row; } }

        public int[] PropsId { get { return m_PropsId; } }

        public int[] AttachmentsId { get { return m_AttachmentsId; } }

        public override bool ParseDataRow(string dataRowString)
        {
            string[] columnStrings = dataRowString.Split(DataTableExtension.DataSplitSeparators);
            for (int i = 0; i < columnStrings.Length; i++)
            {
                columnStrings[i] = columnStrings[i].Trim(DataTableExtension.DataTrimSeparators);
            }

            int index = 0;
            index++;
            m_Row = int.Parse(columnStrings[index++]);
            m_PropsId = new int[columnStrings.Length - 2];
            m_AttachmentsId = new int[columnStrings.Length - 2];

            for (int i = index; i < columnStrings.Length; i++)
            {
                string[] splitedString = columnStrings[i].Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                m_PropsId[i - 2] = int.Parse(splitedString[0]);
                if (splitedString.Length > 1)
                {
                    m_AttachmentsId[i - 2] = int.Parse(splitedString[1]);
                }
            }

            return true;
        }
    }
}