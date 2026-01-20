using System;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace HiddenTemple
{
    /// <summary>
    /// 数据表模块
    /// </summary>
    public sealed class DataTableComponent : MonoBehaviour
    {
        private IDataTableManager m_DataTableManager;

        /// <summary>
        /// 数据表数量
        /// </summary>
        public int Count { get => m_DataTableManager.Count; }

        private void Awake()
        {
            m_DataTableManager = new DataTableManager();

            //载入数据表数据提供者辅助器
            var dataTableHelper = new DefaultDataTableHelper();
            m_DataTableManager.SetDataProviderHelper(dataTableHelper);
        }

        /// <summary>
        /// 是否存在数据表
        /// </summary>
        /// <typeparam name="T">数据表行的类型</typeparam>
        /// <returns>是否存在数据表</returns>
        public bool HasDataTable<T>() where T : class
        {
            return m_DataTableManager.HasDataTable<T>();
        }

        /// <summary>
        /// 是否存在数据表
        /// </summary>
        /// <typeparam name="T">数据表行的类型</typeparam>
        /// <param name="name">数据表名称</param>
        /// <returns>是否存在数据表</returns>
        public bool HasDataTable<T>(string name) where T : class
        {
            return m_DataTableManager.HasDataTable<T>(name);
        }

        /// <summary>
        /// 获取数据表
        /// </summary>
        /// <typeparam name="T">数据表行的类型</typeparam>
        /// <returns>要获取的数据表</returns>
        public IDataTable<T> GetDataTable<T>() where T : class
        {
            return m_DataTableManager.GetDataTable<T>();
        }

        /// <summary>
        /// 获取数据表
        /// </summary>
        /// <typeparam name="T">数据表行的类型</typeparam>
        /// <param name="name">数据表名称</param>
        /// <returns>要获取的数据表</returns>
        public IDataTable<T> GetDataTable<T>(string name) where T : class
        {
            return m_DataTableManager.GetDataTable<T>(name);
        }

        /// <summary>
        /// 获取所有数据表
        /// </summary>
        /// <returns>所有数据表</returns>
        public DataTableBase[] GetAllDataTables()
        {
            return m_DataTableManager.GetAllDataTables();
        }

        /// <summary>
        /// 获取所有数据表
        /// </summary>
        /// <param name="results">所有数据表</param>
        public void GetAllDataTables(List<DataTableBase> results)
        {
            m_DataTableManager.GetAllDataTables(results);
        }

        /// <summary>
        /// 创建DataRow数据表
        /// </summary>
        /// <typeparam name="T">数据表行的类型</typeparam>
        /// <returns>要创建的数据表</returns>
        public IDataTable<T> CreateDataRowDataTable<T>() where T : class, IDataRow, new()
        {
            IDataTable<T> dataTable = m_DataTableManager.CreateDataRowDataTable<T>();
            DataTableBase dataTableBase = (DataTableBase)dataTable;
            dataTableBase.ReadDataSuccess += OnReadDataSuccess;
            dataTableBase.ReadDataFailure += OnReadDataFailure;
            return dataTable;
        }

        /// <summary>
        /// 创建DataRow数据表
        /// </summary>
        /// <typeparam name="T">数据表行的类型</typeparam>
        /// <param name="name">数据表名称</param>
        /// <returns>要创建的数据表</returns>
        public IDataTable<T> CreateDataRowDataTable<T>(string name) where T : class, IDataRow, new()
        {
            IDataTable<T> dataTable = m_DataTableManager.CreateDataRowDataTable<T>(name);
            DataTableBase dataTableBase = (DataTableBase)dataTable;
            dataTableBase.ReadDataSuccess += OnReadDataSuccess;
            dataTableBase.ReadDataFailure += OnReadDataFailure;
            return dataTable;
        }

        /// <summary>
        /// 销毁数据表
        /// </summary>
        /// <typeparam name="T">数据表行的类型</typeparam>
        public bool DestroyDataTable<T>() where T : class
        {
            return m_DataTableManager.DestroyDataTable<T>();
        }

        /// <summary>
        /// 销毁数据表
        /// </summary>
        /// <typeparam name="T">数据表行的类型</typeparam>
        /// <param name="name">数据表名称</param>
        public bool DestroyDataTable<T>(string name) where T : class
        {
            return m_DataTableManager.DestroyDataTable<T>(name);
        }

        private void OnReadDataSuccess(object sender, ReadDataSuccessEventArgs e)
        {
            GameManager.GetGameComponent<EventComponent>().Fire(this, LoadDataTableSuccessEventArgs.Create(e));
        }

        private void OnReadDataFailure(object sender, ReadDataFailureEventArgs e)
        {
            Log.Warning("Load data table failure, asset name '{0}', error message '{1}'.", e.DataAssetName, e.ErrorMessage);
            GameManager.GetGameComponent<EventComponent>().Fire(this, LoadDataTableFailureEventArgs.Create(e));
        }
    }
}