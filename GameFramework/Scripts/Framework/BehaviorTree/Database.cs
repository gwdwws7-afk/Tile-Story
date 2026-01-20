using System.Collections.Generic;

namespace GameFramework.BehaviorTree
{
    /// <summary>
    /// 节点数据类
    /// </summary>
    public class Database
    {
        private Dictionary<string, object> m_Database = new Dictionary<string, object>();

        // Should use dataId as parameter to get data instead of this
        public T GetData<T>(string dataName)
        {
            if (m_Database.TryGetValue(dataName, out object result))
            {
                return (T)result;
            }

            Log.Warning("GetData {0} fail - data not exist", dataName);
            return default;
        }

        public void SetData<T>(string dataName, T data)
        {
            if (m_Database.ContainsKey(dataName))
                m_Database[dataName] = data;
            else
                m_Database.Add(dataName, data);
        }
    }
}