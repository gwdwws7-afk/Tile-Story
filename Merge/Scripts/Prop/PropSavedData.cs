using GameFramework;
using System.Collections.Generic;
using System.Text;

namespace Merge
{
    public class PropSavedData : IReference
    {
        private Dictionary<string, string> m_SavedDataDic = new Dictionary<string, string>();

        /// <summary>
        /// 保存的数据的数量
        /// </summary>
        public int Count { get { return m_SavedDataDic.Count; } }

        /// <summary>
        /// 将保存数据转化为字符串形式
        /// </summary>
        /// <returns>保存数据的字符串</returns>
        public virtual string Save()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var data in m_SavedDataDic)
            {
                sb.Append(data.Key);
                sb.Append("|");
                sb.Append(data.Value);
                sb.Append("_");
            }
            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }

        /// <summary>
        /// 将保存数据的字符串转化为数据形式
        /// </summary>
        /// <param name="savedString">保存数据的字符串</param>
        public virtual void Load(string savedString)
        {
            string[] keyValuePairString = savedString.Split('_');
            for (int i = 0; i < keyValuePairString.Length; i++)
            {
                    string[] pairString = keyValuePairString[i].Split('|');
                    m_SavedDataDic.Add(pairString[0], pairString[1]);
            }
        }

        public bool HasData(string key)
        {
            return m_SavedDataDic.ContainsKey(key);
        }

        public string GetData(string key)
        {
            string result = null;
            if (m_SavedDataDic.TryGetValue(key, out result))
            {
                return result;
            }

            return null;
        }

        public void SetData(string key, string value)
        {
            if (m_SavedDataDic.ContainsKey(key))
            {
                Log.Warning("SavedData SetData() - Key {0} already have.", key);
            }

            if (string.IsNullOrEmpty(value) || value.Contains("|") || value.Contains("_"))
            {
                Log.Error("SavedData SetData() - Key {0} 's Value {1} is invalid.", key, value);
                return;
            }

            m_SavedDataDic[key] = value;
        }

        public void DeleteData(string key)
        {
            if (m_SavedDataDic.ContainsKey(key))
            {
                m_SavedDataDic.Remove(key);
            }
        }

        public virtual void Clear()
        {
            m_SavedDataDic.Clear();
        }
    }
}