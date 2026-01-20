using System.Collections.Generic;

namespace Merge
{
    /// <summary>
    /// 数据表管理器接口
    /// </summary>
    public interface IDataTableManager
    {
        /// <summary>
        /// 获取数据表数量
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 设置数据表数据提供器辅助器
        /// </summary>
        /// <param name="dataProviderHelper">数据表数据提供器辅助器</param>
        void SetDataProviderHelper(IDataProviderHelper<DataTableBase> dataProviderHelper);

        /// <summary>
        /// 是否存在数据表
        /// </summary>
        /// <typeparam name="T">数据表行的类型</typeparam>
        /// <returns>是否存在数据表</returns>
        bool HasDataTable<T>() where T : class;

        /// <summary>
        /// 是否存在数据表
        /// </summary>
        /// <typeparam name="T">数据表行的类型</typeparam>
        /// <param name="name">数据表名称</param>
        /// <returns>是否存在数据表</returns>
        bool HasDataTable<T>(string name) where T : class;

        /// <summary>
        /// 获取数据表
        /// </summary>
        /// <typeparam name="T">数据表行的类型</typeparam>
        /// <returns>要获取的数据表</returns>
        IDataTable<T> GetDataTable<T>() where T : class;

        /// <summary>
        /// 获取数据表
        /// </summary>
        /// <typeparam name="T">数据表行的类型</typeparam>
        /// <param name="name">数据表名称</param>
        /// <returns>要获取的数据表</returns>
        IDataTable<T> GetDataTable<T>(string name) where T : class;

        /// <summary>
        /// 获取所有数据表
        /// </summary>
        /// <returns>所有数据表</returns>
        DataTableBase[] GetAllDataTables();

        /// <summary>
        /// 获取所有数据表
        /// </summary>
        /// <param name="results">所有数据表</param>
        void GetAllDataTables(List<DataTableBase> results);

        /// <summary>
        /// 创建DataRow数据表
        /// </summary>
        /// <typeparam name="T">数据表行的类型</typeparam>
        /// <returns>要创建的数据表</returns>
        IDataTable<T> CreateDataRowDataTable<T>() where T : class, IDataRow, new();

        /// <summary>
        /// 创建DataRow数据表
        /// </summary>
        /// <typeparam name="T">数据表行的类型</typeparam>
        /// <param name="name">数据表名称</param>
        /// <returns>要创建的数据表</returns>
        IDataTable<T> CreateDataRowDataTable<T>(string name) where T : class, IDataRow, new();

        /// <summary>
        /// 销毁数据表
        /// </summary>
        /// <typeparam name="T">数据表行的类型</typeparam>
        bool DestroyDataTable<T>() where T : class;

        /// <summary>
        /// 销毁数据表
        /// </summary>
        /// <typeparam name="T">数据表行的类型</typeparam>
        /// <param name="name">数据表名称</param>
        bool DestroyDataTable<T>(string name) where T : class;
    }
}