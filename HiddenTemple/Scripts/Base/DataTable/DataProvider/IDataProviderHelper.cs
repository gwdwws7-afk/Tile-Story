using System;

namespace HiddenTemple
{
    /// <summary>
    /// 数据提供器辅助器接口
    /// </summary>
    /// <typeparam name="T">数据提供器持有者的类型</typeparam>
    public interface IDataProviderHelper<T>
    {
        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="dataProviderOwner">数据提供器持有者</param>
        /// <param name="dataAssetName">数据资源名称</param>
        /// <param name="ReadDataSuccess">读取数据成功事件</param>
        /// <param name="ReadDataFailure">读取数据失败事件</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>是否读取数据成功</returns>
        bool ReadData(T dataProviderOwner, string dataAssetName, EventHandler<ReadDataSuccessEventArgs> ReadDataSuccess, EventHandler<ReadDataFailureEventArgs> ReadDataFailure, object userData);

        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="dataProviderOwner">数据提供器持有者</param>
        /// <param name="dataString">要解析的数据字符串</param>
        /// <returns>是否解析数据成功</returns>
        bool ParseData(T dataProviderOwner, string dataString);
    }
}
