using System;

namespace Merge
{
    /// <summary>
    /// 数据提供器
    /// </summary>
    /// <typeparam name="T">数据提供器持有者的类型</typeparam>
    internal sealed class DataProvider<T> : IDataProvider<T>
    {
        private readonly T m_Owner;
        private IDataProviderHelper<T> m_DataProviderHelper;
        private EventHandler<ReadDataSuccessEventArgs> m_ReadDataSuccessEventHandler;
        private EventHandler<ReadDataFailureEventArgs> m_ReadDataFailureEventHandler;

        /// <summary>
        /// 初始化数据提供者的新实例
        /// </summary>
        /// <param name="owner">数据提供者的持有者</param>
        public DataProvider(T owner)
        {
            m_Owner = owner;
            m_DataProviderHelper = null;
            m_ReadDataSuccessEventHandler = null;
            m_ReadDataFailureEventHandler = null;
        }

        /// <summary>
        /// 读取数据成功事件
        /// </summary>
        public event EventHandler<ReadDataSuccessEventArgs> ReadDataSuccess
        {
            add
            {
                m_ReadDataSuccessEventHandler += value;
            }
            remove
            {
                m_ReadDataSuccessEventHandler -= value;
            }
        }

        /// <summary>
        /// 读取数据失败事件
        /// </summary>
        public event EventHandler<ReadDataFailureEventArgs> ReadDataFailure
        {
            add
            {
                m_ReadDataFailureEventHandler += value;
            }
            remove
            {
                m_ReadDataFailureEventHandler -= value;
            }
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="dataAssetName">数据资源名称</param>
        /// <param name="userData">用户自定义数据</param>
        public void ReadData(string dataAssetName, object userData)
        {
            if (m_DataProviderHelper == null)
            {
                throw new Exception("You must set data provider helper first.");
            }

            try
            {
                if (!m_DataProviderHelper.ReadData(m_Owner, dataAssetName, m_ReadDataSuccessEventHandler, m_ReadDataFailureEventHandler, userData))
                {
                    throw new Exception(string.Format("Load data failure in data provider helper, data asset name '{0}'.", dataAssetName));
                }
            }
            catch (Exception exception)
            {
                throw new Exception(string.Format("Can not read data string with exception '{0}'.", exception), exception);
            }
        }

        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="dataString">要解析的数据字符串</param>
        /// <returns>是否解析数据成功</returns>
        public bool ParseData(string dataString)
        {
            if (m_DataProviderHelper == null)
            {
                throw new Exception("You must set data helper first.");
            }

            if (dataString == null)
            {
                throw new Exception("Data string is invalid.");
            }

            try
            {
                return m_DataProviderHelper.ParseData(m_Owner, dataString);
            }
            catch (Exception exception)
            {
                throw new Exception(string.Format("Can not parse data string with exception '{0}'.", exception), exception);
            }
        }

        /// <summary>
        /// 设置数据提供器辅助器
        /// </summary>
        /// <param name="dataProviderHelper">数据提供器辅助器</param>
        internal void SetDataProviderHelper(IDataProviderHelper<T> dataProviderHelper)
        {
            if (dataProviderHelper == null)
            {
                throw new Exception("Data provider helper is invalid.");
            }

            m_DataProviderHelper = dataProviderHelper;
        }
    }
}
