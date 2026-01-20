using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    /// <summary>
    /// 可生成道具数据表行
    /// </summary>
    public class DRCanGenerateProp : DataRowBase
    {
        private int m_Id;

        /// <summary>
        /// 道具编号
        /// </summary>
        public override int Id { get { return m_Id; } }

        /// <summary>
        /// 可生成道具编号集合
        /// </summary>
        public List<int> CanGeneratePropIds
        {
            get;
            private set;
        }

        /// <summary>
        /// 可生成道具数量
        /// </summary>
        public List<int> CanGeneratePropNum
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

            string generateIdString = columnStrings[index++];
            if (generateIdString != "0")
            {
                string[] splitedPropsId = generateIdString.Split('_');
                List<int> tempList = new List<int>();
                for (int i = 0; i < splitedPropsId.Length; i++)
                {
                    tempList.Add(int.Parse(splitedPropsId[i]));
                }
                CanGeneratePropIds = tempList;
            }
            else
            {
                CanGeneratePropIds = new List<int>();
            }

            string probabilityString = columnStrings[index++];
            if (probabilityString != "0")
            {
                string[] splitedPropsId = probabilityString.Split('_');
                List<int> tempList = new List<int>();
                for (int i = 0; i < splitedPropsId.Length; i++)
                {
                    tempList.Add(int.Parse(splitedPropsId[i]));
                }
                CanGeneratePropNum = tempList;
            }
            else
            {
                CanGeneratePropNum = new List<int>();
            }

            return true;
        }
    }
}
