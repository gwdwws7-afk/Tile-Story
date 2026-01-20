using System.Collections.Generic;
using System.Globalization;

namespace Merge
{
    /// <summary>
    /// ����Ԫ��Ȩ�ر���
    /// </summary>
    public class DRGeneratePropWeights : DataRowBase
    {
        private int m_Id;

        /// <summary>
        /// ���ȱ��
        /// </summary>
        public override int Id { get { return m_Id; } }

        /// <summary>
        /// ����Ԫ��Ȩ��
        /// </summary>
        public List<float> GenerateWeights
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

            string weightString = columnStrings[index++];
            if (weightString != "0")
            {
                string[] splitedPropsId = weightString.Split('_');
                List<float> tempList = new List<float>();
                for (int i = 0; i < splitedPropsId.Length; i++)
                {
                    tempList.Add(float.Parse(splitedPropsId[i], CultureInfo.InvariantCulture));
                }
                GenerateWeights = tempList;
            }
            else
            {
                GenerateWeights = new List<float>();
            }
            return true;
        }
    }
}