using System.Collections.Generic;

namespace HiddenTemple
{
    /// <summary>
    /// 数据表管理器
    /// </summary>
    public sealed partial class DataTableManager : GameFrameworkModule, IDataTableManager
    {
        private readonly Dictionary<TypeNamePair, DataTableBase> m_DataTables;
        private IDataProviderHelper<DataTableBase> m_DataProviderHelper;

        public DataTableManager()
        {
            m_DataTables = new Dictionary<TypeNamePair, DataTableBase>();
            m_DataProviderHelper = null;
        }

        /// <summary>
        /// 数据表数量
        /// </summary>
        public int Count { get => m_DataTables.Count; }

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        public override void Shutdown()
        {
            foreach (var dataTable in m_DataTables)
            {
                dataTable.Value.Shutdown();
            }
            m_DataTables.Clear();
        }

        /// <summary>
        /// 设置数据表数据提供器辅助器
        /// </summary>
        /// <param name="dataProviderHelper">数据表数据提供器辅助器</param>
        public void SetDataProviderHelper(IDataProviderHelper<DataTableBase> dataProviderHelper)
        {
            if (dataProviderHelper == null)
            {
                throw new System.Exception("Data provider helper is invalid.");
            }

            m_DataProviderHelper = dataProviderHelper;
        }

        /// <summary>
        /// 是否存在数据表
        /// </summary>
        /// <typeparam name="T">数据表行的类型</typeparam>
        /// <returns>是否存在数据表</returns>
        public bool HasDataTable<T>() where T : class
        {
            return InternalHasDataTable(new TypeNamePair(typeof(T)));
        }

        /// <summary>
        /// 是否存在数据表
        /// </summary>
        /// <typeparam name="T">数据表行的类型</typeparam>
        /// <param name="name">数据表名称</param>
        /// <returns>是否存在数据表</returns>
        public bool HasDataTable<T>(string name) where T : class
        {
            return InternalHasDataTable(new TypeNamePair(typeof(T), name));
        }

        /// <summary>
        /// 获取数据表
        /// </summary>
        /// <typeparam name="T">数据表行的类型</typeparam>
        /// <returns>要获取的数据表</returns>
        public IDataTable<T> GetDataTable<T>() where T : class
        {
            return (IDataTable<T>)InternalGetDataTable(new TypeNamePair(typeof(T)));
        }

        /// <summary>
        /// 获取数据表
        /// </summary>
        /// <typeparam name="T">数据表行的类型</typeparam>
        /// <param name="name">数据表名称</param>
        /// <returns>要获取的数据表</returns>
        public IDataTable<T> GetDataTable<T>(string name) where T : class
        {
            return (IDataTable<T>)InternalGetDataTable(new TypeNamePair(typeof(T), name));
        }

        /// <summary>
        /// 获取所有数据表
        /// </summary>
        /// <returns>所有数据表</returns>
        public DataTableBase[] GetAllDataTables()
        {
            int index = 0;
            DataTableBase[] results = new DataTableBase[m_DataTables.Count];
            foreach (KeyValuePair<TypeNamePair, DataTableBase> dataTable in m_DataTables)
            {
                results[index++] = dataTable.Value;
            }

            return results;
        }

        /// <summary>
        /// 获取所有数据表
        /// </summary>
        /// <param name="results">所有数据表</param>
        public void GetAllDataTables(List<DataTableBase> results)
        {
            if (results == null)
            {
                throw new System.Exception("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<TypeNamePair, DataTableBase> dataTable in m_DataTables)
            {
                results.Add(dataTable.Value);
            }
        }

        /// <summary>
        /// 创建DataRow数据表
        /// </summary>
        /// <typeparam name="T">数据表行的类型</typeparam>
        /// <returns>要创建的数据表</returns>
        public IDataTable<T> CreateDataRowDataTable<T>() where T : class, IDataRow, new()
        {
            return CreateDataRowDataTable<T>(string.Empty);
        }

        /// <summary>
        /// 创建DataRow数据表
        /// </summary>
        /// <typeparam name="T">数据表行的类型</typeparam>
        /// <param name="name">数据表名称</param>
        /// <returns>要创建的数据表</returns>
        public IDataTable<T> CreateDataRowDataTable<T>(string name) where T : class, IDataRow, new()
        {
            if (m_DataProviderHelper == null)
            {
                throw new System.Exception("You must set data provider helper first.");
            }

            TypeNamePair typeNamePair = new TypeNamePair(typeof(T), name);
            if (HasDataTable<T>(name))
            {
                throw new System.Exception(string.Format("Already exist data table '{0}'.", typeNamePair));
            }

            DataTable<T> dataTable = new DataTable<T>(name);
            dataTable.SetDataProviderHelper(m_DataProviderHelper);
            m_DataTables.Add(typeNamePair, dataTable);
            return dataTable;
        }

        /// <summary>
        /// 销毁数据表
        /// </summary>
        /// <typeparam name="T">数据表行的类型</typeparam>
        public bool DestroyDataTable<T>() where T : class
        {
            return InternalDestroyDataTable(new TypeNamePair(typeof(T)));
        }

        /// <summary>
        /// 销毁数据表
        /// </summary>
        /// <typeparam name="T">数据表行的类型</typeparam>
        /// <param name="name">数据表名称</param>
        public bool DestroyDataTable<T>(string name) where T : class
        {
            return InternalDestroyDataTable(new TypeNamePair(typeof(T), name));
        }

        private bool InternalHasDataTable(TypeNamePair typeNamePair)
        {
            return m_DataTables.ContainsKey(typeNamePair);
        }

        private DataTableBase InternalGetDataTable(TypeNamePair typeNamePair)
        {
            if (m_DataTables.TryGetValue(typeNamePair, out DataTableBase dataTable))
            {
                return dataTable;
            }
            return null;
        }

        private bool InternalDestroyDataTable(TypeNamePair typeNamePair)
        {
            if (m_DataTables.TryGetValue(typeNamePair, out DataTableBase dataTable))
            {
                dataTable.Shutdown();
                return m_DataTables.Remove(typeNamePair);
            }
            return false;
        }
    }
}
