using System;

namespace Merge
{
    /// <summary>
    /// 数据表辅助器基类
    /// </summary>
    public abstract class DataTableHelperBase : IDataProviderHelper<DataTableBase>
    {
        /// <summary>
        /// 读取数据表
        /// </summary>
        /// <param name="dataTable">数据表</param>
        /// <param name="dataTableAssetName">数据表资源名称</param>
        /// <param name="ReadDataTableSuccess">读取数据表成功事件</param>
        /// <param name="ReadDataTableFailure">读取数据表失败事件</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>是否读取数据表成功</returns>
        public abstract bool ReadData(DataTableBase dataTable, string dataTableAssetName, EventHandler<ReadDataSuccessEventArgs> ReadDataTableSuccess, EventHandler<ReadDataFailureEventArgs> ReadDataTableFailure, object userData);

        /// <summary>
        /// 解析数据表
        /// </summary>
        /// <param name="dataTable">数据表</param>
        /// <param name="dataTableString">要解析的数据表字符串</param>
        /// <returns>是否解析数据表成功</returns>
        public abstract bool ParseData(DataTableBase dataTable, string dataTableString);
    }
}