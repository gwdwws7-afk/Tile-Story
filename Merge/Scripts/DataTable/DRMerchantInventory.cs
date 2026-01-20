using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Merge
{
    public class DRMerchantInventory : DataRowBase
    {
        private int m_Id;

        /// <summary>
        /// Merchant level
        /// </summary>
        public override int Id { get { return m_Id; } }

        /// <summary>
        /// Merchant unit inventory value
        /// </summary>
        public int[] UnitValueArray
        {
            get;
            private set;
        }

        /// <summary>
        /// Total merchant unit inventory value
        /// </summary>
        public int TotalUnitValue
        {
            get;
            private set;
        }

        /// <summary>
        /// range of Reduce random value (every single small gem)
        /// </summary>
        public int[] ReduceRange
        {
            get;
            private set;
        }

        /// <summary>
        /// Single critical hit probability (maximum 1 critical hit per group of hits)
        /// </summary>
        public float CriticalHitProbability
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

            TotalUnitValue = 0;
            string unitValueArrayString = columnStrings[index++];
            string[] splitedUnitString = unitValueArrayString.Split('_');
            UnitValueArray = new int[splitedUnitString.Length];
            for (int i = 0; i < splitedUnitString.Length; i++)
            {
                int value = int.Parse(splitedUnitString[i]);
                TotalUnitValue += value;
                UnitValueArray[i] = TotalUnitValue;
            }

            string rangeString = columnStrings[index++];
            string[] splitedRangeString = rangeString.Split('_');
            ReduceRange = new int[splitedRangeString.Length];
            for (int i = 0; i < splitedRangeString.Length; i++)
            {
                ReduceRange[i] = int.Parse(splitedRangeString[i]);
            }

            CriticalHitProbability = float.Parse(columnStrings[index++], CultureInfo.InvariantCulture);

            return true;
        }
    }
}
