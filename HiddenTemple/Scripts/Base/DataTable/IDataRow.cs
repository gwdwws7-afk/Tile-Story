namespace HiddenTemple
{
    /// <summary>
    /// 数据表行接口
    /// </summary>
    public interface IDataRow
    {
        /// <summary>
        /// 数据表行的编号
        /// </summary>
        int Id { get; }

        /// <summary>
        /// 解析数据表行
        /// </summary>
        /// <param name="dataRowString">要解析的数据表行字符串</param>
        /// <returns>是否解析数据表行成功</returns>
        bool ParseDataRow(string dataRowString);
    }
}