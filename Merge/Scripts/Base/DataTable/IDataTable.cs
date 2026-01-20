using System;
using System.Collections.Generic;

namespace Merge
{
    /// <summary>
    /// 数据表接口
    /// </summary>
    /// <typeparam name="T">数据表行的类型</typeparam>
    public interface IDataTable<T> : IEnumerable<T>
    {
        /// <summary>
        /// 数据表名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 数据表完整名称
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// 数据表行的类型
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// 数据表行数
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 获取数据表行
        /// </summary>
        /// <param name="id">数据表行的编号</param>
        /// <returns>数据表行</returns>
        T this[int id] { get; }

        /// <summary>
        /// 编号最小的数据表行
        /// </summary>
        T MinIdDataRow { get; }

        /// <summary>
        /// 编号最大的数据表行
        /// </summary>
        T MaxIdDataRow { get; }

        /// <summary>
        /// 读取数据表
        /// </summary>
        /// <param name="dataTableAssetName">数据表资源名称</param>
        /// <param name="userData">用户自定义数据</param>
        void ReadData(string dataTableAssetName, object userData);

        /// <summary>
        /// 检查是否存在数据表行
        /// </summary>
        /// <param name="id">数据表行的编号</param>
        /// <returns>是否存在数据表行</returns>
        bool HasDataRow(int id);

        /// <summary>
        /// 获取数据表行
        /// </summary>
        /// <param name="id">数据表行的编号</param>
        /// <returns>数据表行</returns>
        T GetDataRow(int id);

        /// <summary>
        /// 获取所有数据表行
        /// </summary>
        /// <returns>所有数据表行</returns>
        T[] GetAllDataRows();
    }
}