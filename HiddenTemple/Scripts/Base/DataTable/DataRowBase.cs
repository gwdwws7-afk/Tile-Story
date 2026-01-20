namespace HiddenTemple
{
    /// <summary>
    /// 数据表行基类
    /// </summary>
    public abstract class DataRowBase : IDataRow
    {
        /// <summary>
        /// 数据表行的编号
        /// </summary>
        public abstract int Id { get; }

        /// <summary>
        /// 解析数据表行
        /// </summary>
        /// <param name="dataRowString">要解析的数据表行字符串</param>
        /// <returns>是否解析数据表行成功</returns>
        public virtual bool ParseDataRow(string dataRowString)
        {
            Log.Warning("Not implemented ParseDataRow(string dataRowString, object userData).");
            return false;
        }
    }
}