using System;

namespace Merge
{
    /// <summary>
    /// 数据表基类
    /// </summary>
    public abstract class DataTableBase
    {
        private readonly string m_Name;
        private readonly DataProvider<DataTableBase> m_DataProvider;

        public DataTableBase()
            : this(null)
        {
        }

        public DataTableBase(string name)
        {
            m_Name = name ?? string.Empty;
            m_DataProvider = new DataProvider<DataTableBase>(this);
        }

        /// <summary>
        /// 数据表名称
        /// </summary>
        public string Name { get => m_Name; }

        /// <summary>
        /// 数据表完整名称
        /// </summary>
        public string FullName { get => new TypeNamePair(Type, m_Name).ToString(); }

        /// <summary>
        /// 数据表行的类型
        /// </summary>
        public abstract Type Type { get; }

        /// <summary>
        /// 数据表行数
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// 读取数据成功事件
        /// </summary>
        public event EventHandler<ReadDataSuccessEventArgs> ReadDataSuccess
        {
            add
            {
                m_DataProvider.ReadDataSuccess += value;
            }
            remove
            {
                m_DataProvider.ReadDataSuccess -= value;
            }
        }

        /// <summary>
        /// 读取数据失败事件
        /// </summary>
        public event EventHandler<ReadDataFailureEventArgs> ReadDataFailure
        {
            add
            {
                m_DataProvider.ReadDataFailure += value;
            }
            remove
            {
                m_DataProvider.ReadDataFailure -= value;
            }
        }

        /// <summary>
        /// 读取数据表
        /// </summary>
        /// <param name="dataTableAssetName">数据表资源名称</param>
        /// <param name="userData">用户自定义数据</param>
        public void ReadData(string dataTableAssetName, object userData)
        {
            m_DataProvider.ReadData(dataTableAssetName, userData);
        }

        /// <summary>
        /// 解析数据表
        /// </summary>
        /// <param name="dataTableString">要解析的数据表字符串</param>
        /// <returns>是否解析数据表成功</returns>
        public bool ParseData(string dataTableString)
        {
            return m_DataProvider.ParseData(dataTableString);
        }

        /// <summary>
        /// 检查是否存在数据表行
        /// </summary>
        /// <param name="id">数据表行的编号</param>
        /// <returns>是否存在数据表行</returns>
        public abstract bool HasDataRow(int id);

        /// <summary>
        /// 增加数据表行
        /// </summary>
        /// <param name="rawData">数据表数据</param>
        /// <returns>是否增加数据表数据成功</returns>
        public abstract bool AddDataRow(object rawData);

        /// <summary>
        /// 移除指定数据表行
        /// </summary>
        /// <param name="id">要移除数据表行的编号</param>
        /// <returns>是否移除数据表行成功</returns>
        public abstract bool RemoveDataRow(int id);

        /// <summary>
        /// 清空所有的数据表行
        /// </summary>
        public abstract void RemoveAllDataRows();

        /// <summary>
        /// 设置数据提供者辅助器
        /// </summary>
        /// <param name="dataProviderHelper">数据提供者辅助器</param>
        public void SetDataProviderHelper(IDataProviderHelper<DataTableBase> dataProviderHelper)
        {
            m_DataProvider.SetDataProviderHelper(dataProviderHelper);
        }

        /// <summary>
        /// 关闭并清理数据表
        /// </summary>
        public abstract void Shutdown();
    }
}