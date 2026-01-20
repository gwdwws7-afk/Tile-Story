using System;

namespace Merge
{
    /// <summary>
    /// 数据提供器接口
    /// </summary>
    /// <typeparam name="T">数据提供器的持有者的类型</typeparam>
    public interface IDataProvider<T>
    {
        /// <summary>
        /// 读取数据成功事件
        /// </summary>
        event EventHandler<ReadDataSuccessEventArgs> ReadDataSuccess;

        /// <summary>
        /// 读取数据失败事件
        /// </summary>
        event EventHandler<ReadDataFailureEventArgs> ReadDataFailure;

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="dataAssetName">数据资源名称</param>
        /// <param name="userData">用户自定义数据</param>
        void ReadData(string dataAssetName, object userData);

        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="dataString">要解析的数据字符串</param>
        /// <returns>是否解析数据成功</returns>
        bool ParseData(string dataString);
    }
}
