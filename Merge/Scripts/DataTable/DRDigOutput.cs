using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class DRDigOutput : DataRowBase
    {
        private int m_Id;

        /// <summary>
        /// 土块编号
        /// </summary>
        public override int Id { get { return m_Id; } }

        /// <summary>
        ///  产出道具（第一铲）
        /// </summary>
        public List<int> Output1PropIds
        {
            get;
            private set;
        }

        /// <summary>
        ///  产出道具（第二铲）
        /// </summary>
        public List<int> Output2PropIds
        {
            get;
            private set;
        }

        /// <summary>
        ///  产出道具（第三铲）
        /// </summary>
        public List<int> Output3PropIds
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

            if (index < columnStrings.Length) 
            {
                string idString = columnStrings[index++];
                string[] splitedPropsId = idString.Split('_');
                List<int> tempList = new List<int>();
                for (int i = 0; i < splitedPropsId.Length; i++)
                {
                    tempList.Add(int.Parse(splitedPropsId[i]));
                }
                Output1PropIds = tempList;
            }

            if (index < columnStrings.Length)
            {
                string idString = columnStrings[index++];
                if (!string.IsNullOrEmpty(idString))
                {
                    string[] splitedPropsId = idString.Split('_');
                    List<int> tempList = new List<int>();
                    for (int i = 0; i < splitedPropsId.Length; i++)
                    {
                        tempList.Add(int.Parse(splitedPropsId[i]));
                    }
                    Output2PropIds = tempList;
                }
            }

            if (index < columnStrings.Length)
            {
                string idString = columnStrings[index++];
                if (!string.IsNullOrEmpty(idString))
                {
                    string[] splitedPropsId = idString.Split('_');
                    List<int> tempList = new List<int>();
                    for (int i = 0; i < splitedPropsId.Length; i++)
                    {
                        tempList.Add(int.Parse(splitedPropsId[i]));
                    }
                    Output3PropIds = tempList;
                }
            }

            return true;
        }
    }
}
